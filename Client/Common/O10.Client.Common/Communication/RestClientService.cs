using Flurl;
using Flurl.Http;
using System.Net.Http;
using O10.Client.Common.Interfaces;
using O10.Core.Architecture;
using Newtonsoft.Json;
using O10.Core.Serialization;
using Flurl.Http.Configuration;
using Newtonsoft.Json.Converters;

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
            HttpClient httpClient = new(_httpClientHandlerCreationService.GetInsecureHandler());
            FlurlClient flurlClient = new(httpClient);
            flurlClient.Configure(s =>
            {
                var jsonSettings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                    NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                    Formatting = Formatting.Indented,
                };

                jsonSettings.Converters.Add(new StringEnumConverter());
                jsonSettings.Converters.Add(new KeyJsonConverter());
                //jsonSettings.Converters.Add(new ByteArrayJsonConverter());
                s.JsonSerializer = new NewtonsoftJsonSerializer(jsonSettings);
            });

            return uri.WithClient(flurlClient);
        }

        public IFlurlRequest Request(Url url)
        {
            return Request(url.ToString());
        }
    }
}
