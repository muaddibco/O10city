using System.Threading;
using O10.Core.Synchronization;

namespace O10.Node.Core.Registry
{
    internal class SyncCycleDescriptor
    {
        public SyncCycleDescriptor(SynchronizationDescriptor synchronizationDescriptor)
        {
            SynchronizationDescriptor = synchronizationDescriptor;
            CancellationTokenSource = new CancellationTokenSource();
        }

        public SynchronizationDescriptor SynchronizationDescriptor { get; set; }

        public int Round { get; set; }

        public bool CancellationRequested { get; set; }

        public CancellationTokenSource CancellationTokenSource { get; set; }
    }
}
