using O10.Client.Common.Communication;
using O10.Client.Common.Configuration;
using O10.Client.Common.Interfaces;
using O10.Client.Common.Services;
using O10.Client.DataLayer.Enums;
using O10.Client.Stealth.Egress;
using O10.Client.Web.Common.Services;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.Logging;
using O10.Core.Models;
using O10.Core.Notifications;
using O10.Crypto.Models;
using O10.Transactions.Core.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace O10.Client.Stealth
{
    [RegisterDefaultImplementation(typeof(IExecutionScopeService), Lifetime = LifetimeManagement.Scoped)]
    public class ExecutionScopeService : IExecutionScopeService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IWitnessPackagesProviderRepository _witnessPackagesProviderRepository;
        private readonly IStealthTransactionsService _transactionsService;
        private readonly IStealthClientCryptoService _clientCryptoService;
        private readonly ILedgerWriterRepository _ledgerWriterRepository;
        private readonly IBoundedAssetsService _relationsBindingService;
        private readonly ISynchronizersRepository _walletSynchronizersRepository;
        private readonly IPacketsExtractorsRepository _packetsExtractorsRepository;
        private readonly IUpdaterRegistry _updaterRegistry;
        private readonly IGatewayService _gatewayService;
        private readonly IRestApiConfiguration _restApiConfiguration;
        private readonly ILogger _logger;

        public ExecutionScopeService(
            IServiceProvider serviceProvider,
            IConfigurationService configurationService,
            IWitnessPackagesProviderRepository witnessPackagesProviderRepository,
            IStealthTransactionsService transactionsService,
            IStealthClientCryptoService clientCryptoService,
            ILedgerWriterRepository ledgerWriterRepository,
            IBoundedAssetsService relationsBindingService,
            ISynchronizersRepository walletSynchronizersRepository,
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
            _logger = loggerService.GetLogger(nameof(ExecutionScopeService));
        }

        public AccountType AccountType => AccountType.User;

        public async Task Initiliaze(ScopeInitializationParams initializationParams)
        {
            if (initializationParams is not StealthScopeInitializationParams scopeInitializationParams)
            {
                throw new ArgumentException($"It is expected argument of type {nameof(StealthScopeInitializationParams)}");
            }
            _logger.SetContext(scopeInitializationParams.AccountId.ToString());
            _logger.Info("Initializong scope service started...");

            IWitnessPackagesProvider packetsProvider = _witnessPackagesProviderRepository.GetInstance(_restApiConfiguration.WitnessProviderName);
            IPacketsExtractor utxoWalletPacketsExtractor = _packetsExtractorsRepository.GetInstance("StealthWallet");

            CancellationTokenSource cancellationTokenSource = new();

            await packetsProvider.Initialize(scopeInitializationParams.AccountId, cancellationTokenSource.Token);
            _clientCryptoService.Initialize(scopeInitializationParams.SecretSpendKey, scopeInitializationParams.SecretViewKey);

            TaskCompletionSource<byte[]> pwdSource = new();
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
