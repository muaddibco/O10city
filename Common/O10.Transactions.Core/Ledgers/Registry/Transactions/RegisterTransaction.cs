using O10.Transactions.Core.Enums;
using System.Collections.Generic;

namespace O10.Transactions.Core.Ledgers.Registry.Transactions
{
    public class RegisterTransaction : RegistryTransactionBase
    {
        public const string REFERENCED_BODY_HASH = "ReferencedBodyHash";
        public override ushort TransactionType => TransactionTypes.Registry_RegisterEx;

        public LedgerType ReferencedLedgerType { get; set; }

        public ushort ReferencedAction { get; set; }

        public Dictionary<string, string>? Parameters { get; set; }
    }
}
