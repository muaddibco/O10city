using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using O10.Client.Common.Crypto;
using O10.Client.Common.Interfaces;
using O10.Core.Architecture;
using O10.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using O10.Client.Common.Entities;
using Flurl.Http;
using O10.Client.Common.Configuration;
using O10.Core.Configuration;
using System.Collections.Generic;
using Plugin.Fingerprint;
using Xamarin.Essentials;
using O10.Core.ExtensionMethods;
using O10.Crypto.ConfidentialAssets;
using System.Text;
using Plugin.Fingerprint.Abstractions;
using O10.Client.Mobile.Base.Resx;
using O10.Client.Mobile.Base.Interfaces;
using O10.Core.Communication;

namespace O10.Client.Mobile.Base.Services
{
    [RegisterDefaultImplementation(typeof(IExecutionContext), Lifetime = LifetimeManagement.Singleton)]
    public class ExecutionContext : IExecutionContext
    {
        private readonly ILogger _logger;
        private readonly IRestClientService _restClientService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRestApiConfiguration _restApiConfiguration;
        private readonly TransformBlock<bool, bool> _initializationCompleted;
        private CancellationTokenSource _cancellationTokenSource;

        private readonly Dictionary<string, TaskCompletionSource<byte[]>> _issuerBindingKeys;
        private readonly Dictionary<string, DateTime> _issuerBindingKeyTimes;

        public ExecutionContext(IGatewayService gatewayService,
                                IRestClientService restClientService,
                                IBoundedAssetsService relationsBindingService,
                                IConfigurationService configurationService,
                                IServiceProvider serviceProvider,
                                ILoggerService loggerService)
        {
            _logger = loggerService.GetLogger(GetType().Name);
            GatewayService = gatewayService;
            _restClientService = restClientService;
            _serviceProvider = serviceProvider;
            RelationsBindingService = relationsBindingService;
            _restApiConfiguration = configurationService.Get<IRestApiConfiguration>();
            _initializationCompleted = new TransformBlock<bool, bool>(b => b);
            _issuerBindingKeys = new Dictionary<string, TaskCompletionSource<byte[]>>();
            _issuerBindingKeyTimes = new Dictionary<string, DateTime>();
            NavigationPipe = new TransformBlock<string, string>(s => s);
        }

        public bool IsInitialized { get; private set; }
        public long AccountId { get; private set; }
        public string LastExpandedKey { get; set; }
        public IBoundedAssetsService RelationsBindingService { get; private set; }
        public IStealthTransactionsService TransactionsService { get; private set; }
        public IStealthClientCryptoService ClientCryptoService { get; private set; }
        public ISourceBlock<bool> InitializationCompleted => _initializationCompleted;
        public IPropagatorBlock<string, string> NavigationPipe { get; }

        public IGatewayService GatewayService { get; }

        public void InitializeUtxoExecutionServices(long accountId, byte[] secretSpendKey, byte[] secretViewKey)
        {
            _logger.Info($"Starting {nameof(InitializeUtxoExecutionServices)} for account with id {accountId}");

            AccountId = accountId;

            try
            {
                IStealthTransactionsService transactionsService = _serviceProvider.GetService<IStealthTransactionsService>();
                IStealthClientCryptoService clientCryptoService = ActivatorUtilities.CreateInstance<StealthClientCryptoService>(_serviceProvider);

                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                clientCryptoService.Initialize(secretSpendKey, secretViewKey);
                transactionsService.Initialize(accountId, clientCryptoService, RelationsBindingService);
                transactionsService.GetSourcePipe<Tuple<string, IPacketProvider, IPacketProvider>>().LinkTo(GatewayService.PipeInTransactions);

                _cancellationTokenSource = cancellationTokenSource;
                ClientCryptoService = clientCryptoService;
                TransactionsService = transactionsService;

                _initializationCompleted.SendAsync(true);
                IsInitialized = true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failure during {nameof(InitializeUtxoExecutionServices)} for account with id {accountId}", ex);
                _initializationCompleted.SendAsync(false);
                throw;
            }
        }

        public void UnregisterExecutionServices()
        {
            IsInitialized = false;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
        }

        public async Task<IssuerActionDetails> GetActionDetails(string uri)
        {
            IssuerActionDetails actionDetails = null;

            await _restClientService.Request(uri).GetJsonAsync<IssuerActionDetails>().ContinueWith(t =>
            {
                if (t.IsCompleted && !t.IsFaulted)
                {
                    actionDetails = t.Result;
                }
            }, TaskScheduler.Current).ConfigureAwait(false);
            return actionDetails;
        }

        public TaskCompletionSource<byte[]> GenerateBindingKey(string key, string seed)
        {
            if (_issuerBindingKeyTimes.ContainsKey(key))
            {
                _issuerBindingKeyTimes[key] = DateTime.Now;
            }
            else
            {
                _issuerBindingKeyTimes.Add(key, DateTime.Now);
            }

            if (_issuerBindingKeys.ContainsKey(key))
            {
                _issuerBindingKeys.Remove(key);
            }

            _issuerBindingKeys.Add(key, new TaskCompletionSource<byte[]>());

            SyncWithSecureStore(key, seed);

            return _issuerBindingKeys[key];

            async Task SyncWithSecureStore(string key, string seed)
            {
                string seedControlStr = await SecureStorage.GetAsync($"{key}.ctrl").ConfigureAwait(false);
                string seedControl = CryptoHelper.FastHash256(Encoding.UTF8.GetBytes(seed)).ToHexString();
                byte[] bindingKey = null;

                if (seedControl.Equals(seedControlStr, StringComparison.InvariantCultureIgnoreCase))
                {
                    string bindingKeyStr = await SecureStorage.GetAsync(key).ConfigureAwait(false);
                    bindingKey = bindingKeyStr?.HexStringToByteArray();
                }

                if ((bindingKey?.Length ?? 0) == 0)
                {
                    bindingKey = RelationsBindingService.GetBindingKey(seed);
                    await SecureStorage.SetAsync($"{key}.ctrl", seedControl).ConfigureAwait(false);
                    await SecureStorage.SetAsync(key, bindingKey.ToHexString()).ConfigureAwait(false);
                }

                _issuerBindingKeys[key].SetResult(bindingKey);
            }
        }

        public TaskCompletionSource<byte[]> GetBindingKeySource(string pwd)
        {
            TaskCompletionSource<byte[]> bindingKeySource = new TaskCompletionSource<byte[]>();

            Task.Factory.StartNew(o =>
            {
                byte[] bindingKey = RelationsBindingService.GetBindingKey(pwd);
                ((TaskCompletionSource<byte[]>)o).SetResult(bindingKey);
            }, bindingKeySource);

            return bindingKeySource;
        }

        public TaskCompletionSource<byte[]> GetIssuerBindingKeySource(string key)
        {
            if (_issuerBindingKeys.ContainsKey(key) &&
                _issuerBindingKeyTimes.ContainsKey(key) && DateTime.Now.Subtract(_issuerBindingKeyTimes[key]).TotalSeconds < 300)
            {
                return _issuerBindingKeys[key];
            }

            return null;
        }

        public bool IsBindingKeyValid(string key) => _issuerBindingKeys.ContainsKey(key)
                                                     && _issuerBindingKeyTimes.ContainsKey(key)
                                                     && DateTime.Now.Subtract(_issuerBindingKeyTimes[key]).TotalSeconds < 300;

        public async Task<TaskCompletionSource<byte[]>> GetBindingKeySourceWithBio(string key)
        {
            //if((!_issuerBindingKeyTimes.ContainsKey(key) 
            //    || DateTime.Now.Subtract(_issuerBindingKeyTimes[key]).TotalSeconds > 300)
            //    && await CrossFingerprint.Current.IsAvailableAsync(false).ConfigureAwait(false)
            //    && !string.IsNullOrEmpty(await SecureStorage.GetAsync($"{key}.ctrl").ConfigureAwait(false)))
            if (await CrossFingerprint.Current.IsAvailableAsync(false).ConfigureAwait(false)
                && !string.IsNullOrEmpty(await SecureStorage.GetAsync($"{key}.ctrl").ConfigureAwait(false)))
            {
                AuthenticationRequestConfiguration requestConfiguration = new AuthenticationRequestConfiguration(AppResources.CAP_AUTH_TITLE, AppResources.CAP_AUTH_INVITE);
                var authResult = await CrossFingerprint.Current.AuthenticateAsync(requestConfiguration).ConfigureAwait(false);
                if (authResult.Authenticated)
                {
                    _issuerBindingKeyTimes[key] = DateTime.Now;

                    string bindingKeyStr = await SecureStorage.GetAsync(key).ConfigureAwait(false);
                    var bindingKey = bindingKeyStr?.HexStringToByteArray();
                    _issuerBindingKeys[key] = new TaskCompletionSource<byte[]>();

                    _issuerBindingKeys[key].SetResult(bindingKey);

                    return GetIssuerBindingKeySource(key);
                }
            }

            return null;
        }
    }
}
