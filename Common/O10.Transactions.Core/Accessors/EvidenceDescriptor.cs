using O10.Transactions.Core.Enums;
using System.Collections.Generic;

namespace O10.Transactions.Core.Accessors
{
    public class EvidenceDescriptor
    {
        public EvidenceDescriptor()
        {
            Parameters = new Dictionary<string, string>();
        }

        public LedgerType LedgerType { get; set; }
        public ushort ActionType { get; set; }

        public Dictionary<string, string> Parameters { get; }
    }
}
