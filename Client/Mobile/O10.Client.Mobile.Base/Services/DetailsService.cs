using Flurl.Http;
using System;
using System.Threading.Tasks;
using O10.Client.Common.Entities;
using O10.Client.Common.Interfaces;
using O10.Core.Architecture;
using O10.Core.Logging;
using O10.Client.Mobile.Base.Interfaces;
using O10.Client.Mobile.Base.Models;

namespace O10.Client.Mobile.Base.Services
{
    [RegisterDefaultImplementation(typeof(IDetailsService), Lifetime = LifetimeManagement.Singleton)]
    public class DetailsService : IDetailsService
    {
        private readonly ILogger _logger;
        private readonly IRestClientService _restClientService;

        public DetailsService(IRestClientService restClientService, ILoggerService loggerService)
        {
            _logger = loggerService.GetLogger(nameof(DetailsService));
            _restClientService = restClientService;
        }

        public async Task<IssuerActionDetails> GetActionDetails(string uri)
        {
            IssuerActionDetails actionDetails = null;

            await _restClientService.Request(uri).GetJsonAsync<IssuerActionDetails>().ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully)
                {
                    actionDetails = t.Result;
                }
                else
                {
                    _logger.Error($"Failed to obtain details from URI {uri}", t.Exception);
                }
            }, TaskScheduler.Current).ConfigureAwait(false);

            return actionDetails;
        }

        public async Task<ServiceProviderActionAndValidations> GetServiceProviderActionAndValidations(string uri)
        {
            ServiceProviderActionAndValidations serviceProviderActionAndValidations = null;

            try
            {
                serviceProviderActionAndValidations = await _restClientService.Request(uri).GetJsonAsync<ServiceProviderActionAndValidations>().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to obtain Service Provider Action and Validations by URI {uri}", ex);
            }

            return serviceProviderActionAndValidations;
        }
    }
}
