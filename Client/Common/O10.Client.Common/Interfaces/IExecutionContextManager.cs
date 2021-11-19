using O10.Client.Common.Services;
using O10.Client.Common.Services.ExecutionScope;
using O10.Client.DataLayer.Enums;
using O10.Core.Architecture;
using System;

namespace O10.Client.Common.Interfaces
{
    [ServiceContract]
    public interface IExecutionContextManager
    {
        ScopePersistency InitializeExecutionServices(AccountType accountType, ScopeInitializationParams initializationParams, Func<IServiceProvider, IUpdater?>? getUpdater = null);
        ScopePersistency ResolveExecutionServices(long accountId);
        bool IsStarted(long accountId);
        void UnregisterExecutionServices(long accountId);
    }
}
