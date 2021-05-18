using O10.Core;
using O10.Core.Architecture;
using O10.Transactions.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace O10.Client.Common.Interfaces
{
    [ExtensionPoint]
    public interface IPacketsProducer : IDynamicPipe
    {
        public IEnumerable<LedgerType> LedgerTypes { get; }
        Task Initialize(long accountId);
    }
}
