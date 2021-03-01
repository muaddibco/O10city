using System;
using System.Collections.Generic;
using System.Threading;
using O10.Core.Architecture;
using O10.Transactions.Core.Ledgers;
using O10.Transactions.Core.Ledgers.Synchronization.Transactions;
using O10.Transactions.Core.Ledgers.Registry.Transactions;
using O10.Transactions.Core.Ledgers.Synchronization;
using O10.Transactions.Core.Ledgers.Registry;

namespace O10.Node.Core.Centralized
{
    [ServiceContract]
    public interface IRealTimeRegistryService
    {
        IEnumerable<Tuple<SynchronizationPacket, RegistryPacket>> GetRegistryConsumingEnumerable(CancellationToken cancellationToken);

        void PostPackets(SynchronizationPacket aggregatedRegistrationsPacket, RegistryPacket registrationsPacket);
        void PostTransaction(IPacketBase packet);
		long GetLowestCombinedBlockHeight();
		IPacketBase GetPacket(long combinedBlockHeight, byte[] hash);
    }
}
