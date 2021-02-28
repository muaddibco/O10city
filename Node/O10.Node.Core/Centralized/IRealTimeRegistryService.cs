using System;
using System.Collections.Generic;
using System.Threading;
using O10.Core.Architecture;
using O10.Transactions.Core.Ledgers;
using O10.Transactions.Core.Ledgers.Synchronization.Transactions;
using O10.Transactions.Core.Ledgers.Registry.Transactions;

namespace O10.Node.Core.Centralized
{
    [ServiceContract]
    public interface IRealTimeRegistryService
    {
        IEnumerable<Tuple<AggregatedRegistrationsTransaction, FullRegistryTransaction>> GetRegistryConsumingEnumerable(CancellationToken cancellationToken);

        void PostPackets(AggregatedRegistrationsTransaction combinedBlock, FullRegistryTransaction registryFullBlock);
        void PostTransaction(IPacketBase packet);
		long GetLowestCombinedBlockHeight();
		IPacketBase GetPacket(long combinedBlockHeight, byte[] hash);
    }
}
