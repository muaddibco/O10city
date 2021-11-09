using O10.Core.Models;
using O10.Crypto.Models;
using System.Threading.Tasks.Dataflow;

namespace O10.Client.Common.Interfaces
{
    public interface ITransactionsService
    {
        ISourceBlock<TaskCompletionWrapper<TransactionBase>> PipeOutTransactions { get; }
    }
}
