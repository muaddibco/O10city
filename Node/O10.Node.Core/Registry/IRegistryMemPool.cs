using System.Collections.Generic;
using O10.Transactions.Core.DataModel.Registry;
using O10.Core.Architecture;
using O10.Core.Identity;

namespace O10.Node.Core.Registry
{
    [ServiceContract]
    public interface IRegistryMemPool
    {
        bool EnqueueTransactionWitness(RegistryRegisterBlock transactionWitness);
        bool EnqueueTransactionWitness(RegistryRegisterExBlock transactionWitness);
        bool EnqueueTransactionWitness(RegistryRegisterStealth transactionWitness);

        SortedList<ushort, RegistryRegisterBlock> DequeueStateWitnessBulk();
        SortedList<ushort, RegistryRegisterStealth> DequeueUtxoWitnessBulk();

        void ClearWitnessed(RegistryShortBlock transactionsShortBlock);

		bool IsKeyImageWitnessed(IKey keyImage);
    }
}
