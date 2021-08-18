using O10.Client.Common.Communication;
using O10.Client.Common.Communication.Notifications;
using O10.Client.Common.Configuration;
using O10.Client.Common.Interfaces;
using O10.Client.Web.Common.Services;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Core.Models;
using O10.Core.Notifications;
using O10.Crypto.Models;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Ledgers;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace O10.Client.Common.Services
{
    [RegisterDefaultImplementation(typeof(IExecutionScopeService), Lifetime = LifetimeManagement.Scoped)]
    public class StealthExecutionScopeService : IExecutionScopeService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IWitnessPackagesProviderRepository _witnessPackagesProviderRepository;
        private readonly IStealthTransactionsService _transactionsService;
        private readonly IStealthClientCryptoService _clientCryptoService;
        private readonly ILedgerWriterRepository _ledgerWriterRepository;
        private readonly IBoundedAssetsService _relationsBindingService;
        private readonly IWalletSynchronizersRepository _walletSynchronizersRepository;
        private readonly IPacketsExtractorsRepository _packetsExtractorsRepository;
        private readonly IUpdaterRegistry _updaterRegistry;
        private readonly IGatewayService _gatewayService;
        private readonly IRestApiConfiguration _restApiConfiguration;
        private readonly ILogger _logger;

        public StealthExecutionScopeService(
            IServiceProvider serviceProvider,
            IConfigurationService configurationService,
            IWitnessPackagesProviderRepository witnessPackagesProviderRepository,
            IStealthTransactionsService transactionsService,
            IStealthClientCryptoService clientCryptoService,
            ILedgerWriterRepository ledgerWriterRepository,
            IBoundedAssetsService relationsBindingService,
            IWalletSynchronizersRepository walletSynchronizersRepository,
            IPacketsExtractorsRepository packetsExtractorsRepository,
            IUpdaterRegistry updaterRegistry,
            IGatewayService gatewayService,
            ILoggerService loggerService)
        {
            _serviceProvider = serviceProvider;
            _witnessPackagesProviderRepository = witnessPackagesProviderRepository;
            _transactionsService = transactionsService;
            _clientCryptoService = clientCryptoService;
            _ledgerWriterRepository = ledgerWriterRepository;
            _relationsBindingService = relationsBindingService;
            _walletSynchronizersRepository = walletSynchronizersRepository;
            _packetsExtractorsRepository = packetsExtractorsRepository;
            _updaterRegistry = updaterRegistry;
            _gatewayService = gatewayService;
            _restApiConfiguration = configurationService.Get<IRestApiConfiguration>();
            _logger = loggerService.GetLogger(nameof(StealthExecutionScopeService));
        }

        public string Name => "Stealth";

        public T? GetScopeInitializationParams<T>() where T : ScopeInitializationParams
        {
            if (typeof(T) != typeof(UtxoScopeInitializationParams))
            {
                throw new InvalidOperationException($"Only {typeof(UtxoScopeInitializationParams).FullName} can be requested");
            }

            var p = new UtxoScopeInitializationParams();
            return p as T;
        }

        public async Task Initiliaze(ScopeInitializationParams initializationParams)
        {
            if (!(initializationParams is UtxoScopeInitializationParams scopeInitializationParams))
            {
                throw new ArgumentException($"It is expected argument of type {nameof(UtxoScopeInitializationParams)}");
            }
            _logger.SetContext(scopeInitializationParams.AccountId.ToString());
            _logger.Info("Initializong scope service started...");

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

            var ledgerWriter = _ledgerWriterRepository.GetInstance(LedgerType.Stealth);
            await ledgerWriter.Initialize(scopeInitializationParams.AccountId).ConfigureAwait(false);
            _transactionsService.PipeOutTransactions.LinkTo(ledgerWriter.PipeIn);

            IUpdater userIdentitiesUpdater = _updaterRegistry.GetInstance();

            var walletSynchronizer = _walletSynchronizersRepository.GetInstance("Stealth");
            walletSynchronizer.Initialize(scopeInitializationParams.AccountId);

            packetsProvider.PipeOut.LinkTo(utxoWalletPacketsExtractor.GetTargetPipe<WitnessPackageWrapper>());

            utxoWalletPacketsExtractor.GetSourcePipe<TaskCompletionWrapper<TransactionBase>>().LinkTo(walletSynchronizer.GetTargetPipe<TaskCompletionWrapper<TransactionBase>>());
            utxoWalletPacketsExtractor.GetSourcePipe<WitnessPackage>().LinkTo(walletSynchronizer.GetTargetPipe<WitnessPackage>());
            utxoWalletPacketsExtractor.GetSourcePipe<NotificationBase>().LinkTo(userIdentitiesUpdater.PipeInNotifications);

            walletSynchronizer.GetSourcePipe<TransactionBase>().LinkTo(userIdentitiesUpdater.PipeIn);
            walletSynchronizer.GetSourcePipe<NotificationBase>().LinkTo(userIdentitiesUpdater.PipeInNotifications);

            await packetsProvider.Start().ConfigureAwait(false);
            _logger.Info("Initializong scope service finished...");
        }
    }
}
