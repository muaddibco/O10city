using System;
using System.Collections.Generic;
using O10.Client.Common.Entities;
using O10.Client.Common.Interfaces;
using O10.Core.Architecture;

using Flurl;
using Flurl.Http;
using System.Threading.Tasks;
using O10.Client.Common.Exceptions;
using O10.Core.Configuration;
using O10.Client.Common.Configuration;
using O10.Core.Logging;
using Newtonsoft.Json;

namespace O10.Client.Common.Services
{
    [RegisterDefaultImplementation(typeof(ISchemeResolverService), Lifetime = LifetimeManagement.Singleton)]
    public class SchemeResolverService : ISchemeResolverService
    {
        private readonly IRestApiConfiguration _restApiConfiguration;
        private readonly ILogger _logger;
        private readonly IRestClientService _restClientService;

        public SchemeResolverService(IRestClientService restClientService, IConfigurationService configurationService, ILoggerService loggerService)
        {
            _restApiConfiguration = configurationService.Get<IRestApiConfiguration>();
            _logger = loggerService.GetLogger(nameof(SchemeResolverService));
            _restClientService = restClientService;
        }

        public async Task<string> ResolveIssuer(string issuer)
        {
            _logger.Debug($"ResolveIssuer {issuer}");
            try
            {
                if (!Uri.IsWellFormedUriString(_restApiConfiguration.SchemaResolutionUri, UriKind.Absolute))
                {
                    throw new SchemeResolverServiceNotInitializedException();
                }

                string name = null;

                Url url = _restApiConfiguration.SchemaResolutionUri.AppendPathSegment("IdentityProviderName").AppendPathSegment(issuer);

                await _restClientService.Request(url)
                    .GetStringAsync()
                    .ContinueWith(t =>
                    {
                        if (t.IsCompletedSuccessfully)
                        {
                            name = t.Result;
                        }
                        else
                        {
                            _logger.Error($"Failed ResolveIssuer({issuer}) from {_restApiConfiguration.SchemaResolutionUri}", t.Exception);
                        }
                    }, TaskScheduler.Current)
                    .ConfigureAwait(false);

                return name;

            }
            catch (Exception ex)
            {
                _logger.Error($"ResolveIssuer {issuer}", ex);
                throw;
            }
        }


        public async Task<AttributeDefinition?> ResolveAttributeScheme(string issuer, long schemeId)
        {
            if (!Uri.IsWellFormedUriString(_restApiConfiguration.SchemaResolutionUri, UriKind.Absolute))
            {
                throw new SchemeResolverServiceNotInitializedException();
            }

            AttributeDefinition? attributeScheme = null;

            try
            {
                _logger.Debug($"ResolveAttributeScheme({issuer}, {schemeId})");

                Url url = _restApiConfiguration.SchemaResolutionUri
                    .AppendPathSegment("AttributeDefinition")
                    .SetQueryParam("issuer", issuer)
                    .SetQueryParam("schemeId", schemeId);

                await _restClientService.Request(url)
                    .GetJsonAsync<AttributeDefinition>()
                    .ContinueWith(t =>
                    {
                        if (t.IsCompletedSuccessfully)
                        {
                            attributeScheme = t.Result;
                        }
                        else
                        {
                            _logger.Error($"Failed ResolveAttributeScheme({issuer}, {schemeId}) from {_restApiConfiguration.SchemaResolutionUri}", t.Exception);
                        }
                    }, TaskScheduler.Current).ConfigureAwait(false);

                return attributeScheme;

            }
            catch (Exception ex)
            {
                _logger.Error($"ResolveAttributeScheme({issuer}, {schemeId})", ex);
                throw;
            }
        }

        public async Task<AttributeDefinition> ResolveAttributeScheme(string issuer, string schemeName)
        {
            if (!Uri.IsWellFormedUriString(_restApiConfiguration.SchemaResolutionUri, UriKind.Absolute))
            {
                throw new SchemeResolverServiceNotInitializedException();
            }

            AttributeDefinition attributeScheme = null;

            Url url = _restApiConfiguration.SchemaResolutionUri
                .AppendPathSegment("AttributeDefinition2")
                .SetQueryParam("issuer", issuer)
                .SetQueryParam("schemeName", schemeName);

            await _restClientService.Request(url)
                .GetJsonAsync<AttributeDefinition>()
                .ContinueWith(t =>
                {
                    if (t.IsCompleted && !t.IsFaulted)
                    {
                        attributeScheme = t.Result;
                    }
                }, TaskScheduler.Current).ConfigureAwait(false);

            return attributeScheme;
        }

        public async Task<IEnumerable<AttributeDefinition>> ResolveAttributeSchemes(string issuer, bool activeOnly = false)
        {
            if (!Uri.IsWellFormedUriString(_restApiConfiguration.SchemaResolutionUri, UriKind.Absolute))
            {
                throw new SchemeResolverServiceNotInitializedException();
            }

            List<AttributeDefinition> attributeSchemes = null;

            try
            {
                _logger.Debug($"{nameof(ResolveAttributeSchemes)}({issuer}, {activeOnly})");

                Url url = _restApiConfiguration.SchemaResolutionUri
                    .AppendPathSegment("AttributeDefinitions")
                    .SetQueryParam("issuer", issuer)
                    .SetQueryParam("activeOnly", activeOnly);

                await _restClientService.Request(url)
                    .GetJsonAsync<List<AttributeDefinition>>()
                    .ContinueWith(t =>
                    {
                        if (t.IsCompletedSuccessfully)
                        {
                            attributeSchemes = t.Result;
                        }
                        else
                        {
                            _logger.Error($"Failed {nameof(ResolveAttributeSchemes)}({issuer}, {activeOnly}) from {_restApiConfiguration.SchemaResolutionUri}", t.Exception);
                        }
                    }, TaskScheduler.Current).ConfigureAwait(false);

                return attributeSchemes;

            }
            catch (Exception ex)
            {
                _logger.Debug($"Failed {nameof(ResolveAttributeSchemes)}({issuer}, {activeOnly})", ex);

                throw;
            }
        }

        public async Task<AttributeDefinition> GetRootAttributeScheme(string issuer)
        {
            if (!Uri.IsWellFormedUriString(_restApiConfiguration.SchemaResolutionUri, UriKind.Absolute))
            {
                throw new SchemeResolverServiceNotInitializedException();
            }

            AttributeDefinition attributeScheme = null;

            try
            {
                _logger.Debug($"{nameof(GetRootAttributeScheme)}({issuer})");

                Url url = _restApiConfiguration.SchemaResolutionUri
                    .AppendPathSegment("RootAttributeDefinition")
                    .SetQueryParam("issuer", issuer);

                await _restClientService.Request(url)
                    .GetJsonAsync<AttributeDefinition>()
                    .ContinueWith(t =>
                    {
                        if (t.IsCompletedSuccessfully)
                        {
                            attributeScheme = t.Result;
                        }
                        else
                        {
                            _logger.Error($"Failed {nameof(GetRootAttributeScheme)}({issuer}) from {_restApiConfiguration.SchemaResolutionUri}", t.Exception);
                        }
                    }, TaskScheduler.Current).ConfigureAwait(false);

                return attributeScheme;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed {nameof(GetRootAttributeScheme)}({issuer})", ex);
                throw;
            }
        }

        public async Task StoreGroupRelation(string issuer, string assetId, string groupOwnerKey, string groupName)
        {
            try
            {
                _logger.Debug($"{nameof(StoreGroupRelation)}({issuer}, {assetId}, {groupOwnerKey}, {groupName})");

                Url url = _restApiConfiguration.SchemaResolutionUri.AppendPathSegment("GroupRelation");

                await _restClientService.Request(url)
                    .PostJsonAsync(new RegistrationKeyDescriptionStore { Issuer = issuer, AssetId = assetId, Key = groupOwnerKey, Description = groupName })
                    .ContinueWith(t =>
                    {
                        if (!t.IsCompletedSuccessfully)
                        {
                            _logger.Error($"Failed {nameof(StoreGroupRelation)}({issuer}, {assetId}, {groupOwnerKey}, {groupName}) from {_restApiConfiguration.SchemaResolutionUri}", t.Exception);
                        }
                    }, TaskScheduler.Current)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed {nameof(StoreGroupRelation)}({issuer}, {assetId}, {groupOwnerKey}, {groupName})", ex);
                throw;
            }
        }

        public async Task<IEnumerable<RegistrationKeyDescriptionStore>> GetGroupRelations(string issuer, string assetId)
        {
            IEnumerable<RegistrationKeyDescriptionStore> groupRelations = null;
            try
            {
                _logger.Debug($"{nameof(GetGroupRelations)}({issuer}, {assetId})");

                Url url = _restApiConfiguration.SchemaResolutionUri
                        .AppendPathSegment("GroupRelations")
                        .SetQueryParams(new { issuer, assetId });

                await _restClientService.Request(url)
                    .GetJsonAsync<IEnumerable<RegistrationKeyDescriptionStore>>()
                    .ContinueWith(t =>
                    {
                        if (t.IsCompletedSuccessfully)
                        {
                            groupRelations = t.Result;
                        }
                        else
                        {
                            _logger.Error($"Failed {nameof(GetGroupRelations)}({issuer}, {assetId}) from {_restApiConfiguration.SchemaResolutionUri}", t.Exception);
                        }
                    }, TaskScheduler.Current)
                    .ConfigureAwait(false);

                return groupRelations;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed {nameof(GetGroupRelations)}({issuer}, {assetId})", ex);
                throw;
            }
        }

        public async Task<bool> StoreRegistrationCommitment(string issuer, string assetId, string commtiment, string description)
        {
            bool res = false;
            _logger.Debug($"{nameof(StoreRegistrationCommitment)}({issuer}, {assetId}, {commtiment}, {description})");
            Url url = _restApiConfiguration.SchemaResolutionUri.AppendPathSegment("RegistrationCommitment");
            var body = new RegistrationKeyDescriptionStore { Issuer = issuer, AssetId = assetId, Key = commtiment, Description = description };

            try
            {
                await url
                    .PostJsonAsync(new RegistrationKeyDescriptionStore { Issuer = issuer, AssetId = assetId, Key = commtiment, Description = description })
                    .ConfigureAwait(false);

                res = true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Request failed to uri {url} with body {JsonConvert.SerializeObject(body)}", ex);
            }

            return res;
        }

        public async Task<IEnumerable<RegistrationKeyDescriptionStore>> GetRegistrationCommitments(string issuer, string assetId)
        {
            IEnumerable<RegistrationKeyDescriptionStore> result = null;
            try
            {
                _logger.Debug($"{nameof(GetRegistrationCommitments)}({issuer}, {assetId})");

                Url url = _restApiConfiguration.SchemaResolutionUri
                        .AppendPathSegment("RegistrationCommitments")
                        .SetQueryParams(new { issuer, assetId });

                await _restClientService.Request(url)
                        .GetJsonAsync<IEnumerable<RegistrationKeyDescriptionStore>>()
                        .ContinueWith(t =>
                        {
                            if (t.IsCompletedSuccessfully)
                            {
                                result = t.Result;
                            }
                            else
                            {
                                _logger.Error($"Failed {nameof(GetRegistrationCommitments)}({issuer}, {assetId}) from {_restApiConfiguration.SchemaResolutionUri}", t.Exception);
                            }
                        }, TaskScheduler.Current)
                        .ConfigureAwait(false);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed {nameof(GetRegistrationCommitments)}({issuer}, {assetId})", ex);
                throw;
            }
        }

        public async Task BackupAssociatedAttributes(string rootIsser, string rootAssetId, AssociatedAttributeBackupDTO[] associatedAttributeBackups)
        {
            try
            {
                _logger.Debug($"{nameof(BackupAssociatedAttributes)}({rootIsser}, {rootAssetId}), {nameof(associatedAttributeBackups)}: {JsonConvert.SerializeObject(associatedAttributeBackups)}");

                await _restApiConfiguration.SchemaResolutionUri
                    .AppendPathSegment("AssociatedAttributes")
                    .SetQueryParams(new { rootIsser, rootAssetId })
                    .PostJsonAsync(associatedAttributeBackups)
                    .ConfigureAwait(false);
            }
            catch (AggregateException ex)
            {
                _logger.Error($"Failed {nameof(BackupAssociatedAttributes)}({rootIsser}, {rootAssetId})", ex.InnerException);
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed {nameof(BackupAssociatedAttributes)}({rootIsser}, {rootAssetId})", ex);
                throw;
            }
        }

        public async Task<IEnumerable<AssociatedAttributeBackupDTO>> GetAssociatedAttributeBackups(string issuer, string assetId)
        {
            IEnumerable<AssociatedAttributeBackupDTO> result = null;

            try
            {
                _logger.Debug($"{nameof(GetAssociatedAttributeBackups)}({issuer}, {assetId})");

                Url url = _restApiConfiguration.SchemaResolutionUri
                        .AppendPathSegment("AssociatedAttributes")
                        .SetQueryParams(new { issuer, assetId });

                await _restClientService.Request(url)
                        .GetJsonAsync<IEnumerable<AssociatedAttributeBackupDTO>>()
                        .ContinueWith(t =>
                        {
                            if (t.IsCompletedSuccessfully)
                            {
                                result = t.Result;
                            }
                            else
                            {
                                _logger.Error($"Failed {nameof(GetAssociatedAttributeBackups)}({issuer}, {assetId}) from {_restApiConfiguration.SchemaResolutionUri}", t.Exception);
                            }
                        }, TaskScheduler.Current)
                        .ConfigureAwait(false);

                return result;
            }
            catch (AggregateException ex)
            {
                _logger.Error($"Failed {nameof(GetAssociatedAttributeBackups)}({issuer}, {assetId})", ex.InnerException);
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed {nameof(GetAssociatedAttributeBackups)}({issuer}, {assetId})", ex);
                throw;
            }
        }

    }
}
