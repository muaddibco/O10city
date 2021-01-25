using Flurl;
using Flurl.Http;
using System.Net.Http;
using O10.Client.Common.Interfaces;
using O10.Core.Architecture;


namespace O10.Client.Common.Communication
{
	[RegisterDefaultImplementation(typeof(IRestClientService), Lifetime = LifetimeManagement.Singleton)]
	public class RestClientService : IRestClientService
	{
		private readonly IHTTPClientHandlerCreationService _httpClientHandlerCreationService;

		public RestClientService(IHTTPClientHandlerCreationService httpClientHandlerCreationService)
		{
			_httpClientHandlerCreationService = httpClientHandlerCreationService;
		}
		public IFlurlRequest Request(string uri)
		{
			HttpClient httpClient = new HttpClient(_httpClientHandlerCreationService.GetInsecureHandler());
			FlurlClient flurlClient = new FlurlClient(httpClient);
			return uri.WithClient(flurlClient);

		}

		public IFlurlRequest Request(Url url)
		{
			return Request(url.ToString());
		}
	}
}
