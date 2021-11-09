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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.Ledgers;
using O10.Transactions.Core.Exceptions;
using O10.Transactions.Core.Ledgers.Stealth.Transactions;

namespace O10.Gateway.Common.Services
{
    [RegisterDefaultImplementation(typeof(ITransactionsHandler), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionsHandler : ITransactionsHandler
    {
        private readonly IPropagatorBlock<IPacketBase, IPacketBase> _pipeInPacket;
        private readonly IPropagatorBlock<IPacketBase, DependingTaskCompletionWrapper<EvidenceDescriptor, IPacketBase>> _pipeEvidence;
        private readonly IPropagatorBlock<TaskCompletionWrapper<IPacketBase>, TaskCompletionWrapper<IPacketBase>> _pipeOutPacket;
        private readonly IHashCalculation _hashCalculation;
        private readonly ILogger _logger;
        private readonly ISynchronizerConfiguration _synchronizerConfiguration;
        private readonly Dictionary<IPacketBase, TaskCompletionSource<NotificationBase>> _completions = new Dictionary<IPacketBase, TaskCompletionSource<NotificationBase>>();
        private readonly IAccessorProvider _accessorProvider;

        //private readonly Dictionary<PacketBase, Task> _waitingTasks = new Dictionary<PacketBase, Task>();
        //private readonly Dictionary<PacketBase, DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase>> _dependings = new Dictionary<PacketBase, DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase>>();

        public TransactionsHandler(IHashCalculationsRepository hashCalculationsRepository,
                                   IConfigurationService configurationService,
                                   IAccessorProvider accessorProvider,
                                   ILoggerService loggerService)
        {
            _logger = loggerService.GetLogger(nameof(TransactionsHandler));
            _synchronizerConfiguration = configurationService.Get<ISynchronizerConfiguration>();

            _hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
            _pipeInPacket = new TransformBlock<IPacketBase, IPacketBase>(p => p);
            _pipeEvidence = new TransformBlock<IPacketBase, DependingTaskCompletionWrapper<EvidenceDescriptor, IPacketBase>>(p => ProduceEvidenceAndSend(p));
            _pipeOutPacket = new TransformBlock<TaskCompletionWrapper<IPacketBase>, TaskCompletionWrapper<IPacketBase>>(p => p);
            
            _pipeInPacket.LinkTo(_pipeEvidence, ValidatePacket);
            _pipeInPacket.LinkTo(DataflowBlock.NullTarget<IPacketBase>());
            _accessorProvider = accessorProvider;
        }

        public TaskCompletionSource<NotificationBase> SendPacket(IPacketBase packetBase)
        {
            TaskCompletionSource<NotificationBase> taskCompletion = new TaskCompletionSource<NotificationBase>(packetBase);
            _completions.Add(packetBase, taskCompletion);

            _pipeInPacket.SendAsync(packetBase);

            return taskCompletion;
        }

        public ISourceBlock<T> GetSourcePipe<T>(string name = null)
        {
            if(typeof(T) == typeof(DependingTaskCompletionWrapper<EvidenceDescriptor, IPacketBase>))
            {
                return (ISourceBlock<T>)_pipeEvidence;
            }
            else if(typeof(T) == typeof(TaskCompletionWrapper<IPacketBase>))
            {
                return (ISourceBlock<T>)_pipeOutPacket;
            }

            throw new InvalidSourcePipeRequestException(typeof(T));
        }

        public ITargetBlock<T> GetTargetPipe<T>(string name = null)
        {
            if (typeof(T) == typeof(IPacketBase))
            {
                return (ITargetBlock<T>)_pipeEvidence;
            }

            throw new InvalidTargetPipeRequestException(typeof(T));
        }

        private bool ValidatePacket(IPacketBase packet)
        {
            bool res = true;

            if(packet == null)
            {
                return false;
            }
            else if(packet.Transaction == null)
            {
                res = false;
                _completions[packet].SetException(new NoTransactionException(packet));
                _completions.Remove(packet);
            }
            else if (packet is StealthPacket stealth)
            {
                if (!stealth.IsTransaction<KeyImageCompromisedTransaction>() && !stealth.IsTransaction<RevokeIdentityTransaction>())
                {
                    var keyImage = stealth.Payload.Transaction.KeyImage.ToString();

                    _logger.LogIfDebug(() => $"Sending to Node {_synchronizerConfiguration.NodeApiUri} request for finding hash by a key image {keyImage}");
                    var packetHashResponse = AsyncUtil.RunSync(async () => await _synchronizerConfiguration.NodeApiUri.AppendPathSegments("HashByKeyImage", keyImage).GetJsonAsync<PacketHashResponse>().ConfigureAwait(false));


                    if (packetHashResponse.Hash.IsNotEmpty())
                    {
                        _logger.Error($"It was found that key image {keyImage} already was witnessed for the packet with hash {packetHashResponse.Hash.ToHexString()}");
                        res = false;
                        _completions[packet].SetResult(new KeyImageViolatedNotification { ExistingHash = packetHashResponse.Hash });
                        _completions.Remove(packet);
                    }
                }
            }
            else if(packet.LedgerType != LedgerType.O10State)
            {
                _logger.Error($"Packets of the type {packet.GetType().Name} are not supported");
                res = false;
                _completions[packet].SetResult(new FailedNotification());
                _completions.Remove(packet);
            }

            return res;
        }

        private DependingTaskCompletionWrapper<EvidenceDescriptor, IPacketBase> ProduceEvidenceAndSend(IPacketBase packet)
        {
            TaskCompletionWrapper<IPacketBase> wrapper = new TaskCompletionWrapper<IPacketBase>(packet);

            
            EvidenceDescriptor evidenceDescriptor = _accessorProvider.GetInstance(packet.LedgerType).GetEvidence(packet.Transaction);

            DependingTaskCompletionWrapper<EvidenceDescriptor, IPacketBase> depending 
                = new DependingTaskCompletionWrapper<EvidenceDescriptor, IPacketBase>(evidenceDescriptor, wrapper);

            StartWaitingForDependingCompletion(depending);
            //_dependings.Add(packet, depending);
            //_waitingTasks.Add(packet, StartWaitingForDependingCompletion(depending));

            _pipeOutPacket.SendAsync(wrapper);

            return depending;
        }

        private async Task StartWaitingForDependingCompletion(DependingTaskCompletionWrapper<EvidenceDescriptor, IPacketBase> depending)
        {
            var packet = depending.DependingTaskCompletion.State;

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
