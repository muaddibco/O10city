using O10.Client.Common.Services;
using O10.Core.Architecture;

namespace O10.Client.Common.Interfaces
{
    [ExtensionPoint]
    public interface IExecutionScopeService
    {
        string Name { get; }

        T GetScopeInitializationParams<T>() where T: ScopeInitializationParams;
        
        void Initiliaze(ScopeInitializationParams initializationParams);
    }
}
