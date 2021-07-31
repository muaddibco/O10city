using System.Threading;
using O10.Client.Web.Common.Configuration;
using O10.Core;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.IdentityProvider.DataLayer.Services;
using ICoreDataAccessService = O10.Client.DataLayer.Services.IDataAccessService;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.AttributesScheme;
using O10.Core.ExtensionMethods;
using O10.Core.Logging;
using System;
using System.Threading.Tasks;
using O10.Client.Common.Entities;

namespace O10.Server.IdentityProvider.Common.Services
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Singleton)]
	public class IdentityProviderAccountInitializer : InitializerBase
	{
		private readonly IAzureConfiguration _azureConfiguration;
		private readonly IDataAccessService _dataAccessService;
		private readonly ICoreDataAccessService _coreDataAccessService;
		private readonly IAccountsService _accountsService;
		private readonly IExecutionContext _executionContext;
		private readonly ILogger _logger;

		public IdentityProviderAccountInitializer(IConfigurationService configurationService, IDataAccessService dataAccessService, 
			ICoreDataAccessService coreDataAccessService, IAccountsService accountsService, IExecutionContext executionContext,
			ILoggerService loggerService)
		{
			_azureConfiguration = configurationService.Get<IAzureConfiguration>();
			_dataAccessService = dataAccessService;
			_coreDataAccessService = coreDataAccessService;
			_accountsService = accountsService;
			_executionContext = executionContext;
			_logger = loggerService.GetLogger(nameof(IdentityProviderAccountInitializer));
		}

		public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Lowest;

		protected override async Task InitializeInner(CancellationToken cancellationToken)
		{
			_logger.Info($"Started {nameof(InitializeInner)}");

			try
			{
				long accountId = _dataAccessService.GetIdentityProviderAccountId();
				if (accountId == 0)
				{
					_logger.Info("No O10 IdP account created yet, creating new one");
					accountId = CreateIdentityProviderAccount();
				}

				_logger.Info($"O10 IdP account with accountId {accountId}");

				var accountDescriptor = _accountsService.Authenticate(accountId, GetDefaultIdpPassword());
				if (accountDescriptor != null)
				{
					_logger.Info("O10 IdP account authenticated successfully");
					long rootAttributeSchemeId = _coreDataAccessService.AddAttributeToScheme(accountDescriptor.PublicSpendKey.ToHexString(), "Email", AttributesSchemes.ATTR_SCHEME_NAME_EMAIL, "E-Mail", AttributesSchemes.ATTR_SCHEME_NAME_EMAIL_DESC);
					_coreDataAccessService.ToggleOnRootAttributeScheme(rootAttributeSchemeId);
					_executionContext.Initialize(accountDescriptor.AccountId, accountDescriptor.SecretSpendKey);
				}
				else
				{
					_logger.Error("O10 IdP account authentication failed");
				}

				_logger.Info($"Finished {nameof(InitializeInner)}");
			}
			catch (Exception ex)
			{
				_logger.Error($"Failure at {nameof(InitializeInner)}", ex);
			}		
		}

		private string GetDefaultIdpPassword()
		{
			string secretName = "O10IdpPassword";

			return "qqq"; // AzureHelper.GetSecretValue(secretName, _azureConfiguration.AzureADCertThumbprint, _azureConfiguration.AzureADApplicationId, _azureConfiguration.KeyVaultName);
		}

		private long CreateIdentityProviderAccount()
		{
			string pwd = GetDefaultIdpPassword();

			long accountId = _accountsService.Create(AccountTypeDTO.IdentityProvider, "O10 Identity Provider", pwd, true);
			_dataAccessService.SetIdentityProviderAccountId(accountId);

			return accountId;
		}
	}
}
