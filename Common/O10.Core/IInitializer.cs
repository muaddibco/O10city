using System.Threading;
using O10.Core.Architecture;


namespace O10.Core
{
    [ExtensionPoint]
    public interface IInitializer
    {
        ExtensionOrderPriorities Priority { get; }

        bool Initialized { get; }

        void Initialize(CancellationToken cancellationToken);
    }
}
