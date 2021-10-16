using System.Threading;
using System.Threading.Tasks;
using O10.Core.Architecture;

namespace O10.Core.Modularity
{
    [ExtensionPoint]
    public interface IModule
    {
        bool IsInitialized { get; }

        string Name { get; }

        Task Initialize(CancellationToken ct);

        void StartModule();
    }
}
