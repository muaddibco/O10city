using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using O10.Client.Common.Communication;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Services;
using O10.Core.Logging;
using O10.Core.Models;
using O10.Core.Serialization;
using O10.Transactions.Core.DTOs;
using System.Linq;

namespace O10.Client.Common.Services
{
    public abstract class WitnessPackageProviderBase : IWitnessPackagesProvider
    {
        protected readonly ILogger _logger;
        protected readonly IGatewayService _gatewayService;
        protected readonly IDataAccessService _dataAccessService;
        protected CancellationToken _cancellationToken;
        protected long _accountId;
        protected long _lastObtainedCombinedBlockHeight;

        public WitnessPackageProviderBase(IGatewayService gatewayService, IDataAccessService dataAccessService, ILoggerService loggerService)
        {
            _logger = loggerService.GetLogger(GetType().Name);
            _logger.Info($"{GetType().Name} ctor");

            Propagator = new TransformBlock<WitnessPackageWrapper, WitnessPackageWrapper>(w => w);
            _gatewayService = gatewayService;
            _dataAccessService = dataAccessService;
        }

        public abstract string Name { get; }

        public ISourceBlock<WitnessPackageWrapper> PipeOut
        {
            get
            {
                return Propagator;
            }
        }

        protected IPropagatorBlock<WitnessPackageWrapper, WitnessPackageWrapper> Propagator { get; }

        protected SemaphoreSlim WitnessProcessingSemaphore { get; } = new SemaphoreSlim(1);

        public bool Initialize(long accountId, CancellationToken cancellationToken)
        {
            _accountId = accountId;
            _logger.SetContext(accountId.ToString());
            _logger.Info($"{GetType().Name} Initialize");

            _cancellationToken = cancellationToken;
            _cancellationToken.Register(async () =>
            {
                _logger.Info("stopping");

                try
                {
                    await OnStop().ConfigureAwait(false);

                    PipeOut?.Complete();

                    _logger.Info("stopped");
                }
                catch (Exception ex)
                {
                    _logger.Error("stopping failed", ex);
                }
            });

            _dataAccessService.GetLastUpdatedCombinedBlockHeight(accountId, out _lastObtainedCombinedBlockHeight);

            _logger.Info($"LastObtainedCombinedBlockHeight = {_lastObtainedCombinedBlockHeight}");

            try
            {
                InitializeInner();
            }
            catch (Exception ex)
            {
                _logger.Error("InitializeInner failed", ex);
                return false;
            }

            return true;
        }

        protected async Task ObtainWitnessesRange(long start, long end)
        {
            _logger.Debug($"{nameof(ObtainWitnessesRange)}({start}, {end})");

            IOrderedEnumerable<WitnessPackage>? witnessPackages = null;

            try
            {
                witnessPackages = await _gatewayService.GetWitnessesRange(start, end).ConfigureAwait(false);
            }
            catch (AggregateException ex)
            {
                _logger.Error($"{nameof(ObtainWitnessesRange)} - Failure during obtaning witness range", ex.InnerException);
            }
            catch (Exception ex)
            {
                // TODO: blind exception catch seems improper here!
                _logger.Error($"{nameof(ObtainWitnessesRange)} - Failure during obtaning witness range", ex);
            }

            if (witnessPackages != null)
            {
                try
                {
                    foreach (var item in witnessPackages)
                    {
                        _logger.LogIfDebug(() => $"{nameof(ObtainWitnessesRange)} - witnessPackage = {JsonConvert.SerializeObject(item, new ByteArrayJsonConverter())}");
                        WitnessPackageWrapper wrapper = new WitnessPackageWrapper(item);
                        await Propagator.SendAsync(wrapper).ConfigureAwait(false);

                        _logger.LogIfDebug(() => $"====> {nameof(ObtainWitnessesRange)} - waiting for completion of processing witness package at {item.CombinedBlockHeight}...");
                        bool res = await wrapper.CompletionSource.Task.ConfigureAwait(false);
                        _logger.LogIfDebug(() => $"<==== {nameof(ObtainWitnessesRange)} - processing witness package at {item.CombinedBlockHeight} completed");
                    }

                    _lastObtainedCombinedBlockHeight = end;
                    _logger.Info($"Local height of aggregated transactions adjusted to {end}");
                }
                catch (Exception ex)
                {
                    _logger.Error($"{nameof(ObtainWitnessesRange)} - Failure during processing witnesses at ({start}, {end})", ex);
                    throw;
                }
            }
        }

        protected async Task AscertainAccountIsUpToDate()
        {
            try
            {
                await WitnessProcessingSemaphore.WaitAsync();
                _logger.Debug($"==============================================>");
                _logger.Debug($"{nameof(AscertainAccountIsUpToDate)} - starting...");

                AggregatedRegistrationsTransactionDTO registryCombinedBlockModel = await _gatewayService.GetLastRegistryCombinedBlock().ConfigureAwait(false);

                _logger.LogIfDebug(() => $"{nameof(AscertainAccountIsUpToDate)} - Network's LastAggregatedRegistrations = {JsonConvert.SerializeObject(registryCombinedBlockModel, new ByteArrayJsonConverter())}, local height = {_lastObtainedCombinedBlockHeight}");

                if (registryCombinedBlockModel != null)
                {
                    if (registryCombinedBlockModel.Height - _lastObtainedCombinedBlockHeight > 0)
                    {
                        await ObtainWitnessesRange(_lastObtainedCombinedBlockHeight + 1, registryCombinedBlockModel.Height).ConfigureAwait(false);
                    }
                    else if (registryCombinedBlockModel.Height < _lastObtainedCombinedBlockHeight)
                    {
                        _logger.Warning($"It was detected that local aggregated registrations height [{_lastObtainedCombinedBlockHeight}] is higher than GW's one [{registryCombinedBlockModel.Height}]. Adjusting to GW.");
                        _lastObtainedCombinedBlockHeight = registryCombinedBlockModel.Height;
                        _dataAccessService.StoreLastUpdatedCombinedBlockHeight(_accountId, _lastObtainedCombinedBlockHeight);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failure at {nameof(AscertainAccountIsUpToDate)}", ex);
                throw;
            }
            finally
            {
                _logger.Debug($"{nameof(AscertainAccountIsUpToDate)} - completed");
                _logger.Debug($"<==============================================");
                WitnessProcessingSemaphore.Release();
            }
        }

        public abstract Task Start();
        public abstract Task Restart();

        protected abstract void InitializeInner();

        protected abstract Task OnStop();
    }
}
