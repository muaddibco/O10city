using O10.Client.Common.Interfaces;
using O10.Core.Logging;
using O10.Core.Models;
using O10.Crypto.Models;
using O10.Transactions.Core.Enums;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace O10.Client.Common.Communication.LedgerWriters
{
    public abstract class LedgerWriterBase : ILedgerWriter
    {
        public LedgerWriterBase(ILoggerService loggerService)
        {
            Logger = loggerService.GetLogger(GetType().Name);
        }

        public abstract LedgerType LedgerType { get; }
        public abstract ITargetBlock<TaskCompletionWrapper<TransactionBase>> PipeIn { get; }
        protected ILogger Logger { get; }
        protected long AccountId { get; private set; }

        public virtual async Task Initialize(long accountId)
        {
            AccountId = accountId;
        }
    }
}
