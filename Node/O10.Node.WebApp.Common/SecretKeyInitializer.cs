using System;
using System.Threading;
using O10.Core;
using O10.Core.Architecture;
using O10.Core.Logging;
using O10.Core.ExtensionMethods;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using O10.Core.Configuration;
using O10.Core.Identity;
using Chaos.NaCl;
using System.Net;
using O10.Core.States;
using O10.Node.Core.Common;
using O10.Node.WebApp.Common.Configuration;
using O10.Node.Network.Topology;
using O10.Node.Core.DataLayer;
using O10.Node.Core.DataLayer.DataContexts;
using O10.Crypto.Services;
using O10.Core.Persistency;
using O10.Crypto.Models;

namespace O10.Node.WebApp.Common
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Scoped)]
	public class SecretKeyInitializer : InitializerBase
	{
        private readonly IIdentityKeyProvider _identityKeyProvider;
        private readonly ISigningServicesRepository _signingServicesRepository;
		private readonly INodeContext _nodeContext;
        private readonly ILogger _logger;
        private readonly NodeWebAppConfiguration _nodeWebAppConfiguration;
        private readonly DataAccessService _dataAccessService;

        public SecretKeyInitializer(IStatesRepository statesRepository,
                              ISigningServicesRepository signingServicesRepository,
                              IConfigurationService configurationService,
                              IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
                              IDataAccessServiceRepository dataAccessServiceRepository,
                              ILoggerService loggerService)
		{
            if (statesRepository is null)
            {
                throw new ArgumentNullException(nameof(statesRepository));
            }

            if (configurationService is null)
            {
                throw new ArgumentNullException(nameof(configurationService));
            }

            if (identityKeyProvidersRegistry is null)
            {
                throw new ArgumentNullException(nameof(identityKeyProvidersRegistry));
            }

            if (dataAccessServiceRepository is null)
            {
                throw new ArgumentNullException(nameof(dataAccessServiceRepository));
            }

            if (loggerService is null)
            {
                throw new ArgumentNullException(nameof(loggerService));
            }

            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
            _signingServicesRepository = signingServicesRepository ?? throw new ArgumentNullException(nameof(signingServicesRepository));
			_nodeContext = statesRepository.GetInstance<INodeContext>();
			_nodeWebAppConfiguration = configurationService.Get<NodeWebAppConfiguration>();
            _dataAccessService = dataAccessServiceRepository.GetInstance<DataAccessService>();
            _logger = loggerService.GetLogger(nameof(SecretKeyInitializer));
		}

		public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Highest8;

		protected override async Task InitializeInner(CancellationToken cancellationToken)
		{
            try
            {
                byte[] secretKey = GetSecretKey(cancellationToken);
                ISigningService signingService = _signingServicesRepository.GetInstance(_nodeWebAppConfiguration.SigningServiceName);
                signingService.Initialize(secretKey);
                _nodeContext.Initialize(signingService);

                byte[] keyBytes = Ed25519.PublicKeyFromSeed(secretKey);
                IKey key = _identityKeyProvider.GetKey(keyBytes);

                NodeRecord nodeRecord = _dataAccessService.GetNode(key);

                if (nodeRecord == null)
                {
                    await _dataAccessService.RemoveNodeByIp(IPAddress.Parse("127.0.0.1"));
                    await _dataAccessService.AddNode(key, (byte)NodeRole.TransactionsRegistrationLayer, IPAddress.Parse("127.0.0.1"));
                    await _dataAccessService.AddNode(key, (byte)NodeRole.StorageLayer, IPAddress.Parse("127.0.0.1"));
                    await _dataAccessService.AddNode(key, (byte)NodeRole.SynchronizationLayer, IPAddress.Parse("127.0.0.1"));
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to set secret key", ex);
                throw;
            }
		}

        private byte[] GetSecretKey(CancellationToken cancellationToken) => "519F58FA27A3F54CACB468B043782E90A3B6E78D503887DFF3734A6911E32304".HexStringToByteArray();

        // TODO: !!! fix ASAP (need to conffigure identity for Service Fabric)
        //private byte[] GetSecretKey(CancellationToken cancellationToken)
        //{
        //    AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
        //    KeyVaultClient keyVaultClient = null;

        //    if (!string.IsNullOrEmpty(_nodeWebAppConfiguration.AzureADCertThumbprint))
        //    {
        //        var certStore = new X509Store(StoreName.My, StoreLocation.LocalMachine);
        //        certStore.Open(OpenFlags.ReadOnly);
        //        var certs = certStore.Certificates
        //                    .Find(X509FindType.FindByThumbprint,
        //                        _nodeWebAppConfiguration.AzureADCertThumbprint, false);

        //        X509Certificate2 certificate = certs.OfType<X509Certificate2>().FirstOrDefault();

        //        if (certificate == null)
        //        {
        //            throw new CertificateNotFoundException(_nodeWebAppConfiguration.AzureADCertThumbprint, $"{certStore.Location}/{certStore.Name}");
        //        }

        //        ClientAssertionCertificate clientAssertionCertificate = new ClientAssertionCertificate(_nodeWebAppConfiguration.AzureADApplicationId, certificate);
        //        keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback((a, r, s) => GetAccessToken(a, r, s, clientAssertionCertificate)));
        //    }
        //    else
        //    {
        //        keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
        //    }

        //    ManualResetEventSlim resetEvent = new ManualResetEventSlim();

        //    _logger.Debug("Starting to obtain NodeSecretKey");
        //    SecretBundle secret = null;

        //    keyVaultClient
        //        .GetSecretAsync($"https://{_nodeWebAppConfiguration.KeyVaultName}.vault.azure.net/secrets/NodeSecretKey")
        //        .ContinueWith(t =>
        //        {
        //            if (t.IsCompletedSuccessfully)
        //            {
        //                _logger.Info("NodeSecretKey obtained successfully");
        //                secret = t.Result;
        //            }
        //            else
        //            {
        //                _logger.Error("Failed to obtain NodeSecretKey", t.Exception);
        //            }

        //            resetEvent.Set();
        //        }, TaskScheduler.Current);

        //    resetEvent.Wait(cancellationToken);
        //    _logger.Debug("NodeSecretKey obtaining completed");

        //    byte[] secretKey = secret.Value.HexStringToByteArray();
        //    return secretKey;
        //}

        private static async Task<string> GetAccessToken(string authority, string resource, string scope, ClientAssertionCertificate cert)
        {
            var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
            var result = await context.AcquireTokenAsync(resource, cert).ConfigureAwait(false);
            return result.AccessToken;
        }
    }
}
