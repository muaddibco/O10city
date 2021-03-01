using Chaos.NaCl;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using O10.Client.DataLayer.AttributesScheme;
using O10.Client.DataLayer.Configuration;
using O10.Client.DataLayer.Entities;
using O10.Client.DataLayer.Enums;
using O10.Client.DataLayer.Exceptions;
using O10.Client.DataLayer.Model;
using O10.Client.DataLayer.Model.ConsentManagement;
using O10.Client.DataLayer.Model.Inherence;
using O10.Client.DataLayer.Model.Scenarios;
using O10.Client.DataLayer.Model.ServiceProviders;
using O10.Client.DataLayer.Model.Users;
using O10.Core;
using O10.Core.Architecture;

using O10.Core.Configuration;
using O10.Core.ExtensionMethods;
using O10.Core.Logging;
using O10.Crypto.ConfidentialAssets;
using O10.Client.DataLayer.ElectionCommittee;
using System.Linq.Expressions;

namespace O10.Client.DataLayer.Services
{
    [RegisterDefaultImplementation(typeof(IDataAccessService), Lifetime = LifetimeManagement.Singleton)]
    public class DataAccessService : IDataAccessService
    {
        private readonly object _sync = new object();
        private DataContext _dataContext;
        private readonly IEnumerable<IDataContext> _dataContexts;
        private readonly IClientDataContextConfiguration _configuration;
        private readonly ILogger _logger;

        public DataAccessService(IEnumerable<IDataContext> dataContexts, IConfigurationService configurationService, ILoggerService loggerService)
        {
            _dataContexts = dataContexts;
            _configuration = configurationService.Get<IClientDataContextConfiguration>();
            _logger = loggerService.GetLogger(nameof(DataAccessService));
        }

        public bool Initialize()
        {
            try
            {
                lock (_sync)
                {
                    DbInitialize();

                    SystemSettings systemSettings = _dataContext.SystemSettings.FirstOrDefault();

                    if (systemSettings == null)
                    {
                        byte[] initializationVector = new byte[16];
                        RandomNumberGenerator.Create().GetBytes(initializationVector);
                        _dataContext.SystemSettings.Add(new SystemSettings { InitializationVector = initializationVector, BiometricSecretKey = ConfidentialAssetsHelper.GetRandomSeed() });

                        _dataContext.SaveChanges();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to initialize {nameof(DataAccessService)}", ex);
                return false;
            }
        }

        private void DbInitialize()
        {
            _logger.Info($"Initializing connection type {_configuration.ConnectionType} with connection string {_configuration.ConnectionString}");
            _dataContext = _dataContexts.FirstOrDefault(d => d.DataProvider.Equals(_configuration.ConnectionType)) as DataContext;
            _dataContext.Initialize(_configuration.ConnectionString);

            bool retry = false;
            try
            {
                _dataContext.Database.Migrate();
            }
            catch (Exception ex)
            {
                // TODO: this solution must be refactored completely, not good to erase DB at ALL
                retry = true;
                _logger.Error("Failure during database migration. Retry.", ex);
                _dataContext.Database.EnsureDeleted();
            }

            if (retry)
            {
                try
                {
                    _dataContext.Database.Migrate();
                }
                catch (Exception ex)
                {
                    _logger.Error("Failure during database migration. Retry.", ex);
                    throw;
                }
            }

            _logger.Info($"DataContext {_dataContext.GetType().FullName}; ConnectionString = {_dataContext.Database.GetDbConnection().ConnectionString}");
        }

        #region Identity

        public Identity CreateIdentity(long accountId, string description, (string attrName, string content)[] attributes)
        {
            lock (_sync)
            {
                Identity identity = new Identity
                {
                    AccountId = accountId,
                    Description = description,
                };
                _dataContext.Identities.Add(identity);

                foreach (var (attrName, content) in attributes)
                {
                    IdentityAttribute identityAttribute = new IdentityAttribute
                    {
                        Identity = identity,
                        AttributeName = attrName,
                        Content = content,
                        Subject = ClaimSubject.User
                    };

                    _dataContext.Attributes.Add(identityAttribute);
                }

                _dataContext.SaveChanges();

                return identity;

            }
        }

        public void UpdateIdentityAttributeCommitment(long identityAttributeId, byte[] commitment)
        {
            lock (_sync)
            {
                IdentityAttribute identityAttribute = _dataContext.Attributes.FirstOrDefault(a => a.AttributeId == identityAttributeId);

                if (identityAttribute != null)
                {
                    identityAttribute.Commitment = commitment;
                    _dataContext.SaveChanges();
                }
            }
        }

        public IEnumerable<Identity> GetIdentities(long accountId)
        {
            lock (_sync)
            {
                return _dataContext.Identities.Where(a => a.AccountId == accountId).Include(i => i.Attributes).ToList();
            }
        }

        public long AddOrUpdateIdentityTarget(long identityId, string publicSpendKey, string publicViewKey)
        {
            lock(_sync)
            {
                var identityTarget = _dataContext.IdentityTargets.FirstOrDefault(i => i.IdentityId == identityId);
                if(identityTarget == null)
                {
                    identityTarget = new IdentityTarget
                    {
                        IdentityId = identityId
                    };
                    _dataContext.IdentityTargets.Add(identityTarget);
                }

                identityTarget.PublicSpendKey = publicSpendKey;
                identityTarget.PublicViewKey = publicViewKey;

                _dataContext.SaveChanges();

                return identityTarget.IdentityTargetId;
            }
        }

        public IdentityTarget GetIdentityTarget(long identityId)
        {
            lock(_sync)
            {
                var identityTarget = _dataContext.IdentityTargets.FirstOrDefault(i => i.IdentityId == identityId);
                if(identityTarget == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(identityId));
                }

                return identityTarget;
            }
        }

        #endregion Identity

        public void UpdateConfirmedRootAttribute(UserRootAttribute userRootAttribute)
        {
            lock (_sync)
            {
                UserRootAttribute attr = _dataContext.UserRootAttributes.FirstOrDefault(r => r.UserAttributeId == userRootAttribute.UserAttributeId);
                if (attr != null)
                {
                    attr.AssetId = userRootAttribute.AssetId;
                    attr.SchemeName = userRootAttribute.SchemeName;
                    attr.IssuanceCommitment = userRootAttribute.IssuanceCommitment;
                    attr.LastBlindingFactor = userRootAttribute.LastBlindingFactor;
                    attr.LastCommitment = userRootAttribute.LastCommitment;
                    attr.LastDestinationKey = userRootAttribute.LastDestinationKey;
                    attr.LastTransactionKey = userRootAttribute.LastTransactionKey;
                    attr.NextKeyImage = userRootAttribute.NextKeyImage;
                    attr.OriginalBlindingFactor = userRootAttribute.OriginalBlindingFactor;
                    attr.OriginalCommitment = userRootAttribute.OriginalCommitment;
                    attr.Source = userRootAttribute.Source;
                    attr.ConfirmationTime = DateTime.Now;
                    attr.LastUpdateTime = DateTime.Now;

                    _dataContext.SaveChanges();
                }
            }
        }

        public bool DeleteNonConfirmedUserRootAttribute(long accountId, string content)
        {
            lock (_sync)
            {
                IEnumerable<UserRootAttribute> userRootAttributes = _dataContext.UserRootAttributes.Where(r => r.AccountId == accountId && r.Content == content && r.Source == string.Empty).ToList();

                foreach (var userRootAttribute in userRootAttributes)
                {
                    _dataContext.UserRootAttributes.Remove(userRootAttribute);
                }

                if (userRootAttributes.Any())
                {
                    _dataContext.SaveChanges();
                    return true;
                }

                return false;
            }
        }

        public UserRootAttribute GetRootAttributeByOriginalCommitment(long accountId, byte[] originalCommitment)
        {
            lock (_sync)
            {
                return _dataContext.UserRootAttributes.Where(r => r.AccountId == accountId && !r.IsOverriden)
                                          .ToList()
                                          .FirstOrDefault(r => originalCommitment.Equals32(r.OriginalCommitment));
            }
        }

        public UserRootAttribute GetUserRootAttribute(long rootAttributeId)
        {
            lock (_sync)
            {
                return _dataContext.UserRootAttributes.FirstOrDefault(r => r.UserAttributeId == rootAttributeId);
            }
        }

        public List<UserRootAttribute> GetAllNonConfirmedRootAttributes(long accountId)
        {
            lock (_sync)
            {
                return _dataContext
                    .UserRootAttributes
                    .Where(r => r.AccountId == accountId)
                    .ToList()
                    .Where(r => new byte[Globals.DEFAULT_HASH_SIZE].Equals32(r.LastCommitment))
                    .ToList();
            }
        }

        public long AddNonConfirmedRootAttribute(long accountId, string content, string issuer, string schemeName, byte[] assetId)
        {
            lock (_sync)
            {
                UserRootAttribute userRootAttribute = new UserRootAttribute
                {
                    AccountId = accountId,
                    AssetId = assetId,
                    Content = content,
                    SchemeName = schemeName,
                    IssuanceCommitment = new byte[Globals.DEFAULT_HASH_SIZE],
                    LastBlindingFactor = new byte[Globals.DEFAULT_HASH_SIZE],
                    LastCommitment = new byte[Globals.DEFAULT_HASH_SIZE],
                    LastDestinationKey = new byte[Globals.DEFAULT_HASH_SIZE],
                    LastTransactionKey = new byte[Globals.DEFAULT_HASH_SIZE],
                    NextKeyImage = string.Empty,
                    OriginalBlindingFactor = new byte[Globals.DEFAULT_HASH_SIZE],
                    OriginalCommitment = new byte[Globals.DEFAULT_HASH_SIZE],
                    Source = issuer,
                    CreationTime = DateTime.Now,
                    LastUpdateTime = DateTime.Now
                };

                _dataContext.UserRootAttributes.Add(userRootAttribute);
                _dataContext.SaveChanges();

                return userRootAttribute.UserAttributeId;
            }
        }

        public long AddUserRootAttribute(long accountId, UserRootAttribute attribute)
        {
            lock (_sync)
            {
                _logger.Info($"Adding user attribute with keyImage = {attribute.NextKeyImage}");
                bool modified = false;

                if (!_dataContext.UserRootAttributes.AsEnumerable().Any(u => u.AccountId == accountId && u.OriginalCommitment.Equals32(attribute.OriginalCommitment)))
                {
                    modified = true;
                    attribute.AccountId = accountId;
                    attribute.CreationTime = DateTime.Now;
                    attribute.ConfirmationTime = DateTime.Now;
                    attribute.LastUpdateTime = DateTime.Now;
                    _dataContext.UserRootAttributes.Add(attribute);
                }

                if (modified)
                {
                    _dataContext.SaveChanges();
                    return attribute.UserAttributeId;
                }
            }

            return 0;
        }

        public IEnumerable<UserRootAttribute> GetUserAttributes(long accountId)
        {
            lock (_sync)
            {
                return _dataContext.UserRootAttributes.Where(u => u.AccountId == accountId)?.ToList();
            }
        }

        public bool RemoveUserAttribute(long accountId, long userAttributeId)
        {
            lock (_sync)
            {
                UserRootAttribute userRootAttribute = _dataContext.UserRootAttributes.FirstOrDefault(a => a.AccountId == accountId && a.UserAttributeId == userAttributeId);
                if (userRootAttribute != null)
                {
                    _dataContext.UserRootAttributes.Remove(userRootAttribute);
                    _dataContext.SaveChanges();
                    return true;
                }

                return false;
            }
        }

        public long AddServiceProviderRegistration(long accountId, byte[] commitment)
        {
            lock (_sync)
            {
                ServiceProviderRegistration serviceProviderRegistration = new ServiceProviderRegistration { AccountId = accountId, Commitment = commitment };
                _dataContext.ServiceProviderRegistrations.Add(serviceProviderRegistration);

                _dataContext.SaveChanges();

                return serviceProviderRegistration.ServiceProviderRegistrationId;
            }
        }

        public bool UpdateUserAttribute(long accountId, string oldKeyImage, string keyImage, byte[] lastBlindingFactor, byte[] lastCommitment, byte[] lastTransactionKey, byte[] lastDestinationKey)
        {
            lock (_sync)
            {
                _logger.Info($"Updating user attribute with keyImage = {keyImage}");
                UserRootAttribute attr = _dataContext.UserRootAttributes.FirstOrDefault(c => c.AccountId == accountId && c.NextKeyImage == oldKeyImage);

                if (attr != null)
                {
                    attr.LastBlindingFactor = lastBlindingFactor;
                    attr.LastCommitment = lastCommitment;
                    attr.LastTransactionKey = lastTransactionKey;
                    attr.NextKeyImage = keyImage;
                    attr.LastDestinationKey = lastDestinationKey;
                    attr.LastUpdateTime = DateTime.Now;

                    _dataContext.SaveChanges();

                    return true;
                }

                return false;
            }
        }

        public bool UpdateUserAttributeContent(long userAttributeId, string content)
        {
            lock (_sync)
            {
                UserRootAttribute rootAttribute = _dataContext.UserRootAttributes.FirstOrDefault(r => r.UserAttributeId == userAttributeId);
                if (rootAttribute != null)
                {
                    rootAttribute.Content = content;
                    _dataContext.SaveChanges();
                    return true;
                }

                return false;
            }
        }


        public IEnumerable<ServiceProviderRegistration> GetServiceProviderRegistrations(long accountId)
        {
            lock (_sync)
            {
                return _dataContext.ServiceProviderRegistrations.Where(r => r.AccountId == accountId).ToList();
            }
        }

        public ServiceProviderRegistration GetServiceProviderRegistration(long accountId, byte[] registrationKey)
        {
            lock (_sync)
            {
                return _dataContext.ServiceProviderRegistrations.AsEnumerable().FirstOrDefault(r => r.Commitment.Equals32(registrationKey));
            }
        }

        public ServiceProviderRegistration GetServiceProviderRegistration(long registrationId)
        {
            lock (_sync)
            {
                return _dataContext.ServiceProviderRegistrations.FirstOrDefault(r => r.ServiceProviderRegistrationId == registrationId);
            }
        }


        public bool GetServiceProviderRegistrationId(long accountId, byte[] commitment, out long serviceProviderRegistrationId)
        {
            lock (_sync)
            {
                ServiceProviderRegistration serviceProviderRegistration = _dataContext.ServiceProviderRegistrations.AsEnumerable().FirstOrDefault(s => s.AccountId == accountId && s.Commitment.Equals32(commitment));

                if (serviceProviderRegistration != null)
                {
                    serviceProviderRegistrationId = serviceProviderRegistration.ServiceProviderRegistrationId;
                    return true;
                }

                serviceProviderRegistrationId = 0;
                return false;
            }
        }

        public List<long> MarkUserRootAttributesOverriden(long accountId, byte[] issuanceCommitment)
        {
            List<long> updatedIds = new List<long>();

            lock (_sync)
            {
                IEnumerable<UserRootAttribute> rootAttributes = _dataContext.UserRootAttributes.AsEnumerable().Where(c => c.AccountId == accountId && !c.IsOverriden && c.IssuanceCommitment.Equals32(issuanceCommitment));

                foreach (var attr in rootAttributes)
                {
                    attr.IsOverriden = true;
                    attr.LastUpdateTime = DateTime.Now;
                    updatedIds.Add(attr.UserAttributeId);
                }

                if (rootAttributes.Any())
                {
                    _dataContext.SaveChanges();
                }
            }

            return updatedIds;
        }

        public long MarkUserRootAttributesOverriden2(long accountId, byte[] originalCommitment)
        {
            lock (_sync)
            {
                UserRootAttribute rootAttribute = _dataContext.UserRootAttributes.AsEnumerable().FirstOrDefault(c => c.AccountId == accountId && !c.IsOverriden && c.OriginalCommitment.Equals32(originalCommitment));

                if (rootAttribute != null)
                {
                    rootAttribute.IsOverriden = true;
                    rootAttribute.LastUpdateTime = DateTime.Now;
                    _dataContext.SaveChanges();

                    return rootAttribute.UserAttributeId;
                }
            }

            return 0;
        }

        public void UpdateUserAttributeContent(long accountId, byte[] originalCommitment, string content)
        {
            lock (_sync)
            {
                UserRootAttribute attr = _dataContext.UserRootAttributes.AsEnumerable().FirstOrDefault(c => c.AccountId == accountId && c.OriginalCommitment.Equals32(originalCommitment));

                if (attr != null)
                {
                    attr.Content = content;
                    attr.LastUpdateTime = DateTime.Now;

                    _dataContext.SaveChanges();
                }
            }
        }


        public UserSettings GetUserSettings(long accountId)
        {
            lock (_sync)
            {
                UserSettings userSettings = GetLocalAwareUserSettings(accountId);

                if (userSettings == null)
                {
                    Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);

                    if (account != null)
                    {
                        userSettings = new UserSettings
                        {
                            Account = account,
                            IsAutoTheftProtection = true
                        };

                        _dataContext.UserSettings.Add(userSettings);
                        _dataContext.SaveChanges();
                    }
                }

                return userSettings;
            }
        }

        private UserSettings GetLocalAwareUserSettings(long accountId)
        {
            UserSettings userSettings = _dataContext.UserSettings.Local.FirstOrDefault(s => s.Account.AccountId == accountId);
            if (userSettings == null)
            {
                userSettings = _dataContext.UserSettings.Include(s => s.Account).FirstOrDefault(s => s.Account.AccountId == accountId);
            }

            return userSettings;
        }

        public void SetUserSettings(long accountId, UserSettings userSettings)
        {
            lock (_sync)
            {
                UserSettings userSettingsOrig = _dataContext.UserSettings.Include(s => s.Account).FirstOrDefault(s => s.Account.AccountId == accountId);

                if (userSettingsOrig == null)
                {
                    Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);
                    if (account != null)
                    {
                        userSettingsOrig = new UserSettings
                        {
                            Account = account,
                            IsAutoTheftProtection = true
                        };
                        _dataContext.UserSettings.Add(userSettingsOrig);
                    }
                }
                else
                {
                    userSettingsOrig.IsAutoTheftProtection = userSettings.IsAutoTheftProtection;
                }

                _dataContext.SaveChanges();
            }
        }

        public Identity GetIdentity(long id)
        {
            lock (_sync)
            {
                return _dataContext.Identities.Where(i => i.IdentityId == id).Include(i => i.Attributes).FirstOrDefault();
            }
        }

        public Identity GetIdentityByAttribute(long accountId, string attributeName, string attributeValue)
        {
            lock (_sync)
            {
                Identity identity = _dataContext.Identities.Include(i => i.Attributes)
                    .Where(i => i.AccountId == accountId && i.Attributes.Any(a => a.AttributeName == attributeName && a.Content.Equals(attributeValue)))
                    .FirstOrDefault();

                return identity;
            }
        }

        public void StoreSpAttribute(long accountId, string attributeSchemeName, byte[] assetId, string source, byte[] originalBlindingFactor, byte[] originalCommitment, byte[] issuingCommitment)
        {
            lock (_sync)
            {
                SpAttribute spClaim = new SpAttribute
                {
                    AccountId = accountId,
                    AssetId = assetId,
                    AttributeSchemeName = attributeSchemeName,
                    OriginalBlindingFactor = originalBlindingFactor,
                    OriginalCommitment = originalCommitment,
                    IssuingCommitment = issuingCommitment
                };

                _dataContext.SpAttributes.Add(spClaim);

                _dataContext.SaveChanges();
            }
        }

        internal void WipeAll()
        {
            lock (_sync)
            {
                _dataContext.Database.EnsureDeleted();
                _dataContext.Database.EnsureCreated();
            }
        }

        public IEnumerable<SpIdenitityValidation> GetSpIdenitityValidations(long accountId)
        {
            lock (_sync)
            {
                return _dataContext.SpIdenitityValidations.Where(s => s.AccountId == accountId).ToList();
            }
        }

        public void AdjustSpIdenitityValidations(long accountId, IEnumerable<SpIdenitityValidation> spIdenitityValidations)
        {
            lock (_sync)
            {
                List<SpIdenitityValidation> existingValidations = _dataContext.SpIdenitityValidations.Where(s => s.AccountId == accountId).ToList();

                List<SpIdenitityValidation> toRemove = existingValidations.Where(
                    e => !spIdenitityValidations.Any(
                        i =>
                            i.SchemeName == e.SchemeName &&
                            i.ValidationType == e.ValidationType &&
                            i.NumericCriterion.HasValue == e.NumericCriterion.HasValue &&
                            (i.NumericCriterion.HasValue ? i.NumericCriterion.Value == e.NumericCriterion.Value : true) &&
                            i.GroupIdCriterion == null ? e.GroupIdCriterion == null : i.GroupIdCriterion.Equals32(e.GroupIdCriterion)
                )).ToList();

                foreach (SpIdenitityValidation item in toRemove)
                {
                    _dataContext.SpIdenitityValidations.Remove(item);
                }

                List<SpIdenitityValidation> toAdd = spIdenitityValidations.Where(
                    e => !existingValidations.Any(
                        i =>
                            i.SchemeName == e.SchemeName &&
                            i.ValidationType == e.ValidationType &&
                            i.NumericCriterion.HasValue == e.NumericCriterion.HasValue &&
                            (i.NumericCriterion.HasValue ? i.NumericCriterion.Value == e.NumericCriterion.Value : true) &&
                            (i.GroupIdCriterion == null ? e.GroupIdCriterion == null : i.GroupIdCriterion.Equals32(e.GroupIdCriterion))
                )).ToList();

                foreach (SpIdenitityValidation item in toAdd)
                {
                    _dataContext.SpIdenitityValidations.Add(item);
                }

                _dataContext.SaveChanges();
            }
        }

        public IEnumerable<UserAssociatedAttribute> GetUserAssociatedAttributes(long accountId)
        {
            lock (_sync)
            {
                return _dataContext.UserAssociatedAttributes.Where(u => u.AccountId == accountId).ToList();
            }
        }

        public void UpdateUserAssociatedAttributes(long accountId, string issuer, IEnumerable<Tuple<string, string>> associatedAttributes, byte[] rootAssetId = null)
        {
            lock (_sync)
            {
                List<UserAssociatedAttribute> userAssociatedAttributes = _dataContext.UserAssociatedAttributes.AsEnumerable().Where(a => a.AccountId == accountId && a.Source == issuer && associatedAttributes.Any(a1 => a1.Item1 == a.AttributeSchemeName)).ToList();

                foreach (var item in userAssociatedAttributes)
                {
                    item.Content = associatedAttributes.First(a => a.Item1 == item.AttributeSchemeName).Item2;
                    item.RootAssetId = rootAssetId;
                }

                foreach (var item in associatedAttributes.Where(a => userAssociatedAttributes.All(a1 => a1.AttributeSchemeName != a.Item1)))
                {
                    UserAssociatedAttribute userAssociatedAttribute = new UserAssociatedAttribute
                    {
                        AttributeSchemeName = item.Item1,
                        Content = item.Item2,
                        AccountId = accountId,
                        Source = issuer,
                        RootAssetId = rootAssetId
                    };

                    _dataContext.UserAssociatedAttributes.Add(userAssociatedAttribute);
                }

                _dataContext.SaveChanges();
            }
        }

        public void DuplicateAssociatedAttributes(long oldAccountId, long newAccountId)
        {
            lock (_sync)
            {
                foreach (var userAssociatedAttributeOld in _dataContext.UserAssociatedAttributes.Where(u => u.AccountId == oldAccountId))
                {
                    UserAssociatedAttribute userAssociatedAttribute = new UserAssociatedAttribute
                    {
                        AccountId = newAccountId,
                        AttributeSchemeName = userAssociatedAttributeOld.AttributeSchemeName,
                        Content = userAssociatedAttributeOld.Content
                    };

                    _dataContext.UserAssociatedAttributes.Add(userAssociatedAttribute);
                }

                _dataContext.SaveChanges();
            }
        }

        public void AddOrUpdateUserIdentityIsser(string key, string alias, string descrioption)
        {
            lock (_sync)
            {
                UserIdentityIssuer userIdentityIssuer = _dataContext.UserIdentityIssuers.FirstOrDefault(i => i.Key == key);

                if (userIdentityIssuer == null)
                {
                    userIdentityIssuer = new UserIdentityIssuer
                    {
                        Key = key
                    };

                    _dataContext.UserIdentityIssuers.Add(userIdentityIssuer);
                }

                userIdentityIssuer.Alias = alias;
                userIdentityIssuer.Description = descrioption;
                userIdentityIssuer.UpdateTime = DateTime.Now;

                _dataContext.SaveChanges();
            }
        }

        public string GetUserIdentityIsserAlias(string key)
        {
            lock (_sync)
            {
                UserIdentityIssuer userIdentityIssuer = _dataContext.UserIdentityIssuers.FirstOrDefault(i => i.Key == key);

                return userIdentityIssuer?.Alias;
            }
        }

        public void StoreAssociatedAttributes(string rootIssuer, string rootAssetId, IEnumerable<AssociatedAttributeBackup> attributes)
        {
            if (attributes is null)
            {
                throw new ArgumentNullException(nameof(attributes));
            }

            lock (_sync)
            {
                foreach (var attribute in attributes)
                {
                    AssociatedAttributeBackup associatedAttributeBackup =
                        _dataContext.AssociatedAttributeBackups
                        .FirstOrDefault(a =>
                            a.RootIssuer == rootIssuer && a.RootAssetId == rootAssetId &&
                            a.AssociatedIssuer == attribute.AssociatedIssuer && a.SchemeName == attribute.SchemeName);

                    if (associatedAttributeBackup == null)
                    {
                        associatedAttributeBackup = new AssociatedAttributeBackup
                        {
                            RootIssuer = rootIssuer,
                            RootAssetId = rootAssetId,
                            AssociatedIssuer = attribute.AssociatedIssuer,
                            SchemeName = attribute.SchemeName
                        };

                        _dataContext.AssociatedAttributeBackups.Add(associatedAttributeBackup);
                    }

                    associatedAttributeBackup.Content = attribute.Content;
                }

                _dataContext.SaveChanges();
            }
        }

        public IEnumerable<AssociatedAttributeBackup> GetAssociatedAttributeBackups(string rootIssuer, string rootAssetId)
        {
            lock (_sync)
            {
                return _dataContext.AssociatedAttributeBackups.Where(a => a.RootIssuer == rootIssuer && a.RootAssetId == rootAssetId)?.ToList();
            }
        }

        public void AddUserTransactionSecret(long accountId, string keyImage, string issuer, string assetId, string blindingFactor)
        {
            lock (_sync)
            {
                UserTransactionSecrets transactionSecrets = new UserTransactionSecrets
                {
                    AccountId = accountId,
                    KeyImage = keyImage,
                    Issuer = issuer,
                    AssetId = assetId,
                    BlindingFactor = blindingFactor
                };

                _dataContext.UserTransactionSecrets.Add(transactionSecrets);

                _dataContext.SaveChanges();
            }
        }

        public UserTransactionSecrets GetUserTransactionSecrets(long accountId, string keyImage)
        {
            lock (_sync)
            {
                return _dataContext.UserTransactionSecrets.FirstOrDefault(s => s.AccountId == accountId && s.KeyImage == keyImage);
            }
        }

        public void RemoveUserTransactionSecret(long accountId, string keyImage)
        {
            lock (_sync)
            {
                UserTransactionSecrets transactionSecrets = _dataContext.UserTransactionSecrets.FirstOrDefault(s => s.AccountId == accountId && s.KeyImage == keyImage);

                if (transactionSecrets != null)
                {
                    _dataContext.UserTransactionSecrets.Remove(transactionSecrets);

                    _dataContext.SaveChanges();
                }
            }
        }

        public bool GetLastUpdatedCombinedBlockHeight(long accountId, out ulong lastUpdatedCombinedBlockHeight)
        {
            lock (_sync)
            {
                SynchronizationStatus synchronizationStatus = _dataContext.SynchronizationStatuses.FirstOrDefault(s => s.Account.AccountId == accountId);
                if (synchronizationStatus == null)
                {
                    lastUpdatedCombinedBlockHeight = 0;
                    return false;
                }

                lastUpdatedCombinedBlockHeight = synchronizationStatus.LastUpdatedCombinedBlockHeight;
                return true;
            }
        }

        public void StoreLastUpdatedCombinedBlockHeight(long accountId, ulong lastUpdatedCombinedBlockHeight)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);

                if (account == null)
                {
                    throw new AccountDoesNotExistException(accountId);
                }

                SynchronizationStatus synchronizationStatus = _dataContext.SynchronizationStatuses.FirstOrDefault(s => s.Account == account);

                if (synchronizationStatus == null)
                {
                    synchronizationStatus = new SynchronizationStatus { Account = account };
                    _dataContext.SynchronizationStatuses.Add(synchronizationStatus);
                }

                synchronizationStatus.LastUpdatedCombinedBlockHeight = lastUpdatedCombinedBlockHeight;
                _dataContext.SaveChanges();
            }
        }

        public void SetAccountCompromised(long accountId)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);
                account.IsCompromised = true;
            }
        }

        public string GetAccountKeyValue(long accountId, string key)
        {
            lock(_sync)
            {
                return _dataContext.AccountKeyValues.FirstOrDefault(kv => kv.AccountId == accountId && kv.Key == key)?.Value;
            }
        }

        public Dictionary<string, string> GetAccountKeyValues(long accountId, params string[] filter)
        {
            lock(_sync)
            {
                var accountKvs = _dataContext.AccountKeyValues.Where(kv => kv.AccountId == accountId).ToList();

                return accountKvs.Where(kv => filter == null || !filter.Any() || filter.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
            }
        }

        public void SetAccountKeyValues(long accountId, Dictionary<string, string> keyValues)
        {
            if(keyValues == null || keyValues.Count == 0)
            {
                return;
            }

            lock(_sync)
            {
                var accountKvs = _dataContext.AccountKeyValues.Where(kv => kv.AccountId == accountId).ToList();
                var forUpdate = accountKvs.Where(kv => keyValues.ContainsKey(kv.Key) && !string.IsNullOrEmpty(keyValues[kv.Key])).ToList();
                var forDelete = accountKvs.Where(kv => keyValues.ContainsKey(kv.Key) && string.IsNullOrEmpty(keyValues[kv.Key])).ToList();

                var forAdd = keyValues.Where(kv => forUpdate.Find(u => u.Key == kv.Key) == null);

                foreach (var item in forUpdate)
                {
                    item.Value = keyValues[item.Key];
                }

                foreach (var item in forAdd)
                {
                    _dataContext.AccountKeyValues.Add(new AccountKeyValue { AccountId = accountId, Key = item.Key, Value = item.Value });
                }

                foreach (var item in forDelete)
                {
                    _dataContext.AccountKeyValues.Remove(item);
                }

                if(forUpdate.Any() || forAdd.Any() || forDelete.Any())
                {
                    _dataContext.SaveChanges();
                }
            }
        }

        public void RemoveAccountKeyValues(long accountId, IEnumerable<string> keys)
        {
            if (keys == null || !keys.Any())
            {
                return;
            }

            lock (_sync)
            {
                var accountKvs = _dataContext.AccountKeyValues.Where(kv => kv.AccountId == accountId).ToList();
                var forDelete = accountKvs.Where(kv => keys.Contains(kv.Key)).ToList();

                foreach (var item in forDelete)
                {
                    _dataContext.AccountKeyValues.Remove(item);
                }
            }
        }

        public List<Account> GetAccounts()
        {
            lock (_sync)
            {
                return _dataContext.Accounts.ToList();
            }
        }

        public Account GetAccount(long accountId)
        {
            lock (_sync)
            {
                return _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);
            }
        }

        public Account GetAccount(byte[] publicKey)
        {
            lock (_sync)
            {
                return _dataContext.Accounts
                    .Where(a => a.AccountType != AccountType.User)
                    .AsEnumerable()
                    .FirstOrDefault(a =>
                        {
                            byte[] pk = ConfidentialAssetsHelper.GetPublicKey(Ed25519.SecretKeyFromSeed(a.SecretSpendKey));
                            return pk.Equals32(publicKey);
                        });
            }
        }

        public List<Account> GetAccountsByType(AccountType accountType)
        {
            lock (_sync)
            {
                return _dataContext.Accounts.Where(a => a.AccountType == accountType).ToList();
            }
        }


        public bool GetAccountId(byte[] publicKey, out long accountId)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.AsEnumerable().FirstOrDefault(a => a.PublicSpendKey.Equals32(publicKey));

                if (account != null)
                {
                    accountId = account.AccountId;
                    return true;
                }
            }

            accountId = 0;
            return false;
        }

        public bool GetAccountId(byte[] publicSpendKey, byte[] publicViewKey, out long accountId)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.AsEnumerable().FirstOrDefault(a => a.PublicSpendKey.Equals32(publicSpendKey) && a.PublicViewKey.Equals32(publicViewKey));

                if (account != null)
                {
                    accountId = account.AccountId;
                    return true;
                }
            }

            accountId = 0;
            return false;
        }

        public long AddAccount(byte accountType, string accountInfo, byte[] secretSpendKeyEnc, byte[] secretViewKeyEnc, byte[] publicSpendKey, byte[] publicViewKey, ulong lastAggregatedRegistrations, bool isPrivate = false)
        {
            lock (_sync)
            {
                Account account = new Account
                {
                    AccountType = (AccountType)accountType,
                    AccountInfo = accountInfo,
                    SecretSpendKey = secretSpendKeyEnc,
                    SecretViewKey = secretViewKeyEnc,
                    PublicSpendKey = publicSpendKey,
                    PublicViewKey = publicViewKey,
                    LastAggregatedRegistrations = lastAggregatedRegistrations,
                    IsPrivate = isPrivate
                };

                _dataContext.Accounts.Add(account);

                SynchronizationStatus synchronizationStatus = new SynchronizationStatus
                {
                    Account = account,
                    LastUpdatedCombinedBlockHeight = lastAggregatedRegistrations
                };

                _dataContext.SynchronizationStatuses.Add(synchronizationStatus);

                _dataContext.SaveChanges();

                return account.AccountId;
            }
        }

        public void UpdateAccount(long accountId, string accountInfo, byte[] publicSpendKey, byte[] publicViewKey)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);
                if (account != null)
                {
                    account.AccountInfo = accountInfo;
                    account.PublicSpendKey = publicSpendKey;
                    account.PublicViewKey = publicViewKey;

                    _dataContext.SaveChanges();
                }
            }
        }

        public void ResetAccount(long accountId, byte[] secretSpendKeyEnc, byte[] secretViewKeyEnc, byte[] publicSpendKey, byte[] publicViewKey, ulong lastAggregatedRegistrations)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);
                if (account != null)
                {
                    account.IsCompromised = false;
                    account.LastAggregatedRegistrations = lastAggregatedRegistrations;
                    account.SecretSpendKey = secretSpendKeyEnc;
                    account.SecretViewKey = secretViewKeyEnc;
                    account.PublicSpendKey = publicSpendKey;
                    account.PublicViewKey = publicViewKey;

                    IEnumerable<UserRootAttribute> rootAttributes = _dataContext.UserRootAttributes.Where(r => r.AccountId == accountId);
                    foreach (var rootAttribute in rootAttributes)
                    {
                        _dataContext.UserRootAttributes.Remove(rootAttribute);
                    }

                    IEnumerable<UserAssociatedAttribute> associatedAttributes = _dataContext.UserAssociatedAttributes.Where(r => r.AccountId == accountId);
                    foreach (var associatedAttribute in associatedAttributes)
                    {
                        _dataContext.UserAssociatedAttributes.Remove(associatedAttribute);
                    }

                    var spDocuments = _dataContext.SpDocuments.Include(d => d.Account).Where(d => d.Account.AccountId == accountId).ToList();
                    foreach (var item in spDocuments)
                    {
                        var spDocumentSignatures = _dataContext.SpDocumentSignatures.Include(s => s.Document).Where(s => s.Document.SpDocumentId == item.SpDocumentId).ToList();
                        foreach (var signature in spDocumentSignatures)
                        {
                            _dataContext.SpDocumentSignatures.Remove(signature);
                        }

                        _dataContext.SpDocuments.Remove(item);
                    }

                    var spEmployeeGroups = _dataContext.SpEmployeeGroups.Include(g => g.Account).Where(g => g.Account.AccountId == accountId).ToList();
                    foreach (var item in spEmployeeGroups)
                    {
                        var spEmployees = _dataContext.SpEmployees.Include(e => e.SpEmployeeGroup).Where(e => e.SpEmployeeGroup.SpEmployeeGroupId == item.SpEmployeeGroupId).ToList();
                        foreach (var employee in spEmployees)
                        {
                            _dataContext.SpEmployees.Remove(employee);
                        }

                        _dataContext.SpEmployeeGroups.Remove(item);
                    }

                    var spDocumentAllowedSigners = _dataContext.SpDocumentAllowedSigners.Include(s => s.Account).Where(s => s.Account.AccountId == accountId).ToList();
                    foreach (var item in spDocumentAllowedSigners)
                    {
                        _dataContext.SpDocumentAllowedSigners.Remove(item);
                    }

                    var userGroupRelations = _dataContext.UserGroupRelations.Include(r => r.Account).Where(r => r.Account.AccountId == accountId).ToList();
                    foreach (var item in userGroupRelations)
                    {
                        _dataContext.UserGroupRelations.Remove(item);
                    }

                    _dataContext.SaveChanges();
                }
            }
        }

        public long DuplicateUserAccount(long accountId, string accountInfo)
        {
            long accountIdTarget = 0;

            lock (_sync)
            {
                Account accountSource = _dataContext.Accounts.FirstOrDefault(a => a.AccountType == AccountType.User && a.AccountId == accountId);

                if (accountSource != null)
                {
                    Account accountTarget = new Account
                    {
                        AccountInfo = accountInfo,
                        AccountType = AccountType.User,
                        PublicSpendKey = accountSource.PublicSpendKey,
                        PublicViewKey = accountSource.PublicViewKey,
                        SecretSpendKey = accountSource.SecretSpendKey,
                        SecretViewKey = accountSource.SecretViewKey,
                        LastAggregatedRegistrations = accountSource.LastAggregatedRegistrations
                    };

                    _dataContext.Accounts.Add(accountTarget);

                    SynchronizationStatus synchronizationStatusSource = _dataContext.SynchronizationStatuses.FirstOrDefault(s => s.Account.AccountId == accountId);
                    if (synchronizationStatusSource != null)
                    {
                        SynchronizationStatus synchronizationStatus = new SynchronizationStatus
                        {
                            Account = accountTarget,
                            LastUpdatedCombinedBlockHeight = accountSource.LastAggregatedRegistrations
                        };

                        _dataContext.SynchronizationStatuses.Add(synchronizationStatus);
                    }

                    _dataContext.SaveChanges();

                    accountIdTarget = accountTarget.AccountId;

                    if (accountIdTarget > 0)
                    {
                        foreach (var rootAttr in _dataContext.UserRootAttributes.Where(a => a.AccountId == accountId && !a.IsOverriden && a.NextKeyImage != string.Empty).ToList())
                        {
                            AddNonConfirmedRootAttribute(accountIdTarget, rootAttr.Content, rootAttr.Source, rootAttr.SchemeName, rootAttr.AssetId);
                        }

                        foreach (var groupRelation in _dataContext.UserGroupRelations.Include(a => a.Account).Where(a => a.Account.AccountId == accountId).ToList())
                        {
                            UserGroupRelation userGroupRelation = new UserGroupRelation
                            {
                                Account = accountTarget,
                                AssetId = groupRelation.AssetId,
                                Issuer = groupRelation.Issuer,
                                GroupName = groupRelation.GroupName,
                                GroupOwnerKey = groupRelation.GroupOwnerKey,
                                GroupOwnerName = groupRelation.GroupOwnerName
                            };

                            _dataContext.UserGroupRelations.Add(userGroupRelation);
                        }

                        UserSettings userSettings = new UserSettings
                        {
                            Account = accountTarget,
                            IsAutoTheftProtection = false
                        };

                        _dataContext.UserSettings.Add(userSettings);
                    }

                    _dataContext.SaveChanges();
                }
            }

            return accountIdTarget;
        }

        public byte[] GetAesInitializationVector()
        {
            lock (_sync)
            {
                return _dataContext.SystemSettings.FirstOrDefault()?.InitializationVector;
            }
        }

        public byte[] GetBiometricSecretKey()
        {
            lock (_sync)
            {
                return _dataContext.SystemSettings.FirstOrDefault()?.BiometricSecretKey;
            }
        }

        public void AddBiometricRecord(string userData, Guid personGuid)
        {
            lock (_sync)
            {
                BiometricRecord biometricRecord = new BiometricRecord
                {
                    PersonGuid = personGuid,
                    UserData = userData
                };

                _dataContext.BiometricRecords.Add(biometricRecord);

                _dataContext.SaveChanges();
            }
        }

        public void UpdateBiometricRecord(string userData, Guid personGuid)
        {
            lock (_sync)
            {
                BiometricRecord biometricRecord = _dataContext.BiometricRecords.FirstOrDefault(b => b.UserData == userData);

                biometricRecord.PersonGuid = personGuid;

                _dataContext.SaveChanges();
            }
        }

        public Guid FindPersonGuid(string userData)
        {
            lock (_sync)
            {
                return _dataContext.BiometricRecords.FirstOrDefault(p => p.UserData == userData)?.PersonGuid ?? Guid.Empty;
            }
        }

        public bool RemoveBiometricPerson(string userData)
        {
            lock (_sync)
            {
                BiometricRecord biometricRecord = _dataContext.BiometricRecords.FirstOrDefault(p => p.UserData == userData);

                if (biometricRecord != null)
                {
                    _dataContext.BiometricRecords.Remove(biometricRecord);

                    _dataContext.SaveChanges();

                    return true;
                }

                return false;
            }
        }

        public void ClearAccountCompromised(long accountId)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);

                if (account != null)
                {
                    account.IsCompromised = false;
                    _dataContext.SaveChanges();
                }
            }
        }

        public void RemoveAccount(long accountId)
        {
            lock (_sync)
            {
                try
                {

                    bool deleted = false;

                    SynchronizationStatus synchronizationStatus = _dataContext.SynchronizationStatuses.Include(s => s.Account).FirstOrDefault(s => s.Account.AccountId == accountId);
                    if (synchronizationStatus != null)
                    {
                        _dataContext.SynchronizationStatuses.Remove(synchronizationStatus);
                        deleted = true;
                    }

                    IEnumerable<UserRootAttribute> rootAttributes = _dataContext.UserRootAttributes.Where(r => r.AccountId == accountId);
                    foreach (var rootAttribute in rootAttributes)
                    {
                        _dataContext.UserRootAttributes.Remove(rootAttribute);
                        deleted = true;
                    }

                    IEnumerable<UserAssociatedAttribute> associatedAttributes = _dataContext.UserAssociatedAttributes.Where(r => r.AccountId == accountId);
                    foreach (var associatedAttribute in associatedAttributes)
                    {
                        _dataContext.UserAssociatedAttributes.Remove(associatedAttribute);
                        deleted = true;
                    }

                    var userSettings = _dataContext.UserSettings.Include(s => s.Account).Where(s => s.Account != null && s.Account.AccountId == accountId).ToList();
                    foreach (var item in userSettings)
                    {
                        _dataContext.UserSettings.Remove(item);
                    }

                    var autoLogins = _dataContext.AutoLogins.Include(a => a.Account).Where(a => a.Account.AccountId == accountId).ToList();
                    foreach (var item in autoLogins)
                    {
                        _dataContext.AutoLogins.Remove(item);
                    }

                    var spDocuments = _dataContext.SpDocuments.Include(d => d.Account).Where(d => d.Account.AccountId == accountId).ToList();
                    foreach (var item in spDocuments)
                    {
                        var spDocumentSignatures = _dataContext.SpDocumentSignatures.Include(s => s.Document).Where(s => s.Document.SpDocumentId == item.SpDocumentId).ToList();
                        foreach (var signature in spDocumentSignatures)
                        {
                            _dataContext.SpDocumentSignatures.Remove(signature);
                        }

                        _dataContext.SpDocuments.Remove(item);
                    }

                    var spEmployeeGroups = _dataContext.SpEmployeeGroups.Include(g => g.Account).Where(g => g.Account.AccountId == accountId).ToList();
                    foreach (var item in spEmployeeGroups)
                    {
                        var spEmployees = _dataContext.SpEmployees.Include(e => e.SpEmployeeGroup).Where(e => e.SpEmployeeGroup.SpEmployeeGroupId == item.SpEmployeeGroupId).ToList();
                        foreach (var employee in spEmployees)
                        {
                            _dataContext.SpEmployees.Remove(employee);
                        }

                        _dataContext.SpEmployeeGroups.Remove(item);
                    }

                    var spDocumentAllowedSigners = _dataContext.SpDocumentAllowedSigners.Include(s => s.Account).Where(s => s.Account.AccountId == accountId).ToList();
                    foreach (var item in spDocumentAllowedSigners)
                    {
                        _dataContext.SpDocumentAllowedSigners.Remove(item);
                    }

                    var userGroupRelations = _dataContext.UserGroupRelations.Include(r => r.Account).Where(r => r.Account.AccountId == accountId).ToList();
                    foreach (var item in userGroupRelations)
                    {
                        _dataContext.UserGroupRelations.Remove(item);
                    }

                    var userRegistrations = _dataContext.UserRegistrations.Include(r => r.Account).Where(r => r.Account.AccountId == accountId).ToList();
                    foreach (var userRegistration in userRegistrations)
                    {
                        _dataContext.UserRegistrations.Remove(userRegistration);
                    }

                    Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);
                    if (account != null)
                    {
                        _dataContext.Accounts.Remove(account);
                        deleted = true;
                    }

                    if (deleted)
                    {
                        _dataContext.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to remove account", ex);
                    throw;
                }
            }
        }

        public long AddSpEmployeeGroup(long accountId, string groupName)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);

                if (account != null && !_dataContext.SpEmployeeGroups.Include(g => g.Account).Any(g => g.Account == account && g.GroupName.Equals(groupName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    SpEmployeeGroup spEmployeeGroup = new SpEmployeeGroup
                    {
                        Account = account,
                        GroupName = groupName
                    };

                    _dataContext.SpEmployeeGroups.Add(spEmployeeGroup);
                    _dataContext.SaveChanges();

                    return spEmployeeGroup.SpEmployeeGroupId;
                }
            }

            return 0;
        }

        public void RemoveSpEmployeeGroup(long accountId, long groupId)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);
                if (account != null)
                {
                    SpEmployeeGroup spEmployeeGroup = _dataContext.SpEmployeeGroups.FirstOrDefault(g => g.Account == account && g.SpEmployeeGroupId == groupId);

                    if (spEmployeeGroup != null)
                    {
                        _dataContext.SpEmployeeGroups.Remove(spEmployeeGroup);
                        _dataContext.SaveChanges();
                    }
                }
            }
        }

        public IEnumerable<SpEmployeeGroup> GetSpEmployeeGroups(long accountId)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);
                if (account != null)
                {
                    return _dataContext.SpEmployeeGroups.Where(g => g.Account == account).ToList();
                }
            }

            return null;
        }

        public long AddSpEmployee(long accountId, string description, string rootAttributeRaw, long groupId)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);
                if (account != null)
                {
                    SpEmployeeGroup spEmployeeGroup = _dataContext.SpEmployeeGroups.FirstOrDefault(g => g.Account == account && g.SpEmployeeGroupId == groupId);

                    SpEmployee spEmployee = new SpEmployee
                    {
                        Account = account
                    };

                    spEmployee.RootAttributeRaw = rootAttributeRaw;
                    spEmployee.SpEmployeeGroup = spEmployeeGroup;
                    spEmployee.Description = description;
                    _dataContext.SpEmployees.Add(spEmployee);

                    _dataContext.SaveChanges();

                    return spEmployee.SpEmployeeId;
                }
            }

            return 0;
        }

        public List<SpEmployee> GetSpEmployees(long accountId, string attributeContent)
        {
            lock (_sync)
            {
                List<SpEmployee> spEmployees = _dataContext.SpEmployees.Include(s => s.Account).Include(s => s.SpEmployeeGroup).Where(e => e.Account.AccountId == accountId && e.RootAttributeRaw == attributeContent).ToList();

                return spEmployees;
            }
        }

        public List<SpEmployee> GetSpEmployees(long accountId)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);
                if (account != null)
                {
                    List<SpEmployee> spEmployees = _dataContext.SpEmployees.Where(e => e.Account == account).ToList();

                    return spEmployees;
                }

                return null;
            }
        }

        public SpEmployee SetSpEmployeeRegistrationCommitment(long accountId, long relationId, string registrationCommitment)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);
                if (account != null)
                {
                    SpEmployee spEmployee = _dataContext.SpEmployees.FirstOrDefault(e => e.SpEmployeeId == relationId);

                    if (spEmployee != null)
                    {
                        spEmployee.RegistrationCommitment = registrationCommitment;

                        _dataContext.SaveChanges();

                        return spEmployee;
                    }
                }
            }

            return null;
        }

        public void UpdateSpEmployeeCategory(long accountId, long spEmployeeId, long groupId)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);
                if (account != null)
                {
                    SpEmployee spEmployee = _dataContext.SpEmployees.Include(g => g.Account).FirstOrDefault(e => e.Account.AccountId == accountId && e.SpEmployeeId == spEmployeeId);

                    if (spEmployee != null)
                    {
                        SpEmployeeGroup spEmployeeGroup = _dataContext.SpEmployeeGroups.Include(g => g.Account).FirstOrDefault(g => g.Account.AccountId == accountId && g.SpEmployeeGroupId == groupId);

                        spEmployee.SpEmployeeGroup = spEmployeeGroup;
                        spEmployee.RegistrationCommitment = null;

                        _dataContext.SaveChanges();
                    }
                }
            }
        }

        public SpEmployee RemoveSpEmployee(long accountId, long spEmployeeId)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);
                if (account != null)
                {
                    SpEmployee spEmployee = _dataContext.SpEmployees.FirstOrDefault(e => e.Account == account && e.SpEmployeeId == spEmployeeId);
                    if (spEmployee != null)
                    {
                        _dataContext.SpEmployees.Remove(spEmployee);

                        _dataContext.SaveChanges();
                    }

                    return spEmployee;
                }

                return null;
            }
        }

        public IEnumerable<SpEmployee> GetSpEmployeesByGroup(long accountId, long groupId)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);
                if (account != null)
                {
                    SpEmployeeGroup spEmployeeGroup = _dataContext.SpEmployeeGroups.Include(g => g.Account).FirstOrDefault(g => g.Account == account && g.SpEmployeeGroupId == groupId);

                    return _dataContext.SpEmployees.Include(e => e.Account).Include(e => e.SpEmployeeGroup).Where(e => e.Account == account && e.SpEmployeeGroup == spEmployeeGroup).ToList();
                }
            }

            return null;
        }

        public IEnumerable<SpEmployee> GetAllSpEmployees(long accountId)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);

                if (account != null)
                {
                    return _dataContext.SpEmployees.Include(e => e.Account).Where(e => e.Account == account).ToList();
                }
            }

            return null;
        }

        public IEnumerable<SpEmployee> GetSpEmployeesUngrouped(long accountId)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);

                if (account != null)
                {
                    return _dataContext.SpEmployees.Include(e => e.Account).Include(e => e.SpEmployeeGroup).Where(e => e.Account == account && e.SpEmployeeGroup == null).ToList();
                }
            }

            return null;
        }

        public bool IsSpEmployeeExist(long accountId, string registrationCommitment)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);

                if (account != null)
                {
                    return _dataContext.SpEmployees.Include(e => e.Account).Include(e => e.SpEmployeeGroup).Any(e => e.Account == account && e.SpEmployeeGroup != null && e.RegistrationCommitment == registrationCommitment);
                }
            }

            return false;
        }

        public IEnumerable<SpDocument> GetSpDocuments(long accountId)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);

                if (account != null)
                {
                    return _dataContext.SpDocuments.Include(d => d.Account).Include(d => d.AllowedSigners).Include(d => d.DocumentSignatures).Where(d => d.Account.AccountId == accountId).ToList();
                }

                return null;
            }
        }

        public long AddSpDocument(long accountId, string documentName, string hash)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);

                if (account != null)
                {
                    SpDocument spDocument = new SpDocument
                    {
                        Account = account,
                        DocumentName = documentName,
                        Hash = hash
                    };

                    _dataContext.SpDocuments.Add(spDocument);
                    _dataContext.SaveChanges();

                    return spDocument.SpDocumentId;
                }

                return 0;
            }
        }

        public void RemoveSpDocument(long accountId, long spDocumentId)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);

                if (account != null)
                {
                    SpDocument spDocument = _dataContext.SpDocuments.Include(d => d.Account).Include(d => d.AllowedSigners).FirstOrDefault(d => d.SpDocumentId == spDocumentId);

                    if (spDocument != null)
                    {
                        if (spDocument.AllowedSigners != null)
                        {
                            foreach (var item in spDocument.AllowedSigners)
                            {
                                _dataContext.SpDocumentAllowedSigners.Remove(item);
                            }
                        }

                        _dataContext.SpDocuments.Remove(spDocument);

                        _dataContext.SaveChanges();
                    }
                }
            }
        }

        public long AddSpDocumentAllowedSigner(long accountId, long spDocumentId, string groupOwner, string groupName, string groupCommitment, string blindingFactor)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);

                if (account != null)
                {
                    SpDocument spDocument = _dataContext.SpDocuments.Include(d => d.Account).Include(d => d.AllowedSigners).FirstOrDefault(d => d.SpDocumentId == spDocumentId);

                    if (spDocument != null)
                    {
                        SpDocumentAllowedSigner allowedSigner = new SpDocumentAllowedSigner
                        {
                            Account = account,
                            Document = spDocument,
                            GroupIssuer = groupOwner,
                            GroupName = groupName,
                            GroupCommitment = groupCommitment,
                            BlindingFactor = blindingFactor
                        };

                        _dataContext.SpDocumentAllowedSigners.Add(allowedSigner);

                        _dataContext.SaveChanges();

                        return allowedSigner.SpDocumentAllowedSignerId;
                    }
                }

                return 0;
            }
        }

        public long RemoveSpDocumentAllowedSigner(long accountId, long spDocumentAllowedSignerId)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);

                if (account != null)
                {
                    SpDocumentAllowedSigner allowedSigner = _dataContext.SpDocumentAllowedSigners.Include(s => s.Document).FirstOrDefault(s => s.SpDocumentAllowedSignerId == spDocumentAllowedSignerId);

                    _dataContext.SpDocumentAllowedSigners.Remove(allowedSigner);

                    _dataContext.SaveChanges();

                    return allowedSigner.Document.SpDocumentId;
                }

                return 0;
            }
        }

        public SignedDocumentEntity GetSpDocument(long accountId, string hash)
        {
            SignedDocumentEntity signedDocument = null;

            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);

                if (account != null)
                {
                    SpDocument spDocument = _dataContext.SpDocuments.Include(d => d.Account).Include(d => d.AllowedSigners).FirstOrDefault(d => d.Account.AccountId == accountId && d.Hash == hash);

                    if (spDocument != null)
                    {
                        signedDocument = new SignedDocumentEntity
                        {
                            DocumentId = spDocument.SpDocumentId,
                            DocumentName = spDocument.DocumentName,
                            Hash = spDocument.Hash,
                            LastChangeRecordHeight = spDocument.LastChangeRecordHeight,
                            AllowedSigners = spDocument.AllowedSigners.Select(s =>
                                new AllowedSignerEntity
                                {
                                    GroupCommitment = s.GroupCommitment.HexStringToByteArray(),
                                    BlindingFactor = s.BlindingFactor.HexStringToByteArray(),
                                    GroupIssuer = s.GroupIssuer,
                                    GroupName = s.GroupName
                                }).ToList()
                        };
                    }
                }
            }

            return signedDocument;
        }

        public SpDocument GetSpDocument(long accountId, long spDocumentId)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);

                if (account != null)
                {
                    SpDocument spDocument = _dataContext.SpDocuments.Include(d => d.Account).Include(d => d.AllowedSigners).FirstOrDefault(d => d.Account.AccountId == accountId && d.SpDocumentId == spDocumentId);

                    return spDocument;
                }

                return null;
            }
        }

        public bool UpdateSpDocumentSignature(long accountId, string documentHash, ulong documentRecordHeight, ulong signatureRecordHeight, byte[] documentSignRecord)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);

                if (account != null)
                {
                    SpDocumentSignature spDocumentSignature = _dataContext.SpDocumentSignatures.Include(d => d.Document).ThenInclude(d => d.Account).FirstOrDefault(s => s.Document.Account.AccountId == accountId && s.Document.Hash.Equals(documentHash) && s.DocumentRecordHeight == documentRecordHeight && s.SignatureRecordHeight == signatureRecordHeight);

                    if (spDocumentSignature != null)
                    {
                        spDocumentSignature.DocumentSignRecord = documentSignRecord;

                        _dataContext.SaveChanges();

                        return false;
                    }
                }

                return false;
            }
        }


        public long AddSpDocumentSignature(long accountId, long spDocumentId, ulong documentRecordHeight, ulong blockHeight)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);

                if (account != null)
                {
                    SpDocument spDocument = _dataContext.SpDocuments.Include(d => d.Account).FirstOrDefault(d => d.Account.AccountId == accountId && d.SpDocumentId == spDocumentId);

                    if (spDocument != null)
                    {
                        SpDocumentSignature documentSignature = new SpDocumentSignature
                        {
                            Document = spDocument,
                            SignatureRecordHeight = blockHeight,
                            DocumentRecordHeight = documentRecordHeight
                        };

                        _dataContext.SpDocumentSignatures.Add(documentSignature);
                        _dataContext.SaveChanges();

                        return documentSignature.SpDocumentSignatureId;
                    }
                }

                return 0;
            }
        }

        public IEnumerable<SpDocumentSignature> GetSpDocumentSignatures(long accountId, long spDocumentId)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);

                if (account != null)
                {
                    SpDocument spDocument = _dataContext.SpDocuments.Include(d => d.Account).Include(d => d.DocumentSignatures).FirstOrDefault(d => d.Account.AccountId == accountId && d.SpDocumentId == spDocumentId);

                    if (spDocument != null)
                    {
                        return spDocument.DocumentSignatures.ToList();
                    }
                }

                return null;
            }
        }

        public void UpdateSpDocumentChangeRecord(long accountId, string hash, ulong recordHeight)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);

                if (account != null)
                {
                    SpDocument spDocument = _dataContext.SpDocuments.Include(d => d.Account).FirstOrDefault(d => d.Account.AccountId == accountId && d.Hash.Equals(hash));

                    if (spDocument.LastChangeRecordHeight < recordHeight)
                    {
                        spDocument.LastChangeRecordHeight = recordHeight;

                        _dataContext.SaveChanges();
                    }
                }
            }
        }

        public long AddUserGroupRelation(long accountId, string groupOwnerName, string groupOwnerKey, string groupName, string assetId, string issuer)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);

                if (account != null)
                {
                    if (_dataContext.UserGroupRelations.Local.Any(u => (u.Account?.AccountId ?? 0) == accountId && u.GroupOwnerKey == groupOwnerKey && u.GroupName == groupName && u.Issuer == issuer && u.AssetId == assetId) ||
                        _dataContext.UserGroupRelations.Include(u => u.Account).Where(u => u.Account != null).Any(u => u.Account.AccountId == accountId && u.GroupOwnerKey == groupOwnerKey && u.GroupName == groupName && u.Issuer == issuer && u.AssetId == assetId))
                    {
                        return 0;
                    }

                    UserGroupRelation userGroupRelation = new UserGroupRelation
                    {
                        Account = account,
                        GroupOwnerName = groupOwnerName,
                        GroupOwnerKey = groupOwnerKey,
                        GroupName = groupName,
                        AssetId = assetId,
                        Issuer = issuer
                    };

                    _dataContext.UserGroupRelations.Add(userGroupRelation);
                    _dataContext.SaveChanges();

                    return userGroupRelation.UserGroupRelationId;
                }

                return 0;
            }
        }

        public (string groupOwnerName, string issuer, string assetId) GetRelationUserAttributes(long accountId, string groupOwnerKey, string groupName)
        {
            lock (_sync)
            {
                UserGroupRelation groupRelation = _dataContext.UserGroupRelations.Include(g => g.Account)
                    .FirstOrDefault(g => g.Account.AccountId == accountId && g.GroupOwnerKey == groupOwnerKey && g.GroupName == groupName);

                if (groupRelation != null)
                {
                    return (groupRelation.GroupOwnerName, groupRelation.Issuer, groupRelation.AssetId);
                }

                return (null, null, null);
            }
        }


        public void RemoveUserGroupRelation(long userGroupRelationId)
        {
            lock (_sync)
            {
                UserGroupRelation userGroupRelation = _dataContext.UserGroupRelations.FirstOrDefault(g => g.UserGroupRelationId == userGroupRelationId);

                if (userGroupRelation != null)
                {
                    _dataContext.UserGroupRelations.Remove(userGroupRelation);
                    _dataContext.SaveChanges();
                }
            }
        }

        public IEnumerable<UserGroupRelation> GetUserGroupRelations(long accountId)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);

                if (account != null)
                {
                    IEnumerable<UserGroupRelation> userGroupRelations = _dataContext.UserGroupRelations.Include(g => g.Account).Where(g => g.Account.AccountId == accountId).ToList();

                    return userGroupRelations;
                }

                return null;
            }
        }

        public void RemoveUserRegistration(long registrationId)
        {
            lock (_sync)
            {
                var userRegistration = _dataContext.UserRegistrations.FirstOrDefault(r => r.UserRegistrationId == registrationId);

                if (userRegistration != null)
                {
                    _dataContext.UserRegistrations.Remove(userRegistration);

                    _dataContext.SaveChanges();
                }
            }
        }

        public void RemoveUserRegistration(long accountId, string commitment)
        {
            lock (_sync)
            {
                var userRegistration = _dataContext.UserRegistrations.Include(r => r.Account).FirstOrDefault(r => r.Account.AccountId == accountId && r.Commitment == commitment);

                if (userRegistration != null)
                {
                    _dataContext.UserRegistrations.Remove(userRegistration);

                    _dataContext.SaveChanges();
                }
            }
        }

        public long AddUserRegistration(long accountId, string commitment, string spInfo, string assetId, string issuer)
        {
            lock (_sync)
            {
                if (!_dataContext.UserRegistrations.Include(r => r.Account).Any(r => r.Account.AccountId == accountId && r.AssetId == assetId && r.Issuer == r.Issuer && r.Commitment == commitment))
                {
                    Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);
                    UserRegistration userRegistration = new UserRegistration
                    {
                        Account = account,
                        Commitment = commitment,
                        ServiceProviderInfo = spInfo,
                        AssetId = assetId,
                        Issuer = issuer
                    };

                    _dataContext.UserRegistrations.Add(userRegistration);

                    _dataContext.SaveChanges();

                    return userRegistration.UserRegistrationId;
                }

                return 0;
            }
        }

        public List<UserRegistration> GetUserRegistrations(long accountId)
        {
            lock (_sync)
            {
                return _dataContext.UserRegistrations.Include(r => r.Account).Where(r => r.Account.AccountId == accountId).ToList();
            }
        }

        public string[] GetServiceProviderRelationGroups(long accountId)
        {
            lock (_sync)
            {
                return _dataContext.SpEmployeeGroups.Include(r => r.Account).Where(r => r.Account.AccountId == accountId)?.Select(r => r.GroupName).ToArray();
            }
        }

        public IEnumerable<SpUserTransaction> GetSpUserTransactions(long accountId)
        {
            lock (_sync)
            {
                return _dataContext.SpUserTransactions.Where(s => s.AccountId == accountId).ToList();
            }
        }

        public long AddSpUserTransaction(long accountId, long registrationId, string transactionId, string description)
        {
            lock (_sync)
            {
                SpUserTransaction spUserTransaction = new SpUserTransaction
                {
                    AccountId = accountId,
                    ServiceProviderRegistrationId = registrationId,
                    TransactionId = transactionId,
                    TransactionDescription = description
                };

                _dataContext.SpUserTransactions.Add(spUserTransaction);
                _dataContext.SaveChanges();

                return spUserTransaction.SpUserTransactionId;
            }
        }

        public bool SetSpUserTransactionConfirmed(long accountId, string transactionId)
        {
            lock (_sync)
            {
                SpUserTransaction spUserTransaction = _dataContext.SpUserTransactions.FirstOrDefault(s => s.AccountId == accountId && s.TransactionId == transactionId);

                if (spUserTransaction != null)
                {
                    spUserTransaction.IsProcessed = true;
                    spUserTransaction.IsConfirmed = true;
                    _dataContext.SaveChanges();

                    return true;
                }

                return false;
            }
        }

        public bool SetSpUserTransactionDeclined(long accountId, string transactionId)
        {
            lock (_sync)
            {
                SpUserTransaction spUserTransaction = _dataContext.SpUserTransactions.FirstOrDefault(s => s.AccountId == accountId && s.TransactionId == transactionId);

                if (spUserTransaction != null)
                {
                    spUserTransaction.IsProcessed = true;
                    spUserTransaction.IsConfirmed = false;
                    _dataContext.SaveChanges();

                    return true;
                }

                return false;
            }
        }

        public bool SetSpUserTransactionCompromised(long accountId, string transactionId)
        {
            lock (_sync)
            {
                SpUserTransaction spUserTransaction = _dataContext.SpUserTransactions.FirstOrDefault(s => s.AccountId == accountId && s.TransactionId == transactionId);

                if (spUserTransaction != null)
                {
                    spUserTransaction.IsProcessed = true;
                    spUserTransaction.IsCompromised = true;
                    _dataContext.SaveChanges();
                    return true;
                }

                return false;
            }
        }

        public long AddAutoLogin(long accountId, byte[] secretKey)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);

                if (account != null)
                {
                    AutoLogin autoLogin = new AutoLogin
                    {
                        Account = account,
                        SecretKey = secretKey
                    };

                    _dataContext.AutoLogins.Add(autoLogin);

                    _dataContext.SaveChanges();

                    return autoLogin.AutoLoginId;
                }

                return 0;
            }
        }

        public IEnumerable<AutoLogin> GetAutoLogins()
        {
            lock (_sync)
            {
                return _dataContext.AutoLogins.Include(a => a.Account).ToList();
            }
        }

        public bool IsAutoLoginExist(long accountId)
        {
            lock (_sync)
            {
                return _dataContext.AutoLogins.Include(a => a.Account).Any(a => a.Account.AccountId == accountId);
            }
        }

        public IEnumerable<SamlIdentityProvider> GetSamlIdentityProviders()
        {
            lock (_sync)
            {
                return _dataContext.SamlIdentityProviders.ToList();
            }
        }

        public long SetSamlIdentityProvider(string entityId, string publicSpendKey, string secretViewKey)
        {
            lock (_sync)
            {
                SamlIdentityProvider samlIdentityProvider = _dataContext.SamlIdentityProviders.FirstOrDefault(i => i.EntityId == entityId);

                if (samlIdentityProvider == null)
                {
                    samlIdentityProvider = new SamlIdentityProvider();
                    _dataContext.SamlIdentityProviders.Add(samlIdentityProvider);
                }

                samlIdentityProvider.EntityId = entityId;
                samlIdentityProvider.PublicSpendKey = publicSpendKey;
                samlIdentityProvider.SecretViewKey = secretViewKey;

                _dataContext.SaveChanges();

                return samlIdentityProvider.SamlIdentityProviderId;
            }
        }

        public bool RemoveSamlIdentityProvider(string entityId)
        {
            bool found = false;

            lock (_sync)
            {
                foreach (SamlIdentityProvider item in _dataContext.SamlIdentityProviders.Where(s => s.EntityId == entityId).ToList())
                {
                    _dataContext.SamlIdentityProviders.Remove(item);
                    found = true;
                }

                _dataContext.SaveChanges();
            }

            return found;
        }

        public void SetSamlSettings(long defaultSamlIdpId, long defaultSamlIdpAccountId)
        {
            lock (_sync)
            {
                SamlSettings samlSettings = _dataContext.SamlSettings.FirstOrDefault();

                if (samlSettings == null)
                {
                    samlSettings = new SamlSettings
                    {
                        DefaultSamlIdpId = defaultSamlIdpId,
                        DefaultSamlIdpAccountId = defaultSamlIdpAccountId
                    };

                    _dataContext.SamlSettings.Add(samlSettings);
                }
                else
                {
                    samlSettings.DefaultSamlIdpId = defaultSamlIdpId;
                    samlSettings.DefaultSamlIdpAccountId = defaultSamlIdpAccountId;
                }

                _dataContext.SaveChanges();
            }
        }

        public SamlSettings GetSamlSettings()
        {
            lock (_sync)
            {
                return _dataContext.SamlSettings.FirstOrDefault();
            }
        }

        public bool StoreSamlServiceProvider(string entityId, string singleLogoutUrl)
        {
            lock (_sync)
            {
                bool found = true;
                SamlServiceProvider samlServiceProvider = _dataContext.SamlServiceProviders.FirstOrDefault(p => p.EntityId == entityId);

                if (samlServiceProvider == null)
                {
                    samlServiceProvider = new SamlServiceProvider
                    {
                        EntityId = entityId
                    };

                    _dataContext.SamlServiceProviders.Add(samlServiceProvider);
                    found = false;
                }

                samlServiceProvider.SingleLogoutUrl = singleLogoutUrl;

                _dataContext.SaveChanges();

                return found;
            }
        }

        public SamlServiceProvider GetSamlServiceProvider(string entityId)
        {
            lock (_sync)
            {
                return _dataContext.SamlServiceProviders.Local.FirstOrDefault(p => p.EntityId == entityId)
                    ?? _dataContext.SamlServiceProviders.FirstOrDefault(p => p.EntityId == entityId);
            }
        }

        public void OverrideUserAccount(long accountId, byte[] secretSpendKeyEnc, byte[] secretViewKeyEnc, byte[] publicSpendKey, byte[] publicViewKey, ulong lastAggregatedRegistrations)
        {
            lock (_sync)
            {
                Account account = _dataContext.Accounts.FirstOrDefault(a => a.AccountId == accountId);
                account.SecretSpendKey = secretSpendKeyEnc;
                account.SecretViewKey = secretViewKeyEnc;
                account.PublicSpendKey = publicSpendKey;
                account.PublicViewKey = publicViewKey;
                account.IsCompromised = false;
                account.LastAggregatedRegistrations = lastAggregatedRegistrations;

                SynchronizationStatus synchronizationStatus = _dataContext.SynchronizationStatuses.Include(s => s.Account).FirstOrDefault(s => s.Account.AccountId == accountId);
                synchronizationStatus.LastUpdatedCombinedBlockHeight = lastAggregatedRegistrations;

                UserSettings userSettings = GetLocalAwareUserSettings(accountId);

                if (userSettings == null)
                {
                    if (account != null)
                    {
                        userSettings = new UserSettings
                        {
                            Account = account,
                            IsAutoTheftProtection = false
                        };

                        _dataContext.UserSettings.Add(userSettings);
                    }
                }
                else
                {
                    userSettings.IsAutoTheftProtection = false;
                }

                _dataContext.SaveChanges();
            }

            RemoveAllWitnessed(accountId);
        }

        #region Identity Schemes

        public long AddAttributeToScheme(string issuer, string attributeName, string attributeSchemeName, string alias, string description)
        {
            lock (_sync)
            {
                IdentitiesScheme identitiesScheme =
                    _dataContext.IdentitiesSchemes.Local.FirstOrDefault(i => i.Issuer == issuer && i.AttributeName == attributeName) ??
                    _dataContext.IdentitiesSchemes.FirstOrDefault(i => i.Issuer == issuer && i.AttributeName == attributeName);

                if (identitiesScheme == null)
                {
                    identitiesScheme = new IdentitiesScheme
                    {
                        AttributeName = attributeName,
                        Issuer = issuer
                    };

                    _dataContext.IdentitiesSchemes.Add(identitiesScheme);
                }

                identitiesScheme.AttributeSchemeName = attributeSchemeName;
                identitiesScheme.Alias = alias;
                identitiesScheme.Description = description;
                identitiesScheme.IsActive = true;

                _dataContext.SaveChanges();

                return identitiesScheme.IdentitiesSchemeId;
            }
        }

        public IEnumerable<IdentitiesScheme> GetAttributesSchemeByIssuer(string issuer, bool activeOnly = false)
        {
            lock (_sync)
            {
                if (activeOnly)
                {
                    return _dataContext.IdentitiesSchemes.Where(p => p.Issuer == issuer && p.IsActive).ToList();
                }

                return _dataContext.IdentitiesSchemes.Where(p => p.Issuer == issuer).ToList();
            }
        }

        public void DeactivateAttribute(long identitiesSchemeId)
        {
            lock (_sync)
            {
                IdentitiesScheme identitiesScheme =
                    _dataContext.IdentitiesSchemes.Local.FirstOrDefault(i => i.IdentitiesSchemeId == identitiesSchemeId) ??
                    _dataContext.IdentitiesSchemes.FirstOrDefault(i => i.IdentitiesSchemeId == identitiesSchemeId);

                if (identitiesScheme != null)
                {
                    identitiesScheme.IsActive = false;
                    identitiesScheme.CanBeRoot = false;

                    _dataContext.SaveChanges();
                }
            }
        }

        public void ActivateAttribute(long identitiesSchemeId)
        {
            lock (_sync)
            {
                IdentitiesScheme identitiesScheme =
                    _dataContext.IdentitiesSchemes.Local.FirstOrDefault(i => i.IdentitiesSchemeId == identitiesSchemeId) ??
                    _dataContext.IdentitiesSchemes.FirstOrDefault(i => i.IdentitiesSchemeId == identitiesSchemeId);

                if (identitiesScheme != null)
                {
                    identitiesScheme.IsActive = true;

                    _dataContext.SaveChanges();
                }
            }
        }

        public IdentitiesScheme GetRootIdentityScheme(string issuer)
        {
            lock (_sync)
            {
                IdentitiesScheme identitiesScheme =
                    _dataContext.IdentitiesSchemes.Local.FirstOrDefault(i => i.CanBeRoot && i.Issuer == issuer) ??
                    _dataContext.IdentitiesSchemes.FirstOrDefault(i => i.CanBeRoot && i.Issuer == issuer);

                return identitiesScheme;
            }
        }
        public void ToggleOnRootAttributeScheme(long identitiesSchemeId)
        {
            lock (_sync)
            {
                IdentitiesScheme identitiesScheme =
                    _dataContext.IdentitiesSchemes.Local.FirstOrDefault(i => i.IdentitiesSchemeId == identitiesSchemeId && i.IsActive && !i.CanBeRoot) ??
                    _dataContext.IdentitiesSchemes.FirstOrDefault(i => i.IdentitiesSchemeId == identitiesSchemeId && i.IsActive && !i.CanBeRoot);

                if (identitiesScheme != null)
                {
                    IEnumerable<IdentitiesScheme> identitiesSchemes = _dataContext.IdentitiesSchemes.Where(i => i.Issuer == identitiesScheme.Issuer && i.IdentitiesSchemeId != identitiesSchemeId && i.CanBeRoot);
                    foreach (var item in identitiesSchemes)
                    {
                        item.CanBeRoot = false;
                    }

                    identitiesScheme.CanBeRoot = true;

                    if (!identitiesSchemes.Any(i => i.AttributeSchemeName == AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD))
                    {
                        _dataContext.IdentitiesSchemes.Add(new IdentitiesScheme
                        {
                            AttributeName = AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD,
                            AttributeSchemeName = AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD,
                            Alias = AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD,
                            CanBeRoot = false,
                            IsActive = true,
                            Issuer = identitiesScheme.Issuer
                        });
                    }

                    _dataContext.SaveChanges();
                }
            }
        }

        public void ToggleOffRootAttributeSchemes(string issuer)
        {
            lock (_sync)
            {
                List<IdentitiesScheme> identitiesSchemes =
                    _dataContext.IdentitiesSchemes.Where(i => i.Issuer == issuer && i.IsActive && i.CanBeRoot).ToList();

                foreach (var identitiesScheme in identitiesSchemes)
                {
                    identitiesScheme.CanBeRoot = false;
                }

                if (identitiesSchemes.Any())
                {
                    _dataContext.SaveChanges();
                }
            }
        }

        public long AddGroupRelation(string groupOwnerKey, string groupName, string assetId, string issuer)
        {
            lock (_sync)
            {
                if (_dataContext.GroupRelations.Local.Any(u => u.GroupOwnerKey == groupOwnerKey && u.GroupName == groupName && u.Issuer == issuer && u.AssetId == assetId) ||
                    _dataContext.GroupRelations.Any(u => u.GroupOwnerKey == groupOwnerKey && u.GroupName == groupName && u.Issuer == issuer && u.AssetId == assetId))
                {
                    return 0;
                }

                GroupRelation groupRelation = new GroupRelation
                {
                    GroupOwnerKey = groupOwnerKey,
                    GroupName = groupName,
                    AssetId = assetId,
                    Issuer = issuer
                };

                _dataContext.GroupRelations.Add(groupRelation);
                _dataContext.SaveChanges();

                return groupRelation.GroupRelationId;
            }
        }

        public IEnumerable<GroupRelation> GetGroupRelations(string assetId, string issuer)
        {
            lock (_sync)
            {
                return _dataContext.GroupRelations.Where(g => g.Issuer == issuer && g.AssetId == assetId).ToList();
            }
        }

        public long AddRegistrationCommitment(string commitment, string description, string assetId, string issuer)
        {
            lock (_sync)
            {
                if (_dataContext.RegistrationCommitments.Local.Any(u => u.Commitment == commitment && u.ServiceProviderInfo == description && u.Issuer == issuer && u.AssetId == assetId) ||
                    _dataContext.RegistrationCommitments.Any(u => u.Commitment == commitment && u.ServiceProviderInfo == description && u.Issuer == issuer && u.AssetId == assetId))
                {
                    return 0;
                }

                RegistrationCommitment registrationCommitment = new RegistrationCommitment
                {
                    Commitment = commitment,
                    ServiceProviderInfo = description,
                    AssetId = assetId,
                    Issuer = issuer
                };

                _dataContext.RegistrationCommitments.Add(registrationCommitment);
                _dataContext.SaveChanges();

                return registrationCommitment.RegistrationCommitmentId;

            }
        }
        public IEnumerable<RegistrationCommitment> GetRegistrationCommitments(string assetId, string issuer)
        {
            lock (_sync)
            {
                return _dataContext.RegistrationCommitments.Where(g => g.Issuer == issuer && g.AssetId == assetId).ToList();
            }
        }


        #endregion Identity Schemes

        #region ConsentManagementSettings

        public ConsentManagementSettings GetConsentManagementSettings()
        {
            lock (_sync)
            {
                return _dataContext.ConsentManagementSettings.FirstOrDefault();
            }
        }

        public void SetConsentManagementSettings(ConsentManagementSettings consentManagementSettings)
        {
            lock (_sync)
            {
                ConsentManagementSettings settings = _dataContext.ConsentManagementSettings.FirstOrDefault();
                if (settings == null)
                {
                    settings = new ConsentManagementSettings();

                    _dataContext.ConsentManagementSettings.Add(settings);
                }

                settings.AccountId = consentManagementSettings.AccountId;

                _dataContext.SaveChanges();
            }
        }

        #endregion ConsentManagementSettings

        public bool CheckAttributeSchemeToCommitmentMatching(string schemeName, byte[] commitment)
        {
            lock (_sync)
            {
                return _dataContext.Attributes.AsEnumerable().Any(a => a.AttributeName == schemeName && a.Commitment != null && a.Commitment.Equals32(commitment));
            }
        }
        #region Scenarios

        public IEnumerable<ScenarioSession> GetScenarioSessions(string userSubject)
        {
            lock (_sync)
            {
                return _dataContext.ScenarioSessions.Where(s => s.UserSubject == userSubject).ToList();
            }
        }

        public long AddNewScenarionSession(string userSubject, int scenarioId)
        {
            lock (_sync)
            {
                ScenarioSession scenarioSession = new ScenarioSession
                {
                    UserSubject = userSubject,
                    ScenarioId = scenarioId,
                    StartTime = DateTime.UtcNow,
                    CurrentStep = 0
                };

                _dataContext.ScenarioSessions.Add(scenarioSession);

                _dataContext.SaveChanges();

                return scenarioSession.ScenarioSessionId;
            }
        }

        public void UpdateScenarioSessionStep(long scenarionSessionId, int step)
        {
            lock (_sync)
            {
                ScenarioSession scenarioSession = _dataContext.ScenarioSessions.FirstOrDefault(s => s.ScenarioSessionId == scenarionSessionId);

                if (scenarioSession != null)
                {
                    scenarioSession.CurrentStep = step;

                    _dataContext.SaveChanges();
                }
            }
        }

        public void RemoveScenarioSession(string userSubject, int scenarioId)
        {
            lock (_sync)
            {
                List<ScenarioAccount> scenarioAccounts = _dataContext.ScenarioAccounts.Include(s => s.ScenarioSession).Where(s => s.ScenarioSession.UserSubject == userSubject && s.ScenarioSession.ScenarioId == scenarioId).ToList();

                foreach (var scenarioAccount in scenarioAccounts)
                {
                    RemoveAccount(scenarioAccount.AccountId);

                    _dataContext.ScenarioAccounts.Remove(scenarioAccount);
                }

                List<ScenarioSession> scenarioSessions = _dataContext.ScenarioSessions.Where(s => s.UserSubject == userSubject && s.ScenarioId == scenarioId).ToList();

                foreach (var scenarioSession in scenarioSessions)
                {
                    _dataContext.ScenarioSessions.Remove(scenarioSession);
                }

                _dataContext.SaveChanges();
            }
        }

        public ScenarioSession GetScenarioSession(long scenarioSessionId)
        {
            lock (_sync)
            {
                return _dataContext.ScenarioSessions.FirstOrDefault(s => s.ScenarioSessionId == scenarioSessionId);
            }
        }

        public long AddScenarionSessionAccount(long scenarioSessionId, long accountId)
        {
            lock (_sync)
            {
                ScenarioSession scenarioSession = _dataContext.ScenarioSessions.FirstOrDefault(s => s.ScenarioSessionId == scenarioSessionId);

                if (scenarioSession != null)
                {
                    ScenarioAccount scenarioAccount = new ScenarioAccount
                    {
                        ScenarioSession = scenarioSession,
                        AccountId = accountId
                    };

                    _dataContext.ScenarioAccounts.Add(scenarioAccount);

                    _dataContext.SaveChanges();

                    return scenarioAccount.ScenarioAccountId;
                }

                return 0;
            }
        }

        public IEnumerable<ScenarioAccount> GetScenarioAccounts(long scenarioSessionId)
        {
            lock (_sync)
            {
                return _dataContext.ScenarioAccounts.Include(a => a.ScenarioSession).Where(a => a.ScenarioSession.ScenarioSessionId == scenarioSessionId).ToList();
            }
        }

        public bool WitnessAsProcessed(long accountId, long witnessId)
        {
            lock (_sync)
            {
                if (_dataContext.ProcessedWitnesses.Local.Any(w => w.AccountId == accountId && w.WitnessId == witnessId) ||
                    _dataContext.ProcessedWitnesses.Any(w => w.AccountId == accountId && w.WitnessId == witnessId))
                {
                    return true;
                }

                ProcessedWitness processedWitness = new ProcessedWitness
                {
                    AccountId = accountId,
                    WitnessId = witnessId,
                    Time = DateTime.UtcNow
                };

                _dataContext.ProcessedWitnesses.Add(processedWitness);
                _dataContext.SaveChanges();

                return false;
            }
        }

        public void RemoveAllWitnessed(long accountId)
        {
            lock (_sync)
            {
                IEnumerable<ProcessedWitness> witnesses = _dataContext.ProcessedWitnesses.Where(w => w.AccountId == accountId).ToList();
                _dataContext.ProcessedWitnesses.RemoveRange(witnesses);

                _dataContext.SaveChanges();
            }
        }

        #endregion Scenarios

        #region Inherence

        public InherenceSetting GetInherenceSetting(string name)
        {
            lock (_sync)
            {
                return _dataContext.InherenceSettings.FirstOrDefault(s => s.Name == name);
            }
        }

        public InherenceSetting AddInherenceSetting(string name, long accountId)
        {
            InherenceSetting inherenceSetting = GetInherenceSetting(name);

            if (inherenceSetting == null)
            {
                lock (_sync)
                {
                    inherenceSetting = new InherenceSetting
                    {
                        Name = name,
                        AccountId = accountId
                    };

                    _dataContext.InherenceSettings.Add(inherenceSetting);

                    _dataContext.SaveChanges();
                }
            }

            return inherenceSetting;
        }

        public bool RemoveInherenceSetting(string name)
        {
            InherenceSetting inherenceSetting = GetInherenceSetting(name);

            if (inherenceSetting == null)
            {
                return false;
            }

            lock (_sync)
            {
                _dataContext.InherenceSettings.Remove(inherenceSetting);
                _dataContext.SaveChanges();
            }

            return true;
        }

        #endregion Inherence

        #region External IdPs

        public ExternalIdentityProvider GetExternalIdentityProvider(string name)
        {
            lock (_sync)
            {
                return _dataContext.ExternalIdentityProviders.FirstOrDefault(p => p.Name == name);
            }
        }

        public ExternalIdentityProvider AddExternalIdentityProvider(string name, string alias, string description, long accountId)
        {
            lock (_sync)
            {
                ExternalIdentityProvider externalIdentityProvider = new ExternalIdentityProvider
                {
                    Name = name,
                    Alias = alias,
                    Description = description,
                    AccountId = accountId
                };

                _dataContext.ExternalIdentityProviders.Add(externalIdentityProvider);

                _dataContext.SaveChanges();

                return externalIdentityProvider;
            }
        }

        #endregion External IdPs

        #region Election Committee

        public long AddPoll(string pollName, long accountId)
        {
            lock (_sync)
            {
                if (_dataContext.PollRecords.Any(p => p.Name == pollName))
                {
                    throw new ArgumentException($"Poll with the name {pollName} already registered");
                }

                EcPollRecord pollRecord = new EcPollRecord
                {
                    Name = pollName,
                    AccountId = accountId,
                    State = 0
                };

                _dataContext.PollRecords.Add(pollRecord);
                _dataContext.SaveChanges();

                return pollRecord.EcPollRecordId;
            }
        }
        public void SetPollState(long pollId, int state)
        {
            lock(_sync)
            {
                var poll = _dataContext.PollRecords.FirstOrDefault(p => p.EcPollRecordId == pollId);
                if(poll == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(pollId));
                }

                poll.State = state;

                _dataContext.SaveChanges();
            }
        }

        public EcPollRecord GetEcPoll(long pollId, bool includeCandidates = false, bool includeSelections = false)
        {
            lock(_sync)
            {
                EcPollRecord poll = null;

                if (includeCandidates && !includeSelections)
                {
                    poll = _dataContext.PollRecords.Include(p => p.Candidates).FirstOrDefault(p => p.EcPollRecordId == pollId);
                }

                if (!includeCandidates && includeSelections)
                {
                    poll = _dataContext.PollRecords.Include(p => p.PollSelections).FirstOrDefault(p => p.EcPollRecordId == pollId);
                }

                if (includeCandidates && includeSelections)
                {
                    poll = _dataContext.PollRecords.Include(p => p.Candidates).Include(p => p.PollSelections).FirstOrDefault(p => p.EcPollRecordId == pollId);
                }

                if (!includeCandidates && !includeSelections)
                {
                    poll = _dataContext.PollRecords.FirstOrDefault(p => p.EcPollRecordId == pollId);
                }

                if (poll == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(pollId));
                }

                return poll;
            }
        }

        public List<EcPollRecord> GetEcPolls(int pollState)
        {
            lock (_sync)
            {
                return _dataContext.PollRecords.Where(p => p.State == pollState).ToList();
            }
        }

        public List<EcPollRecord> GetEcPolls()
        {
            lock (_sync)
            {
                return _dataContext.PollRecords.ToList();
            }
        }

        public long AddCandidateToPoll(long pollId, string candidateName, string assetId)
        {
            lock(_sync)
            {
                var candidate = _dataContext.CandidateRecords.Include(c => c.EcPollRecord).FirstOrDefault(c => c.EcPollRecord.EcPollRecordId == pollId && (c.Name == candidateName || c.AssetId == assetId));
                if(candidate != null)
                {
                    throw new ArgumentException($"A candidate with either the same {nameof(candidateName)} or {nameof(assetId)} already registered");
                }

                var poll = _dataContext.PollRecords.FirstOrDefault(p => p.EcPollRecordId == pollId);
                if(poll == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(pollId));
                }

                candidate = new EcCandidateRecord
                {
                    Name = candidateName,
                    AssetId = assetId,
                    EcPollRecord = poll,
                    IsActive = true
                };

                _dataContext.CandidateRecords.Add(candidate);
                _dataContext.SaveChanges();

                return candidate.EcCandidateRecordId;
            }
        }

        public void SetCandidateStatus(long candidateId, bool isActive)
        {
            lock(_sync)
            {
                var candidate = _dataContext.CandidateRecords.FirstOrDefault(c => c.EcCandidateRecordId == candidateId);
                if(candidate == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(candidateId));
                }

                candidate.IsActive = isActive;

                _dataContext.SaveChanges();
            }
        }

        public EcCandidateRecord GetCandidateRecord(long candidateId)
        {
            lock(_sync)
            {
                var candidate = _dataContext.CandidateRecords.FirstOrDefault(c => c.EcCandidateRecordId == candidateId);
                if (candidate == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(candidateId));
                }

                return candidate;
            }
        }

        public EcPollSelection AddPollSelection(long pollId, string ecCommitment, string ecBlindingFactor)
        {
            lock(_sync)
            {
                var poll = _dataContext.PollRecords.FirstOrDefault(p => p.EcPollRecordId == pollId);
                if (poll == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(pollId));
                }

                var entry = _dataContext.PollSelections.Include(p => p.EcPollRecord).FirstOrDefault(p => p.EcPollRecord.EcPollRecordId == pollId && (p.EcCommitment == ecCommitment || p.EcBlindingFactor == ecBlindingFactor));
                if(entry != null)
                {
                    throw new ArgumentException($"Invalid {nameof(ecCommitment)} and {nameof(ecBlindingFactor)}");
                }

                entry = new EcPollSelection
                {
                    EcCommitment = ecCommitment,
                    EcBlindingFactor = ecBlindingFactor,
                    EcPollRecord = poll
                };

                _dataContext.PollSelections.Add(entry);
                _dataContext.SaveChanges();

                return entry;
            }
        }

        public EcPollSelection GetPollSelection(long pollId, string ecCommitment)
        {
            lock(_sync)
            {
                var selection = _dataContext.PollSelections.FirstOrDefault(s => s.EcPollRecord.EcPollRecordId == pollId && s.EcCommitment == ecCommitment);

                if (selection == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(ecCommitment));
                }

                return selection;
            }
        }

        public EcPollSelection UpdatePollSelection(long pollId, string ecCommitment, string voterBlindingFactor)
        {
            lock(_sync)
            {
                var selection = _dataContext.PollSelections.FirstOrDefault(s => s.EcPollRecord.EcPollRecordId == pollId && s.EcCommitment == ecCommitment);

                if (selection == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(ecCommitment));
                }

                if(!string.IsNullOrEmpty(selection.VoterBlindingFactor))
                {
                    throw new ArgumentException($"{nameof(selection.VoterBlindingFactor)} already set for poll with is {pollId} and EC commitment {ecCommitment}");
                }

                selection.VoterBlindingFactor = voterBlindingFactor;

                _dataContext.SaveChanges();

                return selection;
            }
        }

        #endregion Election Committee
    }
}