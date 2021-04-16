using O10.Core;
using O10.Core.Architecture;
using O10.Core.Models;
using O10.Core.States;
using O10.Core.Translators;
using O10.Transactions.Core.Accessors;
using O10.Transactions.Core.Enums;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.Ledgers;
using O10.Crypto.Models;
using O10.Transactions.Core.Exceptions;
using O10.Transactions.Core.Ledgers.Registry;

namespace O10.Gateway.Common.Services
{
    [RegisterDefaultImplementation(typeof(IEvidencesHandler), Lifetime = LifetimeManagement.Singleton)]
    public class EvidencesHandler : IEvidencesHandler
    {
        private readonly ITranslator<EvidenceDescriptor, RegistryPacket> _translator;
        private readonly IAccessorProvider _accessorProvider;
        private readonly INetworkSynchronizer _networkSynchronizer;
        private readonly IGatewayContext _gatewayContext;

        private IPropagatorBlock<DependingTaskCompletionWrapper<EvidenceDescriptor, IPacketBase>, DependingTaskCompletionWrapper<EvidenceDescriptor, IPacketBase>> _inputPipe;
        private IPropagatorBlock<DependingTaskCompletionWrapper<EvidenceDescriptor, IPacketBase>, TaskCompletionWrapper<IPacketBase>> _produceRegistrationPacket;

        public EvidencesHandler(ITranslatorsRepository translatorsRepository,
                                IAccessorProvider accessorProvider,
                                INetworkSynchronizer networkSynchronizer,
                                IStatesRepository statesRepository)
        {
            _inputPipe 
                = new TransformBlock<DependingTaskCompletionWrapper<EvidenceDescriptor, IPacketBase>, DependingTaskCompletionWrapper<EvidenceDescriptor, IPacketBase>>(d => d);
            
            _produceRegistrationPacket 
                = new TransformBlock<DependingTaskCompletionWrapper<EvidenceDescriptor, IPacketBase>, TaskCompletionWrapper<IPacketBase>>(e => CreateRegisterPacket(e));

            _translator = translatorsRepository.GetInstance<EvidenceDescriptor, RegistryPacket>();
            _inputPipe.LinkTo(_produceRegistrationPacket, ValidateEvidence);
            _accessorProvider = accessorProvider;
            _networkSynchronizer = networkSynchronizer;
            _gatewayContext = statesRepository.GetInstance<IGatewayContext>();
        }

        public ISourceBlock<T> GetSourcePipe<T>(string name = null)
        {
            return (ISourceBlock<T>)_produceRegistrationPacket;
        }

        public ITargetBlock<T> GetTargetPipe<T>(string name = null)
        {
            return (ITargetBlock<T>)_inputPipe;
        }

        private bool ValidateEvidence(DependingTaskCompletionWrapper<EvidenceDescriptor, IPacketBase> wrapper)
        {
            if(wrapper.State.LedgerType == LedgerType.Stealth || wrapper.State.LedgerType == LedgerType.O10State)
            {
                // TODO: replace this with an apporopriate logic that waits for packet to be stored in a node storage
                return true;
            }

            var accessor = _accessorProvider.GetInstance(wrapper.State.LedgerType);
            if(accessor == null)
            {
                throw new AccessorNotSupportedException(wrapper.State.LedgerType);
            }

            return accessor.GetTransaction<TransactionBase>(wrapper.State) != null;
        }

        private async Task<TaskCompletionWrapper<IPacketBase>> CreateRegisterPacket(DependingTaskCompletionWrapper<EvidenceDescriptor, IPacketBase> wrapper)
        {
            var evidence = wrapper.State;
            try
            {
                var packet = _translator.Translate(evidence);

                var lastPacketInfo = await _gatewayContext.GetLastPacketInfo().ConfigureAwait(false);
                packet.Height = lastPacketInfo.Height;
                packet.SyncHeight = (await _networkSynchronizer.GetLastSyncBlock().ConfigureAwait(false))?.Height ?? 0;

                packet.Signature = (SingleSourceSignature)_gatewayContext.SigningService.Sign(packet.Body);

                var evidenceWrapper = new TaskCompletionWrapper<IPacketBase>(packet);
                evidenceWrapper.TaskCompletion.Task
                    .ContinueWith((t, o) => 
                    {
                        var w = o as DependingTaskCompletionWrapper<EvidenceDescriptor, IPacketBase>;
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
