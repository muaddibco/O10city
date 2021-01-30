using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using O10.Client.Common.Interfaces;
using O10.Client.Common.Services;
using O10.Client.DataLayer.Services;
using O10.Core.Architecture;

using O10.Core.Logging;
using O10.Core.Models;
using O10.Core.Serialization;

namespace O10.Client.Common.Communication
{
	[RegisterExtension(typeof(IWitnessPackagesProvider), Lifetime = LifetimeManagement.Scoped)]
	public class SignalRWitnessPackagesProvider : WitnessPackageProviderBase
	{
		private HubConnection _hubConnection;

        private readonly BlockingCollection<WitnessPackage> _witnessPackages = new BlockingCollection<WitnessPackage>();

		public SignalRWitnessPackagesProvider(IGatewayService gatewayService, IDataAccessService dataAccessService, ILoggerService loggerService)
			: base(gatewayService, dataAccessService, loggerService)
		{
		}

		public override string Name => "SignalR";

		public override async Task Start()
		{
			_logger.Debug($"[{_accountId}]: Starting {nameof(SignalRWitnessPackagesProvider)}");

			await StartHubConnection().ConfigureAwait(false);
		}

		protected override void InitializeInner()
		{
			BuildHubConnection();

			Task.Factory.StartNew(async () =>
			{
				try
				{
					foreach (var w in _witnessPackages.GetConsumingEnumerable(_cancellationToken))
					{
						try
						{
							_logger.LogIfDebug(() => $"[{_accountId}]: processing witness package with {nameof(w.CombinedBlockHeight)}={w.CombinedBlockHeight} while {nameof(_lastObtainedCombinedBlockHeight)}={_lastObtainedCombinedBlockHeight}");
							if (w.CombinedBlockHeight > _lastObtainedCombinedBlockHeight)
							{
								if (w.CombinedBlockHeight - _lastObtainedCombinedBlockHeight > 1)
								{
									await ObtainWitnessesRange(_lastObtainedCombinedBlockHeight + 1, w.CombinedBlockHeight - 1).ConfigureAwait(false);
								}

								await ProcessWitnessPackage(w).ConfigureAwait(false);
							}
							else
							{
								_logger.Error($"[{_accountId}]: Height of RegistryCombinedBlock at obtained packet is {w.CombinedBlockHeight} while last one is {_lastObtainedCombinedBlockHeight}");
							}

						}
						catch (Exception ex)
						{
							_logger.Error($"[{_accountId}]: failure during processing witness packages", ex);
						}
					}
				}
				catch (OperationCanceledException)
				{
					_logger.Info($"[{_accountId}]: {nameof(SignalRWitnessPackagesProvider)} stopped");
				}
				catch (Exception ex)
				{
					_logger.Error($"[{_accountId}]: {nameof(SignalRWitnessPackagesProvider)} failed", ex);
				}
			}, _cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
		}

		private void BuildHubConnection()
		{
			string signalrHubUri = _gatewayService.GetNotificationsHubUri();
			_hubConnection = new HubConnectionBuilder().WithUrl(signalrHubUri).Build();

			_logger.Info($"[{_accountId}]: SignalRPacketsProvider created instance of hubConnection to URI {signalrHubUri}");

			_hubConnection.Closed += async (error) =>
			{
				_logger.Error($"[{_accountId}]: SignalRPacketsProvider hubConnection closed with error '{error?.Message}', reconnecting: {!_cancellationToken.IsCancellationRequested}", error);

				if (!_cancellationToken.IsCancellationRequested)
				{
					await Task.Delay(new Random().Next(0, 5) * 1000, _cancellationToken).ConfigureAwait(false);

					if (!_cancellationToken.IsCancellationRequested)
					{
						await StartHubConnection().ConfigureAwait(false);
					}
				}
			};

			_hubConnection.On<WitnessPackage>("PacketsUpdate", w =>
			{
				_logger.LogIfDebug(() => $"[{_accountId}]: obtained from gateway {nameof(w.CombinedBlockHeight)}={w.CombinedBlockHeight}");
				_witnessPackages.Add(w);
			});
		}

		private async Task ProcessWitnessPackage(WitnessPackage w)
        {
			_logger.LogIfDebug(() => $"[{_accountId}]: {nameof(ProcessWitnessPackage)} {JsonConvert.SerializeObject(w, new ByteArrayJsonConverter())}");

            _lastObtainedCombinedBlockHeight = w.CombinedBlockHeight;
            WitnessPackageWrapper wrapper = new WitnessPackageWrapper(w);
            await Propagator.SendAsync(wrapper).ConfigureAwait(false);

            bool res = await wrapper.CompletionSource.Task.ConfigureAwait(false);
        }

        private async Task StartHubConnection()
        {
			_logger.Info($"[{_accountId}]: starting {nameof(StartHubConnection)}");

			await AscertainAccountIsUpToDate().ConfigureAwait(false);

			_logger.Info($"[{_accountId}]: SignalRPacketsProvider hubConnection connecting");
            await (await _hubConnection.StartAsync(_cancellationToken).ContinueWith(async t =>
            {
                if(t.IsCompletedSuccessfully)
                {
					_logger.Info($"[{_accountId}]: SignalRPacketsProvider hubConnection connected");
                }
                else
                {
					if (t.Exception != null && t.Exception.InnerExceptions != null)
					{
						foreach (Exception exception in t.Exception.InnerExceptions)
						{
							_logger.Error($"[{_accountId}]: Failure during establishing connection with Gateway", exception);
						}
					}
					_logger.Error($"[{_accountId}]: Failure during establishing connection with Gateway. Reconnecting");

					BuildHubConnection();

					await Task.Delay(1000).ConfigureAwait(false);
					await StartHubConnection().ConfigureAwait(false);
                }
            }, TaskScheduler.Default).ConfigureAwait(false)).ConfigureAwait(false);
        }

		protected override async Task OnStop()
		{
			await _hubConnection.StopAsync().ConfigureAwait(false);
			await _hubConnection.DisposeAsync().ConfigureAwait(false);
			_hubConnection = null;
		}
	}
}
