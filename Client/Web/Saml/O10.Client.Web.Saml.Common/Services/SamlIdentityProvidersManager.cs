using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using O10.Client.DataLayer.Model;
using O10.Client.DataLayer.Services;
using O10.Core.Architecture;
using O10.Core.Models;
using O10.Core.ExtensionMethods;
using O10.Client.DataLayer.Enums;
using O10.Core.Configuration;
using System.Linq;
using O10.Client.Common.Entities;
using O10.Client.Common.Communication;
using O10.Client.Common.Interfaces;
using O10.Client.Common.Crypto;
using Microsoft.AspNetCore.SignalR;
using O10.Client.Web.Saml.Common.Hubs;
using O10.Core.Logging;
using O10.Client.Web.Common;
using O10.Client.Web.Common.Configuration;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace O10.Client.Web.Saml.Common.Services
{
    /*[RegisterDefaultImplementation(typeof(ISamlIdentityProvidersManager), Lifetime = LifetimeManagement.Singleton)]
	public class SamlIdentityProvidersManager : ISamlIdentityProvidersManager
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly IDataAccessService _dataAccessService;
		private readonly IAccountsService _accountsService;
        private readonly IGatewayService _gatewayService;
        private readonly IHubContext<SamlIdpHub> _hubContext;
		private readonly ILoggerService _loggerService;
		private readonly Dictionary<string, SamlIdpServicePersistence> _samlIdpServices;
		private readonly BroadcastBlock<WitnessPackageWrapper> _broadcastBlock;
		private readonly IAzureConfiguration _azureConfiguration;
		private SamlIdpService _samlIdpServiceDefault;
		private long _defaultSamlIdpId;

		public SamlIdentityProvidersManager(IServiceProvider serviceProvider,
                                      IDataAccessService dataAccessService,
                                      IAccountsService accountsService,
                                      IConfigurationService configurationService,
                                      IGatewayService gatewayService,
                                      IHubContext<SamlIdpHub> hubContext,
                                      ILoggerService loggerService)
		{
			_serviceProvider = serviceProvider;
			_dataAccessService = dataAccessService;
			_accountsService = accountsService;
            _gatewayService = gatewayService;
            _hubContext = hubContext;
			_loggerService = loggerService;
			_samlIdpServices = new Dictionary<string, SamlIdpServicePersistence>();
			_broadcastBlock = new BroadcastBlock<WitnessPackageWrapper>(p => p);
			_azureConfiguration = configurationService.Get<IAzureConfiguration>();
		}

		public void CreateNewDefaultSamlIdentityProvider()
		{
			long accountId = _accountsService.Create(AccountType.User, "SamlIdpDefault", GetDefaultSamlIdpPassword(), true);
			AccountDescriptor accountDescriptor = _accountsService.Authenticate(accountId, GetDefaultSamlIdpPassword());

			long samlIdPId = _dataAccessService.SetSamlIdentityProvider("default", accountDescriptor.PublicSpendKey.ToHexString(), accountDescriptor.SecretViewKey.ToHexString());
			SamlIdentityProvider samlIdentityProvider = _dataAccessService.GetSamlIdentityProviders().FirstOrDefault(s => s.SamlIdentityProviderId == samlIdPId);
			_dataAccessService.SetSamlSettings(samlIdPId, accountId);
			_defaultSamlIdpId = samlIdPId;

			if (samlIdentityProvider != null)
			{
				_samlIdpServiceDefault?.PipeIn.Complete();
				_samlIdpServiceDefault?.Unsubscribe();
				InitializeSamlIdpService(samlIdentityProvider);
			}
		}

		public SamlIdpService GetSamlIdpService(string entityId)
		{
			if(_samlIdpServices.ContainsKey(entityId))
			{
				return _samlIdpServices[entityId].SamlIdpService;
			}
			else
			{
				return _samlIdpServiceDefault;
			}
		}

		public void Initialize(CancellationToken cancellationToken)
		{
			IEnumerable<SamlIdentityProvider> samlIdentityProviders = _dataAccessService.GetSamlIdentityProviders();
			_defaultSamlIdpId = _dataAccessService.GetSamlSettings()?.DefaultSamlIdpId ?? 0;

			foreach (var samlIdentityProvider in samlIdentityProviders)
			{
				InitializeSamlIdpService(samlIdentityProvider);
			}
		}

		public void Start()
		{

		}

		private void InitializeSamlIdpService(SamlIdentityProvider samlIdentityProvider)
		{
			IWitnessPackagesProvider packetsProvider = _serviceProvider.GetService<IWitnessPackagesProvider>();
			IStealthClientCryptoService clientCryptoService = ActivatorUtilities.CreateInstance<StealthClientCryptoService>(_serviceProvider);
            StealthPacketsExtractor utxoPacketsExtractor = ActivatorUtilities.CreateInstance<StealthPacketsExtractor>(_serviceProvider);
			SamlIdpService samlIdpService = new SamlIdpService(utxoPacketsExtractor, _hubContext, _loggerService);
			SamlIdpWitnessPackageUpdater witnessPackageUpdater = null;
			samlIdpService.Initialize(samlIdentityProvider.EntityId, samlIdentityProvider.SecretViewKey.HexStringToByteArray(), samlIdentityProvider.PublicSpendKey.HexStringToByteArray());

			if (samlIdentityProvider.SamlIdentityProviderId == _defaultSamlIdpId)
			{
				CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
				long accountId = _dataAccessService.GetSamlSettings()?.DefaultSamlIdpAccountId ?? 0;
				utxoPacketsExtractor.Initialize(accountId);

				packetsProvider.Initialize(accountId, cancellationTokenSource.Token);

				AccountDescriptor accountDescriptor = _accountsService.Authenticate(accountId, GetDefaultSamlIdpPassword());
				clientCryptoService.Initialize(accountDescriptor.SecretSpendKey, accountDescriptor.SecretViewKey, accountDescriptor.PwdHash);
				witnessPackageUpdater = new SamlIdpWitnessPackageUpdater(accountId, _dataAccessService);

				packetsProvider.PipeOut.LinkTo(samlIdpService.PipeIn);
				utxoPacketsExtractor.GetSourcePipe<WitnessPackage>().LinkTo(witnessPackageUpdater.WitnessPackagePipeIn);

				packetsProvider.Start();


				_samlIdpServiceDefault = samlIdpService;
			}

			SamlIdpServicePersistence samlIdpServicePersistence = new SamlIdpServicePersistence
			{
				WitnessPackagesProvider = packetsProvider,
				ClientCryptoService = clientCryptoService,
				PacketsExtractor = utxoPacketsExtractor,
				SamlIdpService = samlIdpService,
				WitnessPackageUpdater = witnessPackageUpdater
			};

			// TODO: need to add initializing clientCryptoService for external SAML IdP Service
			_samlIdpServices.Add(samlIdentityProvider.EntityId, samlIdpServicePersistence);

			samlIdpService.SetUnsubscriber(_broadcastBlock.LinkTo(samlIdpService.PipeIn));
		}

        private string GetDefaultSamlIdpPassword()
        {
            string secretName = "SamlIdpPassword";

            return AzureHelper.GetSecretValue(secretName, _azureConfiguration.AzureADCertThumbprint, _azureConfiguration.AzureADApplicationId, _azureConfiguration.KeyVaultName);
        }

	}*/
}
