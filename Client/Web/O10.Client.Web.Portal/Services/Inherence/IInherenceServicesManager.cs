using System.Collections.Generic;
using O10.Core;
using O10.Core.Architecture;

namespace O10.Client.Web.Portal.Services.Inherence
{
    [ServiceContract]
    public interface IInherenceServicesManager : IRepository<IInherenceService, string>
    {
        IEnumerable<IInherenceService> GetAll();
    }
}
