using System.Collections.Generic;
using O10.Transactions.Core.Ledgers.Registry;
using O10.Transactions.Core.Ledgers.Synchronization;
using O10.Core.Architecture;

namespace O10.Node.Core.Synchronization
{
    [ServiceContract]
    public interface ISyncRegistryMemPool
    {
        void AddCandidateBlock(RegistryFullBlock registryFullBlock);

        void RegisterCombinedBlock(SynchronizationRegistryCombinedBlock combinedBlock);

        IEnumerable<RegistryFullBlock> GetRegistryBlocks();
    }
}
