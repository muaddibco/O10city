using O10.Core.Architecture;
using System.Threading;

namespace O10.Network.Handlers
{
    [RegisterDefaultImplementation(typeof(IHandlingFlowContext), Lifetime = LifetimeManagement.Scoped)]
    public class HandlingFlowContext : IHandlingFlowContext
    {
        private static int _index;

        public HandlingFlowContext()
        {
            Index = GetNextIndex();
        }

        public int Index { get; }

        private static int GetNextIndex()
        {
            Interlocked.Increment(ref _index);

            return _index;
        }
    }
}
