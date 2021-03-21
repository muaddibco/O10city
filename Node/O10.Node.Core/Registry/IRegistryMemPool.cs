using System.Collections.Generic;
using O10.Transactions.Core.Ledgers.Registry;
using O10.Core.Architecture;
using O10.Core.Identity;

namespace O10.Node.Core.Registry
{
    [ServiceContract]
    public interface IRegistryMemPool
    {
        bool EnqueueTransactionWitness(RegistryPacket witness);

        SortedList<ushort, RegistryPacket> DequeueWitnessBulk();

        //void ClearWitnessed(RegistryShortBlock transactionsShortBlock);

		bool IsKeyImageWitnessed(IKey keyImage);
    }
}
