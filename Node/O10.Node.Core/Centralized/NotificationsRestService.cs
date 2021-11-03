using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using O10.Core.Architecture;
using O10.Core.Logging;
using O10.Node.Core.Centralized;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using O10.Node.Core.DataLayer;
using O10.Core.Models;
using System.Net.Http;
using O10.Core.Serialization;
using O10.Transactions.Core.DTOs;
using O10.Core.Persistency;
using O10.Network.Handlers;

namespace O10.Node.Worker.Services
{
    [RegisterDefaultImplementation(typeof(INotificationsService), Lifetime = LifetimeManagement.Scoped)]
	public class NotificationsRestService : INotificationsService, IDisposable
	{
		private readonly IRealTimeRegistryService _realTimeRegistryService;
        private readonly DataAccessService _dataAccessService;
		private readonly ILogger _logger;
		private List<string> _gatewayUris;
        private bool _disposedValue;

        public NotificationsRestService(IRealTimeRegistryService realTimeRegistryService,
                                        IHandlingFlowContext handlingFlowContext,
                                        IDataAccessServiceRepository dataAccessServiceRepository,
                                        ILoggerService loggerService)
		{
            if (dataAccessServiceRepository is null)
            {
                throw new ArgumentNullException(nameof(dataAccessServiceRepository));
            }

            _dataAccessService = dataAccessServiceRepository.GetInstance<DataAccessService>();
			_realTimeRegistryService = realTimeRegistryService;
            _logger = loggerService.GetLogger($"{nameof(NotificationsRestService)}#{handlingFlowContext.Index}");
            _logger.Debug(() => $"Creating {nameof(NotificationsRestService)}");
        }

        public async Task Initialize(CancellationToken cancellationToken)
		{
			_logger.Debug(() => $"Initializing {nameof(NotificationsRestService)} started...");
			await UpdateGateways(cancellationToken);

			if(_gatewayUris.Count == 0)
			{
				_logger.Warning("No gateways registered yet");
			}

			Task.Factory.StartNew(async c =>
			{
				using var httpClientHandler = new HttpClientHandler
				{
					ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
				};
				using HttpClient httpClient = new HttpClient(httpClientHandler);

				using FlurlClient flurlClient = new FlurlClient(httpClient);

				_logger.Debug(() => $"Starting consuming registry packets...");
				foreach (var item in _realTimeRegistryService.GetRegistryConsumingEnumerable((CancellationToken)c))
				{
					try
					{
						RtPackage rtPackage = new RtPackage
						{
							AggregatedRegistrations = item.Item1,
							FullRegistrations = item.Item2,
						};

						_logger.Debug("Propagating package to Gateways");

						foreach (var gatewayUri in _gatewayUris)
						{
							_logger.LogIfDebug(() => $"Sending to gateway {gatewayUri} packet {JsonConvert.SerializeObject(rtPackage, new ByteArrayJsonConverter(), new KeyJsonConverter())}");
                            try
                            {
								var response = await gatewayUri.WithClient(flurlClient).AppendPathSegment("PackageUpdate").PostJsonAsync(rtPackage);
                                {
                                    if (response.ResponseMessage.IsSuccessStatusCode)
                                    {
										_logger.Debug($"Posting PackageUpdate to {response.ResponseMessage.RequestMessage.RequestUri} succeeded");
									}
									else
                                    {
										_logger.Error($"Posting PackageUpdate ended with HttpStatus {response.ResponseMessage.StatusCode}, reason phrase \"{response.ResponseMessage.ReasonPhrase}\" and content \"{response.ResponseMessage.Content.ReadAsStringAsync().Result}\"");
									}
                                };
                            }
                            catch (Exception ex)
                            {
                                _logger.Error($"Failure during posting update to Gateway {gatewayUri}", ex);
                            }
						}
					}
					catch (Exception ex)
					{
						_logger.Error("Failure during propagating package", ex);
					}
				}
			}, cancellationToken, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default).ConfigureAwait(false);
			_logger.Debug(() => $"Initializing {nameof(NotificationsRestService)} completed");
		}

		public async Task UpdateGateways(CancellationToken cancellationToken) => _gatewayUris = (await _dataAccessService.GetGateways(cancellationToken)).Select(g => g.BaseUri).ToList();

		public async Task GatewaysConnectivityCheck(InfoMessage message)
        {
            using var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
            };
            using HttpClient httpClient = new HttpClient(httpClientHandler);

            using FlurlClient flurlClient = new FlurlClient(httpClient);

            foreach (var gatewayUri in _gatewayUris)
            {
                try
                {
                    await gatewayUri.WithClient(flurlClient).AppendPathSegment("CheckConnectivity").PostJsonAsync(message).ConfigureAwait(false);
                }
                catch (AggregateException ex)
                {
                    _logger.Error(ex.InnerException.Message);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _logger.Debug(() => $"Stopping {nameof(NotificationsRestService)}...");
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~NotificationsRestService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
