using O10.Client.Common.Interfaces;
using O10.Client.Web.Common.Services;
using O10.Core.Architecture;

namespace O10.Client.Web.Portal.Services
{
    [ServiceContract]
    public interface IExecutionContextManager
    {
        Persistency InitializeStateExecutionServices(long accountId, byte[] secretKey, IUpdater updater = null);
        Persistency InitializeUtxoExecutionServices(long accountId, byte[] secretSpendKey, byte[] secretViewKey, byte[] pwdSecretKey, IUpdater updater = null);
        Persistency ResolveExecutionServices(long accountId);
        void UnregisterExecutionServices(long accountId);
    }
}