using O10.Core;
using O10.Core.Architecture;

namespace O10.Client.IdentityProvider.External
{
    [ServiceContract]
    public interface IExternalIdpDataValidatorsRepository : IRepository<IExternalIdpDataValidator, string>
    {
    }
}
