using O10.Core.Architecture;
using O10.Crypto.Models;
using O10.Gateway.DataLayer.Model;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Ledgers;
using System.Threading.Tasks;

namespace O10.Gateway.Common.Services.LedgerSynchronizers
{
    [ExtensionPoint]
    public interface ILedgerSynchronizer
    {
        public LedgerType LedgerType { get; }

        Task SyncByWitness(WitnessPacket witnessPacket);

        TransactionBase GetByWitness(WitnessPacket witnessPacket);
    }
}
