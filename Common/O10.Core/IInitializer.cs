using System.Threading;
using System.Threading.Tasks;
using O10.Core.Architecture;


namespace O10.Core
{
    [ExtensionPoint]
    public interface IInitializer
    {
        ExtensionOrderPriorities Priority { get; }

        bool Initialized { get; }

        Task Initialize(CancellationToken cancellationToken);
    }
}
