using Flurl;
using Flurl.Http;
using O10.Core;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.Exceptions;
using O10.Core.ExtensionMethods;
using O10.Core.HashCalculations;
using O10.Core.Logging;
using O10.Core.Models;
using O10.Core.Notifications;
using O10.Gateway.Common.Configuration;
using O10.Gateway.Common.Services.Results;
using O10.Transactions.Core.Accessors;
using O10.Transactions.Core.Ledgers.Stealth;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace O10.Gateway.Common.Services
{
    [RegisterDefaultImplementation(typeof(ITransactionsHandler), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionsHandler : ITransactionsHandler
    {
        private readonly IPropagatorBlock<PacketBase, PacketBase> _pipeInPacket;
        private readonly IPropagatorBlock<PacketBase, DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase>> _pipeEvidence;
        private readonly IPropagatorBlock<TaskCompletionWrapper<PacketBase>, TaskCompletionWrapper<PacketBase>> _pipeOutPacket;
        private readonly IHashCalculation _hashCalculation;
        private readonly ILogger _logger;
        private readonly ISynchronizerConfiguration _synchronizerConfiguration;
        private readonly ISerializersFactory _serializersFactory;
        private readonly Dictionary<PacketBase, TaskCompletionSource<NotificationBase>> _completions = new Dictionary<PacketBase, TaskCompletionSource<NotificationBase>>();
        //private readonly Dictionary<PacketBase, Task> _waitingTasks = new Dictionary<PacketBase, Task>();
        //private readonly Dictionary<PacketBase, DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase>> _dependings = new Dictionary<PacketBase, DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase>>();

        public TransactionsHandler(IHashCalculationsRepository hashCalculationsRepository,
                                   IConfigurationService configurationService,
                                   ISerializersFactory serializersFactory,
                                   ILoggerService loggerService)
        {
            _logger = loggerService.GetLogger(nameof(TransactionsHandler));
            _synchronizerConfiguration = configurationService.Get<ISynchronizerConfiguration>();

            _hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
            _pipeInPacket = new TransformBlock<PacketBase, PacketBase>(p => p);
            _pipeEvidence = new TransformBlock<PacketBase, DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase>>(p => ProduceEvidenceAndSend(p));
            _pipeOutPacket = new TransformBlock<TaskCompletionWrapper<PacketBase>, TaskCompletionWrapper<PacketBase>>(p => p);
            
            _pipeInPacket.LinkTo(_pipeEvidence, ValidatePacket);
            _serializersFactory = serializersFactory;
        }

        public TaskCompletionSource<NotificationBase> SendPacket(PacketBase packetBase)
        {
            TaskCompletionSource<NotificationBase> taskCompletion = new TaskCompletionSource<NotificationBase>(packetBase);
            _completions.Add(packetBase, taskCompletion);

            _pipeInPacket.SendAsync(packetBase);

            return taskCompletion;
        }

        public ISourceBlock<T> GetSourcePipe<T>(string name = null)
        {
            if(typeof(T) == typeof(DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase>))
            {
                return (ISourceBlock<T>)_pipeEvidence;
            }
            else if(typeof(T) == typeof(TaskCompletionWrapper<PacketBase>))
            {
                return (ISourceBlock<T>)_pipeOutPacket;
            }

            throw new InvalidSourcePipeRequestException(typeof(T));
        }

        public ITargetBlock<T> GetTargetPipe<T>(string name = null)
        {
            if (typeof(T) == typeof(PacketBase))
            {
                return (ITargetBlock<T>)_pipeEvidence;
            }

            throw new InvalidTargetPipeRequestException(typeof(T));
        }

        private bool ValidatePacket(PacketBase packet)
        {
            bool res = true;
            string existingHash = null;

            if (packet is StealthBase stealth)
            {
                var keyImage = stealth.KeyImage.ToString();

                _logger.LogIfDebug(() => $"Sending to Node {_synchronizerConfiguration.NodeApiUri} request for finding hash by a key image {keyImage}");
                var packetHashResponse = AsyncUtil.RunSync(async () => await _synchronizerConfiguration.NodeApiUri.AppendPathSegments("HashByKeyImage", keyImage).GetJsonAsync<PacketHashResponse>().ConfigureAwait(false));


                if (!string.IsNullOrEmpty(packetHashResponse.Hash))
                {
                    _logger.Error($"It was found that key image {keyImage} already was witnessed for the packet with hash {packetHashResponse.Hash}");
                    res = false;
                    existingHash = packetHashResponse.Hash;
                    _completions[packet].SetResult(new KeyImageViolatedNotification { ExistingHash = existingHash });
                    _completions.Remove(packet);
                }
            }
            else if(packet.LedgerType != (ushort)LedgerType.O10State)
            {
                _logger.Error($"Packets of the type {packet.GetType().Name} are not supported");
                res = false;
                _completions[packet].SetResult(new FailedNotification());
                _completions.Remove(packet);
            }

            return res;
        }

        private DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase> ProduceEvidenceAndSend(PacketBase packet)
        {
            TaskCompletionWrapper<PacketBase> wrapper = new TaskCompletionWrapper<PacketBase>(packet);

            if(packet.RawData.Length == 0)
            {
                using var serializer = _serializersFactory.Create(packet);
                serializer.SerializeFully();
            }

            EvidenceDescriptor evidenceDescriptor = new EvidenceDescriptor
            {
                ActionType = packet.PacketType,
                LedgerType = (LedgerType)packet.LedgerType,
                Parameters = new Dictionary<string, string> { { "BodyHash", _hashCalculation.CalculateHash(packet.RawData).ToHexString() } }
            };

            DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase> depending 
                = new DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase>(evidenceDescriptor, wrapper);

            StartWaitingForDependingCompletion(depending);
            //_dependings.Add(packet, depending);
            //_waitingTasks.Add(packet, StartWaitingForDependingCompletion(depending));

            _pipeOutPacket.SendAsync(wrapper);

            return depending;
        }

        private async Task StartWaitingForDependingCompletion(DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase> depending)
        {
            PacketBase packet = depending.DependingTaskCompletion.State;

            try
            {
                IEnumerable<NotificationBase> notifications = await depending.WaitForCompletion().ConfigureAwait(false);
                if (notifications.All(n => n is SucceededNotification))
                {
                    _completions[packet].SetResult(new SucceededNotification());
                }
                else
                {
                    _completions[packet].SetResult(new FailedNotification());
                }
            }
            catch(Exception ex)
            {
                _completions[packet].SetException(ex);
            }
            finally
            {
                _completions.Remove(packet);
                //_waitingTasks.Remove(packet);
                //_dependings.Remove(packet);
            }
        }
    }
}
