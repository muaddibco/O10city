using Microsoft.AspNetCore.Mvc;
using O10.Core.ExtensionMethods;
using O10.Client.DataLayer.Services;
using O10.Client.DataLayer.Model;
using O10.Client.Common.Entities;
using System.Collections.Generic;
using System.Linq;
using O10.Client.DataLayer.AttributesScheme;
using O10.Client.DataLayer.Enums;
using Microsoft.AspNetCore.Authorization;
using O10.Core.Logging;
using O10.Client.Web.Common.Services;
using O10.Client.Common.Integration;
using O10.Client.Web.Portal.Dtos.SchemeResolution;
using System.Threading.Tasks;

namespace O10.Client.Web.Portal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchemeResolutionController : ControllerBase
    {
        private readonly IDataAccessService _dataAccessService;
        private readonly IAccountsServiceEx _accountsService;
        private readonly IIntegrationIdPRepository _integrationIdPRepository;
        private readonly ILogger _logger;

        public SchemeResolutionController(IDataAccessService dataAccessService,
                                          IAccountsServiceEx accountsService,
                                          IIntegrationIdPRepository integrationIdPRepository,
                                          ILoggerService loggerService)
        {
            _dataAccessService = dataAccessService;
            _accountsService = accountsService;
            _integrationIdPRepository = integrationIdPRepository;
            _logger = loggerService.GetLogger(nameof(SchemeResolutionController));
        }

        [HttpGet("IdentityProviderName/{key}")]
        [AllowAnonymous]
        public ActionResult<string> ResolveIdentityProviderName(string key)
        {
            Account account = _dataAccessService.GetAccount(key.HexStringToByteArray());

            return account?.AccountInfo;
        }

        [HttpGet("AttributeDefinitions")]
        [AllowAnonymous]
        public ActionResult<AttributeDefinition[]> GetAttributeDefinitions(string issuer, bool activeOnly)
        {
            return _dataAccessService.GetAttributesSchemeByIssuer(issuer, activeOnly)
                .Select(a => new AttributeDefinition
                {
                    SchemeId = a.IdentitiesSchemeId,
                    AttributeName = a.AttributeName,
                    SchemeName = a.AttributeSchemeName,
                    Alias = a.Alias,
                    Description = a.Description,
                    IsActive = a.IsActive,
                    IsRoot = a.CanBeRoot
                }).ToArray();
        }

        [HttpGet("AttributeDefinition")]
        [AllowAnonymous]
        public ActionResult<AttributeDefinition> GetAttributeDefinition(string issuer, long schemeId)
        {
            AttributeDefinition attributeDefinition = null;
            IdentitiesScheme identitiesScheme = _dataAccessService.GetAttributesSchemeByIssuer(issuer).FirstOrDefault(a => a.IdentitiesSchemeId == schemeId);

            if (identitiesScheme != null)
            {
                attributeDefinition = new AttributeDefinition
                {
                    SchemeId = identitiesScheme.IdentitiesSchemeId,
                    AttributeName = identitiesScheme.AttributeName,
                    Alias = identitiesScheme.Alias,
                    SchemeName = identitiesScheme.AttributeSchemeName,
                    Description = identitiesScheme.Description,
                    IsActive = identitiesScheme.IsActive,
                    IsRoot = identitiesScheme.CanBeRoot
                };
            }

            return attributeDefinition;
        }

        [HttpGet("AttributeDefinition2")]
        [AllowAnonymous]
        public ActionResult<AttributeDefinition> GetAttributeDefinition(string issuer, string schemeName)
        {
            AttributeDefinition attributeDefinition = null;
            IdentitiesScheme identitiesScheme = _dataAccessService.GetAttributesSchemeByIssuer(issuer).FirstOrDefault(a => a.AttributeSchemeName == schemeName);

            if (identitiesScheme != null)
            {
                attributeDefinition = new AttributeDefinition
                {
                    SchemeId = identitiesScheme.IdentitiesSchemeId,
                    AttributeName = identitiesScheme.AttributeName,
                    Alias = identitiesScheme.Alias,
                    SchemeName = identitiesScheme.AttributeSchemeName,
                    Description = identitiesScheme.Description,
                    IsActive = identitiesScheme.IsActive,
                    IsRoot = identitiesScheme.CanBeRoot
                };
            }

            return attributeDefinition;
        }

        [HttpGet("RootAttributeDefinition")]
        [AllowAnonymous]
        public ActionResult<AttributeDefinition> GetRootAttributeDefinition(string issuer)
        {
            try
            {
                _logger.Debug($"{nameof(GetRootAttributeDefinition)}({issuer})");
                AttributeDefinition attributeDefinition = null;
                IdentitiesScheme identitiesScheme = _dataAccessService.GetRootIdentityScheme(issuer);

                if (identitiesScheme != null)
                {
                    attributeDefinition = new AttributeDefinition
                    {
                        SchemeId = identitiesScheme.IdentitiesSchemeId,
                        AttributeName = identitiesScheme.AttributeName,
                        Alias = identitiesScheme.Alias,
                        SchemeName = identitiesScheme.AttributeSchemeName,
                        Description = identitiesScheme.Description,
                        IsActive = identitiesScheme.IsActive,
                        IsRoot = identitiesScheme.CanBeRoot
                    };
                }

                return attributeDefinition;

            }
            catch (System.Exception ex)
            {
                _logger.Error($" Failed {nameof(GetRootAttributeDefinition)}({issuer})", ex);
                throw;
            }
        }

        [HttpPut("AttributeDefinitions")]
        [AllowAnonymous]
        public async Task<ActionResult<AttributeDefinitionsResponse>> SetAttributeDefinitions(string issuer, [FromBody] AttributeDefinition[] attributeDefinitions)
        {
            IEnumerable<IdentitiesScheme> identitiesSchemes = _dataAccessService.GetAttributesSchemeByIssuer(issuer).Where(a => a.AttributeSchemeName != AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD);

            List<AttributeDefinition> newAttributeDefinitions = attributeDefinitions.Where(a => !identitiesSchemes.Any(i => i.AttributeSchemeName == a.SchemeName)).ToList();

            newAttributeDefinitions.ForEach(a =>
            {
                a.SchemeId = _dataAccessService.AddAttributeToScheme(issuer, a.AttributeName, a.SchemeName, a.Alias, a.Description);
            });

            identitiesSchemes.Where(i => i.IsActive && attributeDefinitions.All(a => a.AttributeName != i.AttributeName)).ToList().ForEach(a =>
            {
                _dataAccessService.DeactivateAttribute(a.IdentitiesSchemeId);
            });

            identitiesSchemes.Where(i => !i.IsActive && attributeDefinitions.Any(a => a.AttributeName == i.AttributeName)).ToList().ForEach(a =>
            {
                _dataAccessService.ActivateAttribute(a.IdentitiesSchemeId);
            });

            AttributeDefinition rootAttributeDefinition = attributeDefinitions.FirstOrDefault(a => a.IsRoot);

            if (rootAttributeDefinition != null)
            {
                _dataAccessService.ToggleOnRootAttributeScheme(rootAttributeDefinition.SchemeId);
            }
            else
            {
                _dataAccessService.ToggleOffRootAttributeSchemes(issuer);
            }

            var accountDescriptor = _accountsService.GetByPublicKey(issuer.HexStringToByteArray());

            ActionStatus actionStatus = null;
            string integrationKey = _dataAccessService.GetAccountKeyValue(accountDescriptor.AccountId, _integrationIdPRepository.IntegrationKeyName);
            var integrationService = _integrationIdPRepository.GetInstance(integrationKey);
            if (integrationService != null)
            {
                var definitions = _dataAccessService.GetAttributesSchemeByIssuer(issuer, true)
                    .Select(
                        a => new AttributeDefinition
                        {
                            SchemeId = a.IdentitiesSchemeId,
                            AttributeName = a.AttributeName,
                            SchemeName = a.AttributeSchemeName,
                            Alias = a.Alias,
                            Description = a.Description,
                            IsActive = a.IsActive,
                            IsRoot = a.CanBeRoot
                        }).ToArray();
                actionStatus = await integrationService.StoreScheme(accountDescriptor.AccountId, definitions).ConfigureAwait(false);
            }

            AttributeDefinitionsResponse response = new AttributeDefinitionsResponse
            {
                IntegrationActionStatus = actionStatus,
                AttributeDefinitions = _dataAccessService.GetAttributesSchemeByIssuer(issuer, true)
                .Where(a => a.AttributeSchemeName != AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD)
                .Select(a => new AttributeDefinition
                {
                    SchemeId = a.IdentitiesSchemeId,
                    AttributeName = a.AttributeName,
                    SchemeName = a.AttributeSchemeName,
                    Alias = a.Alias,
                    Description = a.Description,
                    IsActive = a.IsActive,
                    IsRoot = a.CanBeRoot
                }).ToArray()
            };

            return response;
        }

        [HttpGet("SchemeItems")]
        [AllowAnonymous]
        public ActionResult<SchemeItem[]> GetAllSchemeItems()
        {
            return AttributesSchemes.AttributeSchemes.Select(a => new SchemeItem
            {
                Name = a.Name,
                Description = a.Description,
                ValueType = a.ValueType.ToString(),
                AllowMultiple = a.IsMultiple
            }).ToArray();
        }

        [HttpGet("ServiceProviderRelationGroups")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<ServiceProviderRelationGroups>> GetServiceProviderRelationGroups()
        {
            List<ServiceProviderRelationGroups> serviceProviderRelationGroupsList = new List<ServiceProviderRelationGroups>();
            List<Account> accounts = _dataAccessService.GetAccountsByType(AccountType.ServiceProvider);
            foreach (var account in accounts)
            {
                RelationGroup[] relationGroupNames = _dataAccessService.GetServiceProviderRelationGroups(account.AccountId).Select(g => new RelationGroup { Name = g }).ToArray();

                if (relationGroupNames != null && relationGroupNames.Length > 0)
                {
                    ServiceProviderRelationGroups serviceProviderRelationGroups = new ServiceProviderRelationGroups
                    {
                        Alias = account.AccountInfo,
                        PublicSpendKey = account.PublicSpendKey.ToHexString(),
                        PublicViewKey = account.PublicViewKey?.ToHexString(),
                        RelationGroups = relationGroupNames
                    };

                    serviceProviderRelationGroupsList.Add(serviceProviderRelationGroups);
                }
            }

            return serviceProviderRelationGroupsList;
        }

        [HttpPost("GroupRelation")]
        [AllowAnonymous]
        public IActionResult AddGroupRelation([FromBody] RegistrationKeyDescriptionStore content)
        {
            _dataAccessService.AddGroupRelation(content.Key, content.Description, content.AssetId, content.Issuer);

            return Ok();
        }

        [HttpGet("GroupRelations")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<RegistrationKeyDescriptionStore>> GetGroupRelations(string issuer, string assetId)
        {
            return Ok(_dataAccessService.GetGroupRelations(assetId, issuer)
                .Select(r => new RegistrationKeyDescriptionStore
                {
                    AssetId = r.AssetId,
                    Issuer = r.Issuer,
                    Key = r.GroupOwnerKey,
                    Description = r.GroupName
                }));
        }

        [HttpPost("RegistrationCommitment")]
        [AllowAnonymous]
        public IActionResult AddRegistrationCommitment([FromBody] RegistrationKeyDescriptionStore content)
        {
            _dataAccessService.AddRegistrationCommitment(content.Key, content.Description, content.AssetId, content.Issuer);

            return Ok();
        }

        [HttpGet("RegistrationCommitments")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<RegistrationKeyDescriptionStore>> GetRegistrationCommitments(string issuer, string assetId)
        {
            return Ok(_dataAccessService.GetRegistrationCommitments(assetId, issuer)
                .Select(r => new RegistrationKeyDescriptionStore
                {
                    AssetId = r.AssetId,
                    Issuer = r.Issuer,
                    Key = r.Commitment,
                    Description = r.ServiceProviderInfo
                }));
        }

        [HttpPost("AssociatedAttributes")]
        public IActionResult StoreAssociatedAttributes(string rootIssuer, string rootAssetId, [FromBody] List<AssociatedAttributeBackupDTO> associatedAttributes)
        {
            _dataAccessService.StoreAssociatedAttributes(rootIssuer, rootAssetId, associatedAttributes.Select(a => new AssociatedAttributeBackup { AssociatedIssuer = a.AssociatedIssuer, SchemeName = a.SchemeName, Content = a.Content }));

            return Ok();
        }

        [HttpGet("AssociatedAttributes")]
        public ActionResult<IEnumerable<AssociatedAttributeBackupDTO>> GetAssociatedAttributes(string rootIssuer, string rootAssetId)
        {
            IEnumerable<AssociatedAttributeBackupDTO> associatedAttributes =
                _dataAccessService.GetAssociatedAttributeBackups(rootIssuer, rootAssetId)?
                .Select(a => new AssociatedAttributeBackupDTO
                {
                    AssociatedIssuer = a.AssociatedIssuer,
                    SchemeName = a.SchemeName,
                    Content = a.Content
                });

            return Ok(associatedAttributes);
        }
    }
}
