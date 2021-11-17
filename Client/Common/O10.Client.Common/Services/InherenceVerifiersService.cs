using System.Collections.Generic;
using O10.Client.Common.Configuration;
using O10.Client.Common.Interfaces;
using O10.Core.Architecture;

using O10.Core.Configuration;
using Flurl.Http;
using System.Threading.Tasks;
using O10.Client.Common.Dtos;

namespace O10.Client.Common.Services
{
    [RegisterDefaultImplementation(typeof(IInherenceVerifiersService), Lifetime = LifetimeManagement.Singleton)]
    public class InherenceVerifiersService : IInherenceVerifiersService
    {
        private readonly IRestApiConfiguration _restApiConfiguration;
        private readonly IRestClientService _restClientService;

        public InherenceVerifiersService(IRestClientService restClientService, IConfigurationService configurationService)
        {
            _restApiConfiguration = configurationService.Get<IRestApiConfiguration>();
            _restClientService = restClientService;
        }

        public async Task<IEnumerable<InherenceServiceInfoDTO>> GetInherenceServices()
        {
            var res = await _restClientService
                .Request(_restApiConfiguration.InherenceUri)
                .GetJsonAsync<IEnumerable<InherenceServiceInfoDTO>>()
                .ConfigureAwait(false);

            return res;
        }
    }
}
