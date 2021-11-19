using System.Threading;
using System.Threading.Tasks.Dataflow;
using O10.Core.Notifications;
using O10.Crypto.Models;

namespace O10.Client.Common.Interfaces
{
    public interface IUpdater
    {
        void Initialize(long accountId, CancellationToken cancellationToken);
        ITargetBlock<TransactionBase> PipeIn { get; set; }
        ITargetBlock<NotificationBase> PipeInNotifications { get; }
    }
}
