using System.Net.Http;

namespace O10.Client.Common.Interfaces
{
	public interface IHTTPClientHandlerCreationService
	{
		HttpClientHandler GetInsecureHandler();
	}
}
