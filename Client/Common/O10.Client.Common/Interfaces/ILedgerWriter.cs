using O10.Core;
using O10.Core.Architecture;
using O10.Transactions.Core.Enums;

namespace O10.Client.Common.Interfaces
{
    [ExtensionPoint]
    public interface ILedgerWriter : IDynamicPipe
    {
        public LedgerType LedgerType { get; }
        void Initialize(long accountId);
    }
}
