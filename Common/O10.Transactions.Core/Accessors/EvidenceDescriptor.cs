using O10.Transactions.Core.Enums;
using System.Collections.Generic;

namespace O10.Transactions.Core.Accessors
{
    public class EvidenceDescriptor
    {
        public const string TRANSACTION_HASH = "TransactionHash";

        public EvidenceDescriptor()
        {
            Parameters = new Dictionary<string, string>();
        }

        public EvidenceDescriptor(LedgerType ledgerType, ushort transactionType, Dictionary<string, string> parameters)
        {
            LedgerType = ledgerType;
            ActionType = transactionType;
            Parameters = parameters;
        }

        public LedgerType LedgerType { get; set; }
        public ushort ActionType { get; set; }

        public Dictionary<string, string> Parameters { get; }

        public string? this[string key]
        {
            get => Parameters?.ContainsKey(key) ?? false ? Parameters[key] : null;
        }
    }
}
