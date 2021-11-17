using O10.Client.Common.Interfaces;
using O10.Client.Common.Services.ExecutionScope;
using O10.Core.Architecture;

namespace O10.Client.Web.Portal.Services
{
    [ServiceContract]
    public interface IWebExecutionContextManager
    {
        ScopePersistency InitializeIdentityProviderExecutionServices(long accountId, byte[] secretKey, IUpdater updater = null);
        ScopePersistency InitializeServiceProviderExecutionServices(long accountId, byte[] secretKey, IUpdater updater = null);
        ScopePersistency InitializeUserExecutionServices(long accountId, byte[] secretSpendKey, byte[] secretViewKey, byte[] pwdSecretKey, IUpdater updater = null);
        ScopePersistency ResolveExecutionServices(long accountId);
        bool IsStarted(long accountId);
        void UnregisterExecutionServices(long accountId);
    }
}