using System.Net.Http;
using O10.Client.Common.Interfaces;
using O10.Core.Architecture;


namespace O10.Client.Common.Services
{
    [RegisterDefaultImplementation(typeof(IHTTPClientHandlerCreationService), Lifetime = LifetimeManagement.Singleton)]
    public class HTTPClientHandlerCreationService : IHTTPClientHandlerCreationService
    {
        public HttpClientHandler GetInsecureHandler() => new HttpClientHandler();
    }
}
