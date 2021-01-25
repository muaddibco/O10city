using O10.Core.Architecture;
using O10.Core.Models;
using O10.Core.States;
using O10.Core.Translators;
using O10.Transactions.Core.Accessors;
using O10.Transactions.Core.DataModel.Registry;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Serializers;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace O10.Gateway.Common.Services
{
    [RegisterDefaultImplementation(typeof(IEvidencesHandler), Lifetime = LifetimeManagement.Singleton)]
    public class EvidencesHandler : IEvidencesHandler
    {
        private readonly ITranslator<EvidenceDescriptor, RegistryRegisterExBlock> _translator;
        private readonly IAccessorProvider _accessorProvider;
        private readonly INetworkSynchronizer _networkSynchronizer;
        private readonly IGatewayContext _gatewayContext;
        private readonly ISerializersFactory _serializersFactory;

        private static IPropagatorBlock<EvidenceDescriptor, EvidenceDescriptor> InputPipe => new TransformBlock<EvidenceDescriptor, EvidenceDescriptor>(d => d);
        private IPropagatorBlock<EvidenceDescriptor, RegistryRegisterExBlock> ProduceRegistrationPacket => new TransformBlock<EvidenceDescriptor, RegistryRegisterExBlock>(e => CreateRegisterPacket(e));

        public EvidencesHandler(ITranslatorsRepository translatorsRepository,
                                IAccessorProvider accessorProvider,
                                INetworkSynchronizer networkSynchronizer,
                                IStatesRepository statesRepository,
                                ISerializersFactory serializersFactory)
        {
            _translator = translatorsRepository.GetInstance<EvidenceDescriptor, RegistryRegisterExBlock>();
            InputPipe.LinkTo(ProduceRegistrationPacket, ValidateEvidence);
            _accessorProvider = accessorProvider;
            _networkSynchronizer = networkSynchronizer;
            _gatewayContext = statesRepository.GetInstance<IGatewayContext>();
            _serializersFactory = serializersFactory;
        }

        public ISourceBlock<T> GetSourcePipe<T>(string name = null)
        {
            return (ISourceBlock<T>)ProduceRegistrationPacket;
        }

        public ITargetBlock<T> GetTargetPipe<T>(string name = null)
        {
            return (ITargetBlock<T>)InputPipe;
        }

        private bool ValidateEvidence(EvidenceDescriptor evidenceDescriptor)
        {
            if(evidenceDescriptor.PacketType == PacketType.Stealth || evidenceDescriptor.PacketType == PacketType.Transactional)
            {
                // TODO: replace this with an apporopriate logic that waits for packet to be stored in a node storage
                return true;
            }

            var accessor = _accessorProvider.GetInstance(evidenceDescriptor.PacketType);
            var packet = accessor.GetPacket<PacketBase>(evidenceDescriptor);

            return packet != null;
        }

        private async Task<RegistryRegisterExBlock> CreateRegisterPacket(EvidenceDescriptor evidence)
        {
            var packet = _translator.Translate(evidence);

            var lastPacketInfo = await _gatewayContext.GetLastPacketInfo().ConfigureAwait(false);
            packet.BlockHeight = lastPacketInfo.Height;
            packet.SyncBlockHeight = (await _networkSynchronizer.GetLastSyncBlock().ConfigureAwait(false))?.Height ?? 0;

            ISerializer serializer = _serializersFactory.Create(packet);
            serializer.SerializeBody();
            _gatewayContext.SigningService.Sign(packet);

            return packet;
        }
    }
}
