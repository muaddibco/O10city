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
using O10.Transactions.Core.DataModel.Stealth;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Serializers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace O10.Gateway.Common.Services
{
    [RegisterDefaultImplementation(typeof(ITransactionsHandler), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionsHandler : ITransactionsHandler
    {
        private readonly IPropagatorBlock<TaskCompletionWrapper<PacketBase>, DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase>> _pipeEvidence;
        private readonly IPropagatorBlock<TaskCompletionWrapper<PacketBase>, TaskCompletionWrapper<PacketBase>> _pipeInPacket;
        private readonly IPropagatorBlock<TaskCompletionWrapper<PacketBase>, TaskCompletionWrapper<PacketBase>> _pipeOutPacket;
        private readonly IHashCalculation _hashCalculation;
        private readonly ILogger _logger;
        private readonly ISynchronizerConfiguration _synchronizerConfiguration;
        private readonly ISerializersFactory _serializersFactory;
        private readonly Dictionary<PacketBase, TaskCompletionSource<NotificationBase>> _completions = new Dictionary<PacketBase, TaskCompletionSource<NotificationBase>>();

        public TransactionsHandler(IHashCalculationsRepository hashCalculationsRepository,
                                   IConfigurationService configurationService,
                                   ISerializersFactory serializersFactory,
                                   ILoggerService loggerService)
        {
            _logger = loggerService.GetLogger(nameof(TransactionsHandler));
            _synchronizerConfiguration = configurationService.Get<ISynchronizerConfiguration>();

            _hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
            _pipeInPacket = new TransformBlock<TaskCompletionWrapper<PacketBase>, TaskCompletionWrapper<PacketBase>>(p => p);
            _pipeEvidence = new TransformBlock<TaskCompletionWrapper<PacketBase>, DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase>>(p => ProduceEvidenceAndSend(p));
            
            _pipeInPacket.LinkTo(_pipeEvidence, ValidatePacket);
            _serializersFactory = serializersFactory;
        }

        public TaskCompletionSource<NotificationBase> SendPacket(PacketBase packetBase)
        {
            TaskCompletionWrapper<PacketBase> wrapper = new TaskCompletionWrapper<PacketBase>(packetBase);

            _pipeInPacket.SendAsync(wrapper);

            return wrapper.TaskCompletion;
        }

        public ISourceBlock<T> GetSourcePipe<T>(string name = null)
        {
            if(typeof(T) == typeof(EvidenceDescriptor))
            {
                return (ISourceBlock<T>)_pipeEvidence;
            }
            else if(typeof(T) == typeof(PacketBase))
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

        private bool ValidatePacket(TaskCompletionWrapper<PacketBase> wrapper)
        {
            bool res = true;
            string existingHash = null;

            if (wrapper.State is StealthBase stealth)
            {
                var keyImage = stealth.KeyImage.ToString();

                _logger.LogIfDebug(() => $"Sending to Node {_synchronizerConfiguration.NodeApiUri} request for finding hash by a key image {keyImage}");
                var packetHashResponse = AsyncUtil.RunSync(async () => await _synchronizerConfiguration.NodeApiUri.AppendPathSegments("HashByKeyImage", keyImage).GetJsonAsync<PacketHashResponse>().ConfigureAwait(false));


                if (!string.IsNullOrEmpty(packetHashResponse.Hash))
                {
                    _logger.Error($"It was found that key image {keyImage} already was witnessed for the packet with hash {packetHashResponse.Hash}");
                    res = false;
                    existingHash = packetHashResponse.Hash;
                }
            }
            else if(wrapper.State.PacketType != (ushort)PacketType.Transactional)
            {
                _logger.Error($"Packets of the type {wrapper.State.GetType().Name} are not supported");
                res = false;
            }

            wrapper.TaskCompletion.SetResult(new KeyImageViolatedNotification { ExistingHash = existingHash });

            return res;
        }

        private DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase> ProduceEvidenceAndSend(TaskCompletionWrapper<PacketBase> wrapper)
        {
            if(wrapper.State.RawData.Length == 0)
            {
                using var serializer = _serializersFactory.Create(wrapper.State);
                serializer.SerializeFully();
            }

            EvidenceDescriptor evidenceDescriptor = new EvidenceDescriptor
            {
                ActionType = wrapper.State.BlockType,
                PacketType = (PacketType)wrapper.State.PacketType,
                Parameters = new Dictionary<string, string> { { "BodyHash", _hashCalculation.CalculateHash(wrapper.State.RawData).ToHexString() } }
            };

            DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase> depending 
                = new DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase>(evidenceDescriptor, wrapper);

            depending
                .CompletionAll.Task.ContinueWith((t, o) => 
                {
                    var packet = o as PacketBase;
                    var completionSource = _completions[packet];
                    if(t.IsCompletedSuccessfully)
                    {
                        if(t.Result.All(n => n is SucceededNotification))
                        {
                            completionSource.SetResult(new SucceededNotification());
                        }
                        else
                        {
                            completionSource.SetResult(new FailedNotification());
                        }
                    }
                    else
                    {
                        completionSource.SetException(t.Exception.InnerException);
                    }

                    _completions.Remove(packet);
                }, wrapper.State, TaskScheduler.Default);

            _pipeOutPacket.SendAsync(wrapper);

            return depending;
        }
    }
}
