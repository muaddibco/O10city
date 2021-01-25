using O10.Core.Architecture;

namespace O10.Core.Identity
{
    [ServiceContract]
    public interface IIdentityKeyProvidersRegistry : IRepository<IIdentityKeyProvider>, IRepository<IIdentityKeyProvider, string>
    {
        IIdentityKeyProvider GetTransactionsIdenityKeyProvider();
    }
}
