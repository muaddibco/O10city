using O10.Core;
using O10.Core.Architecture;

namespace O10.Client.Web.Portal.ExternalIdps.Validators
{
    [ServiceContract]
    public interface IExternalIdpDataValidatorsRepository : IRepository<IExternalIdpDataValidator, string>
    {
    }
}
