using O10.Core.Architecture;
using O10.Crypto.Models;
using O10.Gateway.DataLayer.Model;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Ledgers;
using O10.Transactions.Core.Ledgers.Registry.Transactions;
using System.Threading.Tasks;

namespace O10.Gateway.Common.Services.LedgerSynchronizers
{
    [ExtensionPoint]
    public interface ILedgerSynchronizer
    {
        public LedgerType LedgerType { get; }

        /// <summary>
        /// Obtains a packet from a ledger and stores it into the local GW DB
        /// </summary>
        /// <param name="witnessPacket">Will be used for creating relation of the obtained transaction to its witness</param>
        /// <param name="registerTransaction">Will be used for obtaining the transaction from a ledger</param>
        /// <returns></returns>
        Task SyncByWitness(WitnessPacket witnessPacket, RegisterTransaction registerTransaction);

        TransactionBase? GetByWitness(WitnessPacket witnessPacket);
    }
}
