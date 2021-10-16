using O10.Core.Architecture;
using System.Threading;
using System.Threading.Tasks;

namespace O10.Core.Persistency
{
    [ExtensionPoint]
    public interface IDataAccessService
    {
        bool IsInitialized { get; }
        Task Initialize(CancellationToken cancellationToken);
    }
}
