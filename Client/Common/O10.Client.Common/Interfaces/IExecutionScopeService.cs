using O10.Client.Common.Services;
using O10.Client.DataLayer.Enums;
using O10.Core.Architecture;
using System.Threading.Tasks;

namespace O10.Client.Common.Interfaces
{
    [ExtensionPoint]
    public interface IExecutionScopeService
    {
        AccountType AccountType { get; }

        Task Initiliaze(ScopeInitializationParams initializationParams);
    }
}
