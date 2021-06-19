using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using O10.Transactions.Core.Ledgers;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;
using O10.Core.Logging;
using O10.Node.Core.Centralized;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using O10.Node.Core.DataLayer;
using O10.Core.DataLayer;
using O10.Core.Models;
using System.Net.Http;
using O10.Core.Serialization;
using O10.Transactions.Core.DTOs;

namespace O10.Node.Worker.Services
{
    [RegisterDefaultImplementation(typeof(INotificationsService), Lifetime = LifetimeManagement.Singleton)]
	public class NotificationsRestService : INotificationsService
	{
		private readonly IRealTimeRegistryService _realTimeRegistryService;
        private readonly DataAccessService _dataAccessService;
		private readonly ILogger _logger;
		private List<string> _gatewayUris;

		public NotificationsRestService(IRealTimeRegistryService realTimeRegistryService, IDataAccessServiceRepository dataAccessServiceRepository, ILoggerService loggerService)
		{
            if (dataAccessServiceRepository is null)
            {
                throw new ArgumentNullException(nameof(dataAccessServiceRepository));
            }

            _dataAccessService = dataAccessServiceRepository.GetInstance<DataAccessService>();
			_realTimeRegistryService = realTimeRegistryService;
			_logger = loggerService.GetLogger(nameof(NotificationsRestService));
		}

		public void Initialize(CancellationToken cancellationToken)
		{
			UpdateGateways();

			if(_gatewayUris.Count == 0)
			{
				_logger.Warning("No gateways registered yet");
			}

			Task.Factory.StartNew(c =>
			{
				using var httpClientHandler = new HttpClientHandler
				{
					ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
				};
				using HttpClient httpClient = new HttpClient(httpClientHandler);

				using FlurlClient flurlClient = new FlurlClient(httpClient);

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
							_logger.LogIfDebug(() => $"Sending to gateway {gatewayUri} packet {JsonConvert.SerializeObject(rtPackage, new ByteArrayJsonConverter())}");
                            try
                            {
                                gatewayUri.WithClient(flurlClient).AppendPathSegment("PackageUpdate").PostJsonAsync(rtPackage).ContinueWith(t =>
                                {
                                    if (t.IsCompletedSuccessfully)
                                    {
										_logger.Debug($"Posting PackageUpdate to {t.Result.ResponseMessage.RequestMessage.RequestUri} succeeded");
									}
									else
                                    {
										_logger.Error($"Posting PackageUpdate ended with HttpStatus {t.Result.ResponseMessage.StatusCode}, reason phrase \"{t.Result.ResponseMessage.ReasonPhrase}\" and content \"{t.Result.ResponseMessage.Content.ReadAsStringAsync().Result}\"", t.Exception);

										if(t.Exception?.InnerExceptions != null)
										{
											foreach (var ex in t.Exception.InnerExceptions)
											{
												_logger.Error($"Failure during posting package to {t.Result.ResponseMessage.RequestMessage.RequestUri}", ex);
											}
										}
									}

                                }, TaskScheduler.Default);
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
			}, cancellationToken, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
		}

		public void UpdateGateways() => _gatewayUris = _dataAccessService.GetGateways().Select(g => g.BaseUri).ToList();

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
	}
}
