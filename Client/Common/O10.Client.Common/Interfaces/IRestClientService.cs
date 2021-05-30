using Flurl;
using Flurl.Http;
using O10.Core.Architecture;

namespace O10.Client.Common.Interfaces
{
    [ServiceContract]
    public interface IRestClientService
    {
        IFlurlRequest Request(string uri);
        IFlurlRequest Request(Url url);
    }
}
