using O10.Core;
using O10.Core.Architecture;

namespace O10.Client.Web.Common.Services
{
    [ServiceContract]
    public interface IExternalUpdatersRepository : IBulkRepository<IExternalUpdater>
    {
    }
}
