using System;
using System.Collections.Generic;
using System.Threading;
using O10.Core.Architecture;
using O10.Transactions.Core.Ledgers.Synchronization;
using O10.Transactions.Core.Ledgers.Registry;
using O10.Node.DataLayer.DataServices;
using O10.Transactions.Core.Ledgers;
using System.Threading.Tasks;

namespace O10.Node.Core.Centralized
{
    [ServiceContract]
    public interface IRealTimeRegistryService
    {
        IEnumerable<Tuple<SynchronizationPacket, RegistryPacket>> GetRegistryConsumingEnumerable(CancellationToken cancellationToken);

        Task PostPackets(SynchronizationPacket aggregatedRegistrationsPacket, RegistryPacket registrationsPacket, CancellationToken cancellationToken);
        void PostTransaction(DataResult<IPacketBase> result);
		long GetLowestCombinedBlockHeight();
    }
}
