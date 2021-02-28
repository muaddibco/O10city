using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using O10.Client.Common.Communication;
using O10.Client.Common.Crypto;
using O10.Client.Common.Interfaces;
using O10.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using O10.Client.Common.Configuration;
using O10.Core.Configuration;
using System.Linq;
using O10.Client.DataLayer.Enums;
using O10.Core.Models;
using O10.Client.Common.Communication.SynchronizerNotifications;
using O10.Transactions.Core.Ledgers;

namespace O10.Client.Mobile.Base.Services
{
    public class BackgroundSynchronizer
    {
        private readonly ILogger _logger;
        private readonly IAccountsService _accountsService;
        private readonly IWitnessPackagesProviderRepository _witnessPackagesProviderRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRestApiConfiguration _restApiConfiguration;

        private long _accountId;
        private IWitnessPackagesProvider _packetsProvider;
        private IStealthClientCryptoService _clientCryptoService;
        private StealthWalletSynchronizer _walletSynchronizer;
        private UserIdentitiesUpdater _userIdentitiesUpdater;
        private CancellationTokenSource _cancellationTokenSource;

        public BackgroundSynchronizer(
            IAccountsService accountsService, IWitnessPackagesProviderRepository witnessPackagesProviderRepository,
            IConfigurationService configurationService, IServiceProvider serviceProvider, ILoggerService loggerService)
        {
            _logger = loggerService.GetLogger(GetType().Name);
            _accountsService = accountsService;
            _witnessPackagesProviderRepository = witnessPackagesProviderRepository;
            _serviceProvider = serviceProvider;
            _restApiConfiguration = configurationService.Get<IRestApiConfiguration>();
            InitializedEvent = new ManualResetEventSlim();
        }

        public StealthWalletPacketsExtractor PacketsExtractor { get; private set; }

        public ManualResetEventSlim InitializedEvent { get; }

        public bool IsAccountCompromized()
        {
            return _accountsService.GetById(_accountId)?.IsCompromised ?? false;
        }

        public void Initialize()
        {
            _accountId = _accountsService.GetAll().FirstOrDefault()?.AccountId ?? 0;
            if (_accountId == 0)
            {
                _accountId = _accountsService.Create(AccountType.User);
            }

            var account = _accountsService.GetById(_accountId);

            _packetsProvider = _witnessPackagesProviderRepository.GetInstance(_restApiConfiguration.WitnessProviderName);
            _clientCryptoService = ActivatorUtilities.CreateInstance<StealthClientCryptoService>(_serviceProvider);
            PacketsExtractor = ActivatorUtilities.CreateInstance<StealthWalletPacketsExtractor>(_serviceProvider);
            _walletSynchronizer = ActivatorUtilities.CreateInstance<StealthWalletSynchronizer>(_serviceProvider);
            _userIdentitiesUpdater = ActivatorUtilities.CreateInstance<UserIdentitiesUpdater>(_serviceProvider);
            _cancellationTokenSource = new CancellationTokenSource();

            _packetsProvider.Initialize(_accountId, _cancellationTokenSource.Token);
            _clientCryptoService.Initialize(account.SecretSpendKey, account.SecretViewKey);
            PacketsExtractor.Initialize(_accountId);
            _walletSynchronizer.Initialize(_accountId);
            _userIdentitiesUpdater.Initialize(_accountId);

            PacketsExtractor.GetSourcePipe<NotificationBase>().LinkTo(_userIdentitiesUpdater.PipeInNotifications);
            PacketsExtractor.GetSourcePipe<TaskCompletionWrapper>().LinkTo(_walletSynchronizer.GetTargetPipe<TaskCompletionWrapper>());
            PacketsExtractor.GetSourcePipe<WitnessPackage>().LinkTo(_walletSynchronizer.GetTargetPipe<WitnessPackage>());

            _walletSynchronizer.GetSourcePipe<PacketBase>().LinkTo(_userIdentitiesUpdater.PipeIn);
            _walletSynchronizer.GetSourcePipe<NotificationBase>().LinkTo(_userIdentitiesUpdater.PipeInNotifications);

            _packetsProvider.PipeOut.LinkTo(PacketsExtractor.GetTargetPipe<WitnessPackageWrapper>());
            _packetsProvider.Start();
            InitializedEvent.Set();
        }

        public void Run()
        {
            _packetsProvider.Start();
        }

        public void Stop()
        {
            InitializedEvent.Reset();
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            _walletSynchronizer?.Dispose();
            _walletSynchronizer = null;
        }
    }
}
