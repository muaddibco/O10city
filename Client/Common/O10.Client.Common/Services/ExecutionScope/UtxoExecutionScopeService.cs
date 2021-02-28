using O10.Client.Common.Communication;
using O10.Client.Common.Communication.Notifications;
using O10.Client.Common.Configuration;
using O10.Client.Common.Interfaces;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.Models;
using O10.Core.Notifications;
using O10.Transactions.Core.Ledgers;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace O10.Client.Common.Services
{
    [RegisterDefaultImplementation(typeof(IExecutionScopeService), Lifetime = LifetimeManagement.Scoped)]
    public class UtxoExecutionScopeService : IExecutionScopeService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IWitnessPackagesProviderRepository _witnessPackagesProviderRepository;
        private readonly IStealthTransactionsService _transactionsService;
        private readonly IStealthClientCryptoService _clientCryptoService;
        private readonly IBoundedAssetsService _relationsBindingService;
        private readonly IWalletSynchronizersRepository _walletSynchronizersRepository;
        private readonly IPacketsExtractorsRepository _packetsExtractorsRepository;
        private readonly IUpdaterRegistry _updaterRegistry;
        private readonly IGatewayService _gatewayService;
        private readonly IRestApiConfiguration _restApiConfiguration;

        public UtxoExecutionScopeService(
            IServiceProvider serviceProvider,
            IConfigurationService configurationService,
            IWitnessPackagesProviderRepository witnessPackagesProviderRepository,
            IStealthTransactionsService transactionsService,
            IStealthClientCryptoService clientCryptoService,
            IBoundedAssetsService relationsBindingService,
            IWalletSynchronizersRepository walletSynchronizersRepository,
            IPacketsExtractorsRepository packetsExtractorsRepository,
            IUpdaterRegistry updaterRegistry,
            IGatewayService gatewayService)
        {
            _serviceProvider = serviceProvider;
            _witnessPackagesProviderRepository = witnessPackagesProviderRepository;
            _transactionsService = transactionsService;
            _clientCryptoService = clientCryptoService;
            _relationsBindingService = relationsBindingService;
            _walletSynchronizersRepository = walletSynchronizersRepository;
            _packetsExtractorsRepository = packetsExtractorsRepository;
            _updaterRegistry = updaterRegistry;
            _gatewayService = gatewayService;
            _restApiConfiguration = configurationService.Get<IRestApiConfiguration>();
        }

        public string Name => "Stealth";

        public T GetScopeInitializationParams<T>() where T: ScopeInitializationParams
        {
            if(typeof(T) != typeof(UtxoScopeInitializationParams))
            {
                throw new InvalidOperationException($"Only {typeof(UtxoScopeInitializationParams).FullName} can be requested");
            }

            var p = new UtxoScopeInitializationParams();
            return p as T;
        }

        public void Initiliaze(ScopeInitializationParams initializationParams)
        {
            if(!(initializationParams is UtxoScopeInitializationParams scopeInitializationParams))
            {
                throw new ArgumentException($"It is expected argument of type {nameof(UtxoScopeInitializationParams)}");
            }

            IWitnessPackagesProvider packetsProvider = _witnessPackagesProviderRepository.GetInstance(_restApiConfiguration.WitnessProviderName);
            IPacketsExtractor utxoWalletPacketsExtractor = _packetsExtractorsRepository.GetInstance("StealthWallet");

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            packetsProvider.Initialize(scopeInitializationParams.AccountId, cancellationTokenSource.Token);
            _clientCryptoService.Initialize(scopeInitializationParams.SecretSpendKey, scopeInitializationParams.SecretViewKey);

            TaskCompletionSource<byte[]> pwdSource = new TaskCompletionSource<byte[]>();
            if (scopeInitializationParams.PwdSecretKey != null)
            {
                pwdSource.SetResult(scopeInitializationParams.PwdSecretKey);
            }
            _relationsBindingService.Initialize(pwdSource);

            _transactionsService.Initialize(scopeInitializationParams.AccountId);
            utxoWalletPacketsExtractor.Initialize(scopeInitializationParams.AccountId);
            _transactionsService.GetSourcePipe<TaskCompletionWrapper<PacketBase>>().LinkTo(_gatewayService.PipeInTransactions);
            _transactionsService.GetSourcePipe<byte[]>().LinkTo(utxoWalletPacketsExtractor.GetTargetPipe<byte[]>());

            IUpdater userIdentitiesUpdater = _updaterRegistry.GetInstance();

            var walletSynchronizer = _walletSynchronizersRepository.GetInstance("Stealth");
            walletSynchronizer.Initialize(scopeInitializationParams.AccountId);

            packetsProvider.PipeOut.LinkTo(utxoWalletPacketsExtractor.GetTargetPipe<WitnessPackageWrapper>());

            utxoWalletPacketsExtractor.GetSourcePipe<TaskCompletionWrapper<PacketBase>>().LinkTo(walletSynchronizer.GetTargetPipe<TaskCompletionWrapper<PacketBase>>());
            utxoWalletPacketsExtractor.GetSourcePipe<WitnessPackage>().LinkTo(walletSynchronizer.GetTargetPipe<WitnessPackage>());
            utxoWalletPacketsExtractor.GetSourcePipe<NotificationBase>().LinkTo(userIdentitiesUpdater.PipeInNotifications);

            walletSynchronizer.GetSourcePipe<PacketBase>().LinkTo(userIdentitiesUpdater.PipeIn);
            walletSynchronizer.GetSourcePipe<NotificationBase>().LinkTo(userIdentitiesUpdater.PipeInNotifications);

            packetsProvider.Start();
        }
    }
}
