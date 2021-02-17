using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using O10.Transactions.Core.Ledgers.Synchronization;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Interfaces;
using O10.Network.Interfaces;
using O10.Core.Architecture;
using O10.Core.States;
using O10.Core.Synchronization;
using O10.Core.Models;

namespace O10.Node.Core.Synchronization
{
    // need to implement logic with time limit for confirmation of retransmitted blocks, etc
    // what happens when consensus was not achieved
    [RegisterExtension(typeof(IBlocksHandler), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationBlocksHandler : IBlocksHandler
    {
        public const string NAME = "SynchronizationBlocksHandler";
        public const ushort TARGET_CONSENSUS_SIZE = 21;
        public const ushort TARGET_CONSENSUS_LOW_LIMIT = 1;// 14;

        private readonly ISynchronizationContext _synchronizationContext;
        private readonly IServerCommunicationServicesRegistry _communicationServicesRegistry;
        private readonly ISyncRegistryMemPool _syncRegistryMemPool;

        private readonly BlockingCollection<SynchronizationBlockBase> _synchronizationBlocks;
        

        public SynchronizationBlocksHandler(IStatesRepository statesRepository,
                                            IServerCommunicationServicesRegistry communicationServicesRegistry,
                                            ISyncRegistryMemPool syncRegistryMemPool)
        {
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _communicationServicesRegistry = communicationServicesRegistry;
            _syncRegistryMemPool = syncRegistryMemPool;
            _synchronizationBlocks = new BlockingCollection<SynchronizationBlockBase>();
        }

        public string Name => NAME;

        public LedgerType PacketType => LedgerType.Synchronization;

        public void Initialize(CancellationToken ct)
        {
            Task.Factory.StartNew(() => ProcessBlocks(ct), ct, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        public void ProcessBlock(PacketBase blockBase)
        {
            if (blockBase is SynchronizationBlockBase synchronizationBlock && !_synchronizationBlocks.IsAddingCompleted)
            {
                _synchronizationBlocks.Add(synchronizationBlock);
            }
        }

        #region Private Functions

        private void ProcessBlocks(CancellationToken ct)
        {
            foreach (SynchronizationBlockBase synchronizationBlock in _synchronizationBlocks.GetConsumingEnumerable(ct))
            {
                ulong lastBlockHeight = _synchronizationContext.LastBlockDescriptor?.BlockHeight ?? 0;
                if (lastBlockHeight + 1 > synchronizationBlock.Height)
                {
                    continue;
                }

                if(synchronizationBlock is SynchronizationRegistryCombinedBlock synchronizationRegistryCombined)
                {
                    _syncRegistryMemPool.RegisterCombinedBlock(synchronizationRegistryCombined);
                }
            }
        }

        #endregion Private Functions
    }
}
