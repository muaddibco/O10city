using O10.Client.Common.Communication;
using O10.Client.Common.Configuration;
using O10.Client.Common.Interfaces;
using O10.Client.Common.Services;
using O10.Core.Architecture;
using O10.Core.Communication;
using O10.Core.Configuration;
using O10.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace O10.Client.Web.Common.Services
{
    [RegisterExtension(typeof(IExecutionScopeService), Lifetime = LifetimeManagement.Scoped)]
    public class StateExecutionScopeService : IExecutionScopeService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IStateTransactionsService _transactionsService;
        private readonly IStateClientCryptoService _clientCryptoService;
        private readonly IWitnessPackagesProviderRepository _witnessPackagesProviderRepository;
        private readonly IWalletSynchronizersRepository _walletSynchronizersRepository;
        private readonly IPacketsExtractorsRepository _packetsExtractorsRepository;
        private readonly IExternalUpdatersRepository _externalUpdatersRepository;
        private readonly IUpdaterRegistry _updaterRegistry;
        private readonly IGatewayService _gatewayService;
        private readonly IRestApiConfiguration _restApiConfiguration;

        public StateExecutionScopeService(
            IServiceProvider serviceProvider,
            IConfigurationService configurationService,
            IStateTransactionsService transactionsService,
            IStateClientCryptoService clientCryptoService,
            IWitnessPackagesProviderRepository witnessPackagesProviderRepository,
            IWalletSynchronizersRepository walletSynchronizersRepository,
            IPacketsExtractorsRepository packetsExtractorsRepository,
            IExternalUpdatersRepository externalUpdatersRepository,
            IUpdaterRegistry updaterRegistry,
            IGatewayService gatewayService)
        {
            _serviceProvider = serviceProvider;
            _transactionsService = transactionsService;
            _clientCryptoService = clientCryptoService;
            _witnessPackagesProviderRepository = witnessPackagesProviderRepository;
            _walletSynchronizersRepository = walletSynchronizersRepository;
            _packetsExtractorsRepository = packetsExtractorsRepository;
            _externalUpdatersRepository = externalUpdatersRepository;
            _updaterRegistry = updaterRegistry;
            _gatewayService = gatewayService;
            _restApiConfiguration = configurationService.Get<IRestApiConfiguration>();
        }

        public string Name => "State";

        public T GetScopeInitializationParams<T>() where T: ScopeInitializationParams
        {
            if (typeof(T) != typeof(StateScopeInitializationParams))
            {
                throw new InvalidOperationException($"Only {typeof(StateScopeInitializationParams).FullName} can be requested");
            }

            return new StateScopeInitializationParams() as T;
        }

        public void Initiliaze(ScopeInitializationParams initializationParams)
        {
            if (!(initializationParams is StateScopeInitializationParams scopeInitializationParams))
            {
                throw new ArgumentException($"It is expected argument of type {nameof(StateScopeInitializationParams)}");
            }

            IWitnessPackagesProvider packetsProvider = _witnessPackagesProviderRepository.GetInstance(_restApiConfiguration.WitnessProviderName);
            IPacketsExtractor statePacketsExtractor = _packetsExtractorsRepository.GetInstance("State");

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            packetsProvider.Initialize(scopeInitializationParams.AccountId, cancellationTokenSource.Token);
            _clientCryptoService.Initialize(scopeInitializationParams.SecretKey);
            _transactionsService.Initialize(scopeInitializationParams.AccountId);
            _transactionsService.GetSourcePipe<TaskCompletionWrapper<PacketBase>>().LinkTo(_gatewayService.PipeInTransactions);
            statePacketsExtractor.Initialize(scopeInitializationParams.AccountId);

            IUpdater updater = _updaterRegistry.GetInstance();

            var walletSynchronizer = _walletSynchronizersRepository.GetInstance("State");
            walletSynchronizer.Initialize(scopeInitializationParams.AccountId);

            packetsProvider.PipeOut.LinkTo(statePacketsExtractor.GetTargetPipe<WitnessPackageWrapper>());
            statePacketsExtractor.GetSourcePipe<TaskCompletionWrapper<PacketBase>>()
                                 .LinkTo(walletSynchronizer.GetTargetPipe<TaskCompletionWrapper<PacketBase>>());
            statePacketsExtractor.GetSourcePipe<WitnessPackage>()
                                 .LinkTo(walletSynchronizer.GetTargetPipe<WitnessPackage>());

            foreach (var externalUpdater in _externalUpdatersRepository.GetInstances())
            {
                externalUpdater.Initialize(scopeInitializationParams.AccountId);
            }

            walletSynchronizer.GetSourcePipe<PacketBase>().LinkTo(
                new ActionBlock<PacketBase>(async p =>
                {
                    var tasks = new List<Task>
                    {
                        updater.PipeIn.SendAsync(p)
                    };

                    foreach (var externalUpdater in _externalUpdatersRepository.GetInstances())
                    {
                        tasks.Add(externalUpdater.PipeIn.SendAsync(p));
                    }

                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }));

            packetsProvider.Start();
        }
    }
}
