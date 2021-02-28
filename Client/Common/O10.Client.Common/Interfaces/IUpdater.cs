using System.Threading;
using System.Threading.Tasks.Dataflow;
using O10.Core.Architecture;
using O10.Core.Notifications;
using O10.Transactions.Core.Ledgers;

namespace O10.Client.Common.Interfaces
{
    [ExtensionPoint]
    public interface IUpdater
    {
        void Initialize(long accountId, CancellationToken cancellationToken);
        ITargetBlock<PacketBase> PipeIn { get; set; }
        ITargetBlock<NotificationBase> PipeInNotifications { get; }
    }
}
