using O10.Core.Architecture;
using O10.Core.Models;
using O10.Crypto.Models;
using O10.Transactions.Core.Enums;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace O10.Client.Common.Interfaces
{
    [ExtensionPoint]
    public interface ILedgerWriter
    {
        public LedgerType LedgerType { get; }
        public ITargetBlock<TaskCompletionWrapper<TransactionBase>> PipeIn { get; }
        Task Initialize(long accountId);
    }
}
