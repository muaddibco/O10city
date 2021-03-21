using O10.Transactions.Core.Enums;

namespace O10.Node.DataLayer.DataServices.Keys
{
    public class LedgerAndIdKey : IdKey
    {
        public LedgerAndIdKey(LedgerType ledgerType, long id)
            : base(id)
        {
            LedgerType = ledgerType;
        }
        
        public LedgerType LedgerType { get; }
    }
}
