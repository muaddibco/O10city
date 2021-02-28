using O10.Core;
using O10.Core.Architecture;
using O10.Core.Models;
using O10.Core.States;
using O10.Core.Translators;
using O10.Transactions.Core.Accessors;
using O10.Transactions.Core.Ledgers.Registry;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Serializers;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.Ledgers;

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

        private IPropagatorBlock<DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase>, DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase>> _inputPipe;
        private IPropagatorBlock<DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase>, TaskCompletionWrapper<PacketBase>> _produceRegistrationPacket;

        public EvidencesHandler(ITranslatorsRepository translatorsRepository,
                                IAccessorProvider accessorProvider,
                                INetworkSynchronizer networkSynchronizer,
                                IStatesRepository statesRepository,
                                ISerializersFactory serializersFactory)
        {
            _inputPipe = new TransformBlock<DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase>, DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase>>(d => d);
            _produceRegistrationPacket = new TransformBlock<DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase>, TaskCompletionWrapper<PacketBase>>(e => CreateRegisterPacket(e));

            _translator = translatorsRepository.GetInstance<EvidenceDescriptor, RegistryRegisterExBlock>();
            _inputPipe.LinkTo(_produceRegistrationPacket, ValidateEvidence);
            _accessorProvider = accessorProvider;
            _networkSynchronizer = networkSynchronizer;
            _gatewayContext = statesRepository.GetInstance<IGatewayContext>();
            _serializersFactory = serializersFactory;
        }

        public ISourceBlock<T> GetSourcePipe<T>(string name = null)
        {
            return (ISourceBlock<T>)_produceRegistrationPacket;
        }

        public ITargetBlock<T> GetTargetPipe<T>(string name = null)
        {
            return (ITargetBlock<T>)_inputPipe;
        }

        private bool ValidateEvidence(DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase> wrapper)
        {
            if(wrapper.State.LedgerType == LedgerType.Stealth || wrapper.State.LedgerType == LedgerType.O10State)
            {
                // TODO: replace this with an apporopriate logic that waits for packet to be stored in a node storage
                return true;
            }

            var accessor = _accessorProvider.GetInstance(wrapper.State.LedgerType);
            var packet = accessor.GetPacket<PacketBase>(wrapper.State);

            return packet != null;
        }

        private async Task<TaskCompletionWrapper<PacketBase>> CreateRegisterPacket(DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase> wrapper)
        {
            var evidence = wrapper.State;
            try
            {
                var packet = _translator.Translate(evidence);

                var lastPacketInfo = await _gatewayContext.GetLastPacketInfo().ConfigureAwait(false);
                packet.Height = lastPacketInfo.Height;
                packet.PowHash = new byte[Globals.DEFAULT_HASH_SIZE];
                packet.SyncHeight = (await _networkSynchronizer.GetLastSyncBlock().ConfigureAwait(false))?.Height ?? 0;

                ISerializer serializer = _serializersFactory.Create(packet);
                serializer.SerializeBody();
                _gatewayContext.SigningService.Sign(packet);

                var evidenceWrapper = new TaskCompletionWrapper<PacketBase>(packet);
                evidenceWrapper.TaskCompletion.Task
                    .ContinueWith((t, o) => 
                    {
                        var w = o as DependingTaskCompletionWrapper<EvidenceDescriptor, PacketBase>;
                        if(t.IsCompletedSuccessfully)
                        {
                            w.TaskCompletion.SetResult(t.Result);
                        }
                        else
                        {
                            w.TaskCompletion.SetException(t.Exception.InnerException);
                        }
                    }, wrapper, TaskScheduler.Default);

                return evidenceWrapper;

            }
            catch (Exception ex)
            {
                wrapper.TaskCompletion.SetException(ex);
            }

            return null;
        }
    }
}
