using O10.Client.Common.Services;
using O10.Core.Architecture;
using System.Threading.Tasks;

namespace O10.Client.Common.Interfaces
{
    [ExtensionPoint]
    public interface IExecutionScopeService
    {
        string Name { get; }

        T GetScopeInitializationParams<T>() where T: ScopeInitializationParams;
        
        Task Initiliaze(ScopeInitializationParams initializationParams);
    }
}
