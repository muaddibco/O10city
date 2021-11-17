using O10.Client.Common.Communication;
using O10.Client.Common.Configuration;
using O10.Client.Common.Interfaces;
using O10.Client.Common.Services;
using O10.Client.DataLayer.Enums;
using O10.Client.State;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.Models;
using O10.Crypto.Models;
using O10.Transactions.Core.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace O10.Client.Web.Common.Services
{
    [RegisterExtension(typeof(IExecutionScopeService), Lifetime = LifetimeManagement.Scoped)]
    public class ExecutionScopeService : IExecutionScopeService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceProviderTransactionsService _transactionsService;
        private readonly IStateClientCryptoService _clientCryptoService;
        private readonly ILedgerWriterRepository _ledgerWriterRepository;
        private readonly IWitnessPackagesProviderRepository _witnessPackagesProviderRepository;
        private readonly ISynchronizersRepository _walletSynchronizersRepository;
        private readonly IPacketsExtractorsRepository _packetsExtractorsRepository;
        private readonly IUpdaterRegistry _updaterRegistry;
        private readonly IGatewayService _gatewayService;
        private readonly IRestApiConfiguration _restApiConfiguration;

        public ExecutionScopeService(
            IServiceProvider serviceProvider,
            IConfigurationService configurationService,
            IServiceProviderTransactionsService transactionsService,
            IStateClientCryptoService clientCryptoService,
            ILedgerWriterRepository ledgerWriterRepository,
            IWitnessPackagesProviderRepository witnessPackagesProviderRepository,
            ISynchronizersRepository walletSynchronizersRepository,
            IPacketsExtractorsRepository packetsExtractorsRepository,
            IUpdaterRegistry updaterRegistry,
            IGatewayService gatewayService)
        {
            _serviceProvider = serviceProvider;
            _transactionsService = transactionsService;
            _clientCryptoService = clientCryptoService;
            _ledgerWriterRepository = ledgerWriterRepository;
            _witnessPackagesProviderRepository = witnessPackagesProviderRepository;
            _walletSynchronizersRepository = walletSynchronizersRepository;
            _packetsExtractorsRepository = packetsExtractorsRepository;
            _updaterRegistry = updaterRegistry;
            _gatewayService = gatewayService;
            _restApiConfiguration = configurationService.Get<IRestApiConfiguration>();
        }

        public AccountType AccountType => AccountType.ServiceProvider;

        public async Task Initiliaze(ScopeInitializationParams initializationParams)
        {
            if (initializationParams is not StateScopeInitializationParams scopeInitializationParams)
            {
                throw new ArgumentException($"It is expected argument of type {nameof(StateScopeInitializationParams)}");
            }

            IWitnessPackagesProvider packetsProvider = _witnessPackagesProviderRepository.GetInstance(_restApiConfiguration.WitnessProviderName);
            IPacketsExtractor statePacketsExtractor = _packetsExtractorsRepository.GetInstance("State");

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            await packetsProvider.Initialize(scopeInitializationParams.AccountId, cancellationTokenSource.Token);
            _clientCryptoService.Initialize(scopeInitializationParams.SecretKey);
            await _transactionsService.Initialize(scopeInitializationParams.AccountId).ConfigureAwait(false);
            var ledgerWriter = _ledgerWriterRepository.GetInstance(LedgerType.O10State);
            await ledgerWriter.Initialize(scopeInitializationParams.AccountId).ConfigureAwait(false);
            _transactionsService.PipeOutTransactions.LinkTo(ledgerWriter.PipeIn);
            statePacketsExtractor.Initialize(scopeInitializationParams.AccountId);

            IUpdater updater = _updaterRegistry.GetInstance();

            var walletSynchronizer = _walletSynchronizersRepository.GetInstance("State");
            walletSynchronizer.Initialize(scopeInitializationParams.AccountId);

            packetsProvider.PipeOut.LinkTo(statePacketsExtractor.GetTargetPipe<WitnessPackageWrapper>());
            statePacketsExtractor.GetSourcePipe<TaskCompletionWrapper<TransactionBase>>()
                                 .LinkTo(walletSynchronizer.GetTargetPipe<TaskCompletionWrapper<TransactionBase>>());
            statePacketsExtractor.GetSourcePipe<WitnessPackage>()
                                 .LinkTo(walletSynchronizer.GetTargetPipe<WitnessPackage>());

            walletSynchronizer.GetSourcePipe<TransactionBase>().LinkTo(
                new ActionBlock<TransactionBase>(async p => await updater.PipeIn.SendAsync(p).ConfigureAwait(false)));

            await packetsProvider.Start().ConfigureAwait(false);
        }
    }
}
