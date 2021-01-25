using System.Threading;
using O10.Core.Architecture;

namespace O10.Core.Modularity
{
    [ExtensionPoint]
    public interface IModule
    {
        bool IsInitialized { get; }

        string Name { get; }

        void Initialize(CancellationToken ct);

        void StartModule();
    }
}
