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
using System.Threading;

namespace O10.Client.Common.Communication
{
    [RegisterExtension(typeof(IWitnessPackagesProvider), Lifetime = LifetimeManagement.Scoped)]
    public class SignalRWitnessPackagesProvider : WitnessPackageProviderBase
    {
        private SignalrHubConnection? _hubConnection;

        private readonly BlockingCollection<WitnessPackage> _witnessPackages = new BlockingCollection<WitnessPackage>();

        public SignalRWitnessPackagesProvider(IGatewayService gatewayService, IDataAccessService dataAccessService, ILoggerService loggerService)
            : base(gatewayService, dataAccessService, loggerService)
        {
        }

        public override string Name => "SignalR";

        public override async Task Start()
        {
            _logger.Debug($"[{_accountId}]: #### Starting {nameof(SignalRWitnessPackagesProvider)}");

            await StartHubConnection().ConfigureAwait(false);
            _logger.Debug($"[{_accountId}]: #### Starting {nameof(SignalRWitnessPackagesProvider)} completed");
        }

        public override async Task Restart()
        {
            await BuildHubConnection();
            await StartHubConnection();
        }

        protected override void InitializeInner()
        {
            Task.Factory.StartNew<Task>(async () =>
            {
                await BuildHubConnection();
                await ProcessObtainedPackages().ConfigureAwait(false);
            }, _cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private async Task ProcessObtainedPackages()
        {
            try
            {
                foreach (var w in _witnessPackages.GetConsumingEnumerable(_cancellationToken))
                {
                    await WitnessProcessingSemaphore.WaitAsync();
                    _logger.Debug($"[{_accountId}]: ==============================================>");
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
                            _logger.Warning($"[{_accountId}]: Skip processing - height of RegistryCombinedBlock at obtained packet is {w.CombinedBlockHeight} while last one is {_lastObtainedCombinedBlockHeight}");
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"[{_accountId}]: failure during processing witness packages", ex);
                    }
                    finally
                    {
                        _logger.Debug($"[{_accountId}]: <==============================================");
                        WitnessProcessingSemaphore.Release();
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
        }

        private async Task BuildHubConnection()
        {
            string signalrHubUri = _gatewayService.GetNotificationsHubUri();

            if (_hubConnection != null)
            {
                await _hubConnection.DestroyHubConnection();
            }

            _hubConnection = new SignalrHubConnection(new Uri(signalrHubUri), _accountId.ToString(), _logger, _cancellationToken);
            await _hubConnection.BuildHubConnection();

            _logger.Info($"[{_accountId}]: SignalRPacketsProvider created instance of hubConnection to URI {signalrHubUri}");

            _hubConnection.On<WitnessPackage>("PacketsUpdate", w =>
            {
                _logger.LogIfDebug(() => $"[{_accountId}]: SignalR - obtained from gateway {nameof(w.CombinedBlockHeight)}={w.CombinedBlockHeight}");
                _witnessPackages.Add(w);
            });
        }

        private async Task ProcessWitnessPackage(WitnessPackage w)
        {
            _logger.LogIfDebug(() => $"[{_accountId}]: {nameof(ProcessWitnessPackage)} {JsonConvert.SerializeObject(w, new ByteArrayJsonConverter())}");

            _lastObtainedCombinedBlockHeight = w.CombinedBlockHeight;
            WitnessPackageWrapper wrapper = new WitnessPackageWrapper(w);
            await Propagator.SendAsync(wrapper).ConfigureAwait(false);
            
            _logger.LogIfDebug(() => $"[{_accountId}]: ====> waiting for completion of processing witness package at {w.CombinedBlockHeight}...");
            bool res = await wrapper.CompletionSource.Task.ConfigureAwait(false);
            _logger.LogIfDebug(() => $"[{_accountId}]: <==== processing witness package at {w.CombinedBlockHeight} completed");
        }

        private async Task StartHubConnection()
        {
            _logger.Info($"[{_accountId}]: **** starting {nameof(StartHubConnection)}");
            _logger.LogIfDebug(() => $"[{_accountId}]: {nameof(StartHubConnection)} called from the {(new System.Diagnostics.StackTrace()).GetFrame(3).GetMethod().Name}...");

            await AscertainAccountIsUpToDate().ConfigureAwait(false);

            await _hubConnection.StartHubConnection();
        }

        protected override Task OnStop()
        {
            return Task.CompletedTask;
        }
    }
}
