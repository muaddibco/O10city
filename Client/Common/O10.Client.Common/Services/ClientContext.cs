using O10.Client.Common.Interfaces;
using O10.Core.Architecture;

namespace O10.Client.Common.Services
{
    [RegisterDefaultImplementation(typeof(IClientContext), Lifetime = LifetimeManagement.Scoped)]
    public class ClientContext : IClientContext
    {
        public long AccountId { get; private set; }

        public void Initialize(long accountId)
        {
            AccountId = accountId;
        }
    }
}
