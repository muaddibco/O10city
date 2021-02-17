﻿using System;
using System.Collections.Generic;
using System.Threading;
using O10.Transactions.Core.Ledgers.Registry;
using O10.Transactions.Core.Ledgers.Synchronization;
using O10.Core.Architecture;
using O10.Core.Models;

namespace O10.Node.Core.Centralized
{
    [ServiceContract]
    public interface IRealTimeRegistryService
    {
        IEnumerable<Tuple<SynchronizationRegistryCombinedBlock, RegistryFullBlock>> GetRegistryConsumingEnumerable(CancellationToken cancellationToken);

        void PostPackets(SynchronizationRegistryCombinedBlock combinedBlock, RegistryFullBlock registryFullBlock);
        void PostTransaction(PacketBase packet);
		ulong GetLowestCombinedBlockHeight();
		PacketBase GetPacket(ulong combinedBlockHeight, byte[] hash);
    }
}
