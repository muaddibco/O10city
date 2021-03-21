using System;
using System.Collections.Generic;
using System.Threading;
using O10.Core.Architecture;
using O10.Transactions.Core.Ledgers.Synchronization;
using O10.Transactions.Core.Ledgers.Registry;
using O10.Node.DataLayer.DataServices;
using O10.Core.Models;
using O10.Core.Identity;

namespace O10.Node.Core.Centralized
{
    [ServiceContract]
    public interface IRealTimeRegistryService
    {
        IEnumerable<Tuple<SynchronizationPacket, RegistryPacket>> GetRegistryConsumingEnumerable(CancellationToken cancellationToken);

        void PostPackets(SynchronizationPacket aggregatedRegistrationsPacket, RegistryPacket registrationsPacket);
        void PostTransaction(TaskCompletionWrapper<IKey> completionWrapperByTransactionHashKey);
		long GetLowestCombinedBlockHeight();
        void RegisterInternalChainDataService(IChainDataService chainDataService);
    }
}
