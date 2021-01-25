using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;
using O10.Client.Common.Entities;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.AttributesScheme;
using O10.Client.DataLayer.Enums;
using O10.Client.DataLayer.Services;
using O10.Client.Web.Common;
using O10.Client.Web.Common.Configuration;
using O10.Core;
using O10.Core.ExtensionMethods;
using O10.Core.Architecture;

using O10.Core.Configuration;
using O10.Core.Logging;
using System;

namespace O10.Client.Web.Portal.Services.ExternalIdps
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Singleton)]
    public class ExternalIdpsInitializer : InitializerBase
    {
        private readonly IAzureConfiguration _azureConfiguration;
        private readonly ILogger _logger;

        public ExternalIdpsInitializer(IDataAccessService dataAccessService, IAccountsService accountsService, IExecutionContextManager executionContextManager,
            IConfigurationService configurationService, ILoggerService loggerService)
        {
            if (configurationService is null)
            {
                throw new System.ArgumentNullException(nameof(configurationService));
            }

            if (loggerService is null)
            {
                throw new System.ArgumentNullException(nameof(loggerService));
            }

            _dataAccessService = dataAccessService ?? throw new System.ArgumentNullException(nameof(dataAccessService));
            _accountsService = accountsService ?? throw new System.ArgumentNullException(nameof(accountsService));
            _executionContextManager = executionContextManager;
            _azureConfiguration = configurationService.Get<IAzureConfiguration>();
            _logger = loggerService.GetLogger(nameof(ExternalIdpsInitializer));
        }

        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Normal;

        private readonly ExternalIdpDTO[] _externalIdps = new ExternalIdpDTO[]
        {
            GetBlinkIdDrivingLicense(),
            GetBlinkIdPassport()
        };

        private static ExternalIdpDTO GetBlinkIdDrivingLicense() => new ExternalIdpDTO
        {
            Name = "BlinkID-DrivingLicense",
            Alias = "Blink ID Driving License",
            Description = "Provides an Identity based on scan of your Driving License",
            AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        AttributeName = "DocumentPhoto",
                        Alias = "Document Photo",
                        SchemeName = AttributesSchemes.ATTR_SCHEME_NAME_PASSPORTPHOTO,
                        Description = AttributesSchemes.ATTR_SCHEME_NAME_PASSPORTPHOTO_DESC
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "FirstName",
                        Alias = "First Name",
                        SchemeName = AttributesSchemes.ATTR_SCHEME_NAME_FIRSTNAME,
                        Description = AttributesSchemes.ATTR_SCHEME_NAME_FIRSTNAME_DESC
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "LastName",
                        Alias = "Last Name",
                        SchemeName = AttributesSchemes.ATTR_SCHEME_NAME_LASTNAME,
                        Description = AttributesSchemes.ATTR_SCHEME_NAME_LASTNAME_DESC
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "IDCard",
                        Alias = "Id Card",
                        SchemeName = AttributesSchemes.ATTR_SCHEME_NAME_IDCARD,
                        Description = AttributesSchemes.ATTR_SCHEME_NAME_IDCARD_DESC
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "DrivingLicense",
                        Alias = "Driving License",
                        SchemeName = AttributesSchemes.ATTR_SCHEME_NAME_DRIVINGLICENSE,
                        Description = AttributesSchemes.ATTR_SCHEME_NAME_DRIVINGLICENSE_DESC,
                        IsRoot = true
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "DateOfBirth",
                        Alias = "Date of Birth",
                        SchemeName = AttributesSchemes.ATTR_SCHEME_NAME_DATEOFBIRTH,
                        Description = AttributesSchemes.ATTR_SCHEME_NAME_DATEOFBIRTH
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "IssuanceDate",
                        Alias = "Issuance Date",
                        SchemeName = AttributesSchemes.ATTR_SCHEME_NAME_ISSUANCEDATE,
                        Description = AttributesSchemes.ATTR_SCHEME_NAME_ISSUANCEDATE_DESC
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "ExpirationDate",
                        Alias = "Expiration Date",
                        SchemeName = AttributesSchemes.ATTR_SCHEME_NAME_EXPIRATIONDATE,
                        Description = AttributesSchemes.ATTR_SCHEME_NAME_EXPIRATIONDATE_DESC
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "Password",
                        Alias = "Password",
                        SchemeName = AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD,
                        Description = AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD_DESC
                    }
                }
        };

        private static ExternalIdpDTO GetBlinkIdPassport() => new ExternalIdpDTO
        {
            Name = "BlinkID-Passport",
            Alias = "Blink ID Passport",
            Description = "Provides an Identity based on scan of your Passport",
            AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        AttributeName = "PassportPhoto",
                        Alias = "Passport Photo",
                        SchemeName = AttributesSchemes.ATTR_SCHEME_NAME_PASSPORTPHOTO,
                        Description = AttributesSchemes.ATTR_SCHEME_NAME_PASSPORTPHOTO_DESC
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "FirstName",
                        Alias = "First Name",
                        SchemeName = AttributesSchemes.ATTR_SCHEME_NAME_FIRSTNAME,
                        Description = AttributesSchemes.ATTR_SCHEME_NAME_FIRSTNAME_DESC
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "LastName",
                        Alias = "Last Name",
                        SchemeName = AttributesSchemes.ATTR_SCHEME_NAME_LASTNAME,
                        Description = AttributesSchemes.ATTR_SCHEME_NAME_LASTNAME_DESC
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "IDCard",
                        Alias = "Id Card",
                        SchemeName = AttributesSchemes.ATTR_SCHEME_NAME_IDCARD,
                        Description = AttributesSchemes.ATTR_SCHEME_NAME_IDCARD_DESC
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "Passport",
                        Alias = "Passport",
                        SchemeName = AttributesSchemes.ATTR_SCHEME_NAME_PASSPORT,
                        Description = AttributesSchemes.ATTR_SCHEME_NAME_PASSPORT_DESC,
                        IsRoot = true
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "DateOfBirth",
                        Alias = "Date of Birth",
                        SchemeName = AttributesSchemes.ATTR_SCHEME_NAME_DATEOFBIRTH,
                        Description = AttributesSchemes.ATTR_SCHEME_NAME_DATEOFBIRTH
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "ExpirationDate",
                        Alias = "Expiration Date",
                        SchemeName = AttributesSchemes.ATTR_SCHEME_NAME_EXPIRATIONDATE,
                        Description = AttributesSchemes.ATTR_SCHEME_NAME_EXPIRATIONDATE_DESC
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "Issuer",
                        Alias = "Issuer",
                        SchemeName = AttributesSchemes.ATTR_SCHEME_NAME_ISSUER,
                        Description = AttributesSchemes.ATTR_SCHEME_NAME_ISSUER_DESC
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "Nationality",
                        Alias = "Nationality",
                        SchemeName = AttributesSchemes.ATTR_SCHEME_NAME_NATIONALITY,
                        Description = AttributesSchemes.ATTR_SCHEME_NAME_NATIONALITY_DESC
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "Password",
                        Alias = "Password",
                        SchemeName = AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD,
                        Description = AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD_DESC
                    }
                }
        };

        private readonly IDataAccessService _dataAccessService;
        private readonly IAccountsService _accountsService;
        private readonly IExecutionContextManager _executionContextManager;

        protected override void InitializeInner(CancellationToken cancellationToken)
        {
            _logger.Info($"Started {nameof(InitializeInner)}");

            _logger.Info($"There are {_externalIdps?.Length ?? 0} external IdPs");

            foreach (var externalIdp in _externalIdps)
            {
                _logger.Info($"Initializing {JsonConvert.SerializeObject(externalIdp)}");

                try
                {
                    var provider = _dataAccessService.GetExternalIdentityProvider(externalIdp.Name);
                    if (provider == null)
                    {
                        long accountId = CreateIdentityProviderAccount(externalIdp);

                        _dataAccessService.AddExternalIdentityProvider(externalIdp.Name, externalIdp.Alias, externalIdp.Description, accountId);
                        provider = _dataAccessService.GetExternalIdentityProvider(externalIdp.Name);
                    }

                    var accountDescriptor = _accountsService.Authenticate(provider.AccountId, GetDefaultIdpPassword(provider.Name));
                    if (accountDescriptor != null)
                    {
                        _logger.Info($"Account {externalIdp.Name} authenticated successfully");

                        if (externalIdp.AttributeDefinitions != null)
                        {
                            foreach (var item in externalIdp.AttributeDefinitions)
                            {
                                long rootAttributeSchemeId = _dataAccessService.AddAttributeToScheme(accountDescriptor.PublicSpendKey.ToHexString(), item.AttributeName, item.SchemeName, item.Alias, item.Description);
                                if (item.IsRoot)
                                {
                                    _dataAccessService.ToggleOnRootAttributeScheme(rootAttributeSchemeId);
                                }
                            }
                        }

                        _executionContextManager.InitializeStateExecutionServices(accountDescriptor.AccountId, accountDescriptor.SecretSpendKey);
                    }
                    else
                    {
                        _logger.Error($"Authentication of the account {externalIdp.Name} failed");
                    }

                    _logger.Info($"Finished {nameof(InitializeInner)}");

                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to initialize the External IdP {externalIdp.Name}", ex);
                }
            }
        }
        private string GetDefaultIdpPassword(string name)
        {
            string secretName = $"ExtIdp-{name}-pwd";

            return AzureHelper.GetSecretValue(secretName, _azureConfiguration.AzureADCertThumbprint, _azureConfiguration.AzureADApplicationId, _azureConfiguration.KeyVaultName);
        }

        private long CreateIdentityProviderAccount(ExternalIdpDTO externalIdp)
        {
            string pwd = GetDefaultIdpPassword(externalIdp.Name);

            long accountId = _accountsService.Create(AccountType.IdentityProvider, externalIdp.Alias, pwd, true);

            return accountId;
        }
    }
}
