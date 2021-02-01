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

        private static IPropagatorBlock<DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase>, DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase>> InputPipe => new TransformBlock<DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase>, DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase>>(d => d);
        private IPropagatorBlock<DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase>, TaskCompletionWrapper<PacketBase>> ProduceRegistrationPacket 
            => new TransformBlock<DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase>, TaskCompletionWrapper<PacketBase>>(e => CreateRegisterPacket(e));

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

        private bool ValidateEvidence(DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase> wrapper)
        {
            if(wrapper.State.PacketType == PacketType.Stealth || wrapper.State.PacketType == PacketType.Transactional)
            {
                // TODO: replace this with an apporopriate logic that waits for packet to be stored in a node storage
                return true;
            }

            var accessor = _accessorProvider.GetInstance(wrapper.State.PacketType);
            var packet = accessor.GetPacket<PacketBase>(wrapper.State);

            return packet != null;
        }

        private async Task<TaskCompletionWrapper<PacketBase>> CreateRegisterPacket(DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase> wrapper)
        {
            var evidence = wrapper.State;
            var packet = _translator.Translate(evidence);

            var lastPacketInfo = await _gatewayContext.GetLastPacketInfo().ConfigureAwait(false);
            packet.BlockHeight = lastPacketInfo.Height;
            packet.SyncBlockHeight = (await _networkSynchronizer.GetLastSyncBlock().ConfigureAwait(false))?.Height ?? 0;

            ISerializer serializer = _serializersFactory.Create(packet);
            serializer.SerializeBody();
            _gatewayContext.SigningService.Sign(packet);

            return new TaskCompletionWrapper<PacketBase>(packet);
        }
    }
}
