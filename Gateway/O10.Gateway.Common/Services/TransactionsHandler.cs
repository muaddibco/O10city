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
using O10.Gateway.Common.Configuration;
using O10.Gateway.Common.Services.Results;
using O10.Transactions.Core.Accessors;
using O10.Transactions.Core.DataModel.Stealth;
using O10.Transactions.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace O10.Gateway.Common.Services
{
    [RegisterDefaultImplementation(typeof(ITransactionsHandler), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionsHandler : ITransactionsHandler
    {
        private readonly IPropagatorBlock<PacketBase, EvidenceDescriptor> _pipeEvidence;
        private readonly IPropagatorBlock<PacketBase, PacketBase> _pipeInPacket;
        private readonly IPropagatorBlock<PacketBase, PacketBase> _pipeOutPacket;
        private readonly IHashCalculation _hashCalculation;
        private readonly ILogger _logger;
        private readonly Dictionary<PacketBase, TaskCompletionSource<ResultBase>> _packetValidationTasks;
        private readonly ISynchronizerConfiguration _synchronizerConfiguration;

        public TransactionsHandler(IHashCalculationsRepository hashCalculationsRepository,
                                   IConfigurationService configurationService,
                                   ILoggerService loggerService)
        {
            _logger = loggerService.GetLogger(nameof(TransactionsHandler));
            _packetValidationTasks = new Dictionary<PacketBase, TaskCompletionSource<ResultBase>>();
            _synchronizerConfiguration = configurationService.Get<ISynchronizerConfiguration>();

            _hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
            _pipeInPacket = new TransformBlock<PacketBase, PacketBase>(p => p);
            _pipeEvidence = new TransformBlock<PacketBase, EvidenceDescriptor>(p => ProduceEvidenceAndSend(p));
            _pipeOutPacket = new TransformBlock<PacketBase, PacketBase>(p => p);
            
            _pipeInPacket.LinkTo(_pipeEvidence, ValidatePacket);
        }

        public TaskCompletionSource<ResultBase> SendPacket(PacketBase packetBase)
        {
            var validationCompletionTask = new TaskCompletionSource<ResultBase>();
            _packetValidationTasks.Add(packetBase, validationCompletionTask);

            _pipeInPacket.SendAsync(packetBase);

            return validationCompletionTask;
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

        private bool ValidatePacket(PacketBase packetBase)
        {
            bool res = true;
            string existingHash = null;

            if (packetBase is StealthBase stealth)
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
            else if(packetBase.PacketType != (ushort)PacketType.Transactional)
            {
                _logger.Error($"Packets of the type {packetBase.GetType().Name} are not supported");
                res = false;
            }

            if (_packetValidationTasks.TryGetValue(packetBase, out TaskCompletionSource<ResultBase> validationCompletionTask))
            {
                validationCompletionTask.SetResult(new KeyImageValidationResult { Succeeded = res, ExistingHash = existingHash });
                _packetValidationTasks.Remove(packetBase);
            }

            return res;
        }

        private EvidenceDescriptor ProduceEvidenceAndSend(PacketBase packet)
        {
            EvidenceDescriptor evidenceDescriptor = new EvidenceDescriptor
            {
                ActionType = packet.BlockType,
                PacketType = (PacketType)packet.PacketType,
                Parameters = new Dictionary<string, string> { { "BodyHash", _hashCalculation.CalculateHash(packet.RawData).ToHexString() } }
            };

            _pipeOutPacket.SendAsync(packet);

            return evidenceDescriptor;
        }
    }
}
