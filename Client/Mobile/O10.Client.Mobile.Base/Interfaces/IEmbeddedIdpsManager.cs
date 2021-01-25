using System.Collections.Generic;
using O10.Core;
using O10.Core.Architecture;

namespace O10.Client.Mobile.Base.Interfaces
{
    [ServiceContract]
    public interface IEmbeddedIdpsManager : IRepository<IEmbeddedIdpService, string>
    {
        IEnumerable<IEmbeddedIdpService> GetAllServices();
    }
}
