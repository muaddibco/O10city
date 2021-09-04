using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using O10.Client.Web.Portal.Services;
using O10.Core.ExtensionMethods;
using O10.Client.DataLayer.Services;
using O10.Client.DataLayer.Model;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Enums;
using System.Globalization;
using O10.Client.Web.DataContracts.ServiceProvider;
using O10.Core.HashCalculations;
using O10.Core;
using System.Threading.Tasks;
using O10.Client.DataLayer.AttributesScheme;
using O10.Client.DataLayer.Model.Scenarios;
using O10.Client.Common.Configuration;
using O10.Core.Configuration;
using O10.Core.Logging;
using Flurl;
using Flurl.Http;
using O10.Client.Common.Interfaces.Inputs;
using Newtonsoft.Json;
using O10.Client.Common.Entities;
using O10.Core.Identity;
using O10.Crypto.ConfidentialAssets;
using System.IO;

namespace O10.Client.Web.Portal.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceProvidersController : ControllerBase
    {
        private readonly IAccountsService _accountsService;
        private readonly IExecutionContextManager _executionContextManager;
        private readonly IDataAccessService _dataAccessService;
        private readonly IIdentityAttributesService _identityAttributesService;
        private readonly IConsentManagementService _consentManagementService;
        private readonly IAssetsService _assetsService;
        private readonly IHashCalculation _hashCalculation;
        private readonly IRestApiConfiguration _restApiConfiguration;
        private readonly IIdentityKeyProvider _identityKeyProvider;
        private readonly ILogger _logger;

        public ServiceProvidersController(IAccountsService accountsService,
                                          IExecutionContextManager executionContextManager,
                                          IDataAccessService dataAccessService,
                                          IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
                                          IIdentityAttributesService identityAttributesService,
                                          IConsentManagementService consentManagementService,
                                          IHashCalculationsRepository hashCalculationsRepository,
                                          IAssetsService assetsService,
                                          IConfigurationService configurationService,
                                          ILoggerService loggerService)
        {
            _accountsService = accountsService;
            _executionContextManager = executionContextManager;
            _dataAccessService = dataAccessService;
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
            _identityAttributesService = identityAttributesService;
            _consentManagementService = consentManagementService;
            _assetsService = assetsService;
            _hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
            _restApiConfiguration = configurationService.Get<IRestApiConfiguration>();
            _logger = loggerService.GetLogger(nameof(ServiceProvidersController));
        }

        [HttpGet("Registrations")]
        public IActionResult GetRegistrations(long accountId)
        {
            IEnumerable<ServiceProviderRegistration> registrations = _dataAccessService.GetServiceProviderRegistrations(accountId);

            return Ok(registrations.Select(r =>
            new ServiceProviderRegistrationDto
            {
                RegistrationId = r.ServiceProviderRegistrationId,
                Commitment = r.Commitment.ToHexString()
            }));
        }

        [HttpGet("GetAll")]
        public IActionResult GetAll(long scenarioId = 0)
        {
            ScenarioSession scenarioSession = scenarioId > 0 ? _dataAccessService.GetScenarioSessions(User.Identity.Name).FirstOrDefault(s => s.ScenarioId == scenarioId) : null;
            if (scenarioSession != null)
            {
                IEnumerable<ScenarioAccount> scenarioAccounts = _dataAccessService.GetScenarioAccounts(scenarioSession.ScenarioSessionId);
                var serviceProviders = _accountsService.GetAll().Where(a => a.AccountType == AccountTypeDTO.ServiceProvider && scenarioAccounts.Any(sa => sa.AccountId == a.AccountId)).Select(a => new ServiceProviderInfoDto
                {
                    Id = a.AccountId.ToString(CultureInfo.InvariantCulture),
                    Description = a.AccountInfo,
                    Target = a.PublicSpendKey.ToHexString()
                });

                return Ok(serviceProviders);
            }
            else
            {
                var serviceProviders = _accountsService.GetAll().Where(a => !a.IsPrivate && a.AccountType == AccountTypeDTO.ServiceProvider).Select(a => new ServiceProviderInfoDto
                {
                    Id = a.AccountId.ToString(CultureInfo.InvariantCulture),
                    Description = a.AccountInfo,
                    Target = a.PublicSpendKey.ToHexString()
                });

                return Ok(serviceProviders);
            }
        }

        [HttpGet("ById/{id}")]
        public IActionResult GetById(long id)
        {
            var serviceProvider = _accountsService.GetAll().FirstOrDefault(a => a.AccountId == id);

            return Ok(new ServiceProviderInfoDto { Id = id.ToString(CultureInfo.InvariantCulture), Description = serviceProvider.AccountInfo, Target = serviceProvider.PublicSpendKey.ToHexString() });
        }

        [HttpGet("IdentityAttributeValidationDescriptors")]
        public async Task<IActionResult> GetIdentityAttributeValidationDescriptors(long accountId)
        {
            AccountDescriptor account = _accountsService.GetById(accountId);

            return Ok(await _identityAttributesService.GetIdentityAttributeValidationDescriptors(account.PublicSpendKey.ToHexString(), true).ConfigureAwait(false));
        }

        [AllowAnonymous]
        [HttpGet("IdentityAttributeValidations")]
        public IActionResult GetIdentityAttributeValidations(long accountId)
        {
            IEnumerable<SpIdenitityValidation> spIdenitityValidations = _dataAccessService.GetSpIdenitityValidations(accountId);

            return Ok(spIdenitityValidations.Select(s =>
            {
                return s.SchemeName switch
                {
                    AttributesSchemes.ATTR_SCHEME_NAME_PLACEOFBIRTH => new IdentityAttributeValidationDefinitionDto
                    {
                        CriterionValue = s.GroupIdCriterion?.ToHexString(),
                        SchemeName = s.SchemeName,
                        ValidationType = ((ushort)s.ValidationType).ToString(CultureInfo.InvariantCulture)
                    },
                    AttributesSchemes.ATTR_SCHEME_NAME_DATEOFBIRTH => new IdentityAttributeValidationDefinitionDto
                    {
                        CriterionValue = s.NumericCriterion.Value.ToString(CultureInfo.InvariantCulture),
                        SchemeName = s.SchemeName,
                        ValidationType = ((ushort)s.ValidationType).ToString(CultureInfo.InvariantCulture)
                    },
                    _ => new IdentityAttributeValidationDefinitionDto
                    {
                        SchemeName = s.SchemeName,
                        ValidationType = ((ushort)s.ValidationType).ToString(CultureInfo.InvariantCulture)
                    },
                };
            }));
        }

        [HttpPut("IdentityAttributeValidationDefinitions")]
        public IActionResult SetIdentityAttributeValidationDefinitions(long accountId, [FromBody] IdentityAttributeValidationDefinitionsDto identityAttributeValidationDefinitions)
        {
            List<SpIdenitityValidation> spIdenitityValidations =
                identityAttributeValidationDefinitions.IdentityAttributeValidationDefinitions
                .Select(i =>
                    new SpIdenitityValidation
                    {
                        AccountId = accountId,
                        SchemeName = i.SchemeName,
                        ValidationType = (ValidationType)ushort.Parse(i.ValidationType, CultureInfo.InvariantCulture),
                        NumericCriterion = (!string.IsNullOrEmpty(i.CriterionValue)) ? ushort.Parse(i.CriterionValue, CultureInfo.InvariantCulture) : new ushort?(),
                        GroupIdCriterion = i.CriterionValue?.HexStringToByteArray()
                    }).ToList();

            _dataAccessService.AdjustSpIdenitityValidations(accountId, spIdenitityValidations);

            return Ok();
        }

        #region Relations

        [HttpGet("{accountId}/RelationGroups")]
        public IActionResult GetRelationGroups(long accountId)
        {
            List<RelationGroupDto> employeeGroups = new List<RelationGroupDto>();

            employeeGroups = _dataAccessService.GetRelationGroups(accountId).Select(g => new RelationGroupDto { GroupId = g.RelationGroupId, GroupName = g.GroupName }).ToList();

            return Ok(employeeGroups);
        }

        [HttpPost("{accountId}/RelationGroup")]
        public IActionResult AddRelationGroup(long accountId, [FromBody] RelationGroupDto relationGroup)
        {
            relationGroup.GroupId = _dataAccessService.AddRelationGroup(accountId, relationGroup.GroupName);

            return Ok(relationGroup);
        }

        [HttpDelete("{accountId}/RelationGroup/{groupId}")]
        public IActionResult DeleteRelationGroup(long accountId, long groupId)
        {
            _dataAccessService.RemoveRelationGroup(accountId, groupId);

            return Ok();
        }

        [HttpGet("{accountId}/Relations")]
        public IActionResult GetRelations(long accountId)
        {
            return Ok(_dataAccessService.GetRelations(accountId).Select(e => new RelationDto
            {
                RelationId = e.RelationRecordId,
                Description = e.Description,
                RawRootAttribute = e.RootAttributeValue,
                RegistrationCommitment = e.RegistrationCommitment?.Commitment,
                GroupId = e.RelationGroup?.RelationGroupId ?? 0
            }));
        }

        [HttpPost("{accountId}/RelationGroup/{groupId}/Relation")]
        public IActionResult AddRelation(long accountId, long groupId, [FromBody] RelationDto relation)
        {
            relation.RelationId = _dataAccessService.AddRelationToGroup(accountId, relation.Description, relation.RawRootAttribute, groupId);
            relation.GroupId = groupId;

            return Ok(relation);
        }

        #endregion Relations

        #region Documents

        [HttpPost("Document")]
        public IActionResult AddDocument(long accountId, [FromBody] DocumentDto documentDto)
        {
            documentDto.DocumentId = _dataAccessService.AddSpDocument(accountId, documentDto.DocumentName, documentDto.Hash);
            SpDocument document = _dataAccessService.GetSpDocument(accountId, documentDto.DocumentId);

            var persistency = _executionContextManager.ResolveExecutionServices(accountId);
            var transactionsService = persistency.Scope.ServiceProvider.GetService<IStateTransactionsService>();
            transactionsService.IssueDocumentRecord(document.Hash.HexStringToByteArray(), document.AllowedSigners?.Select(s => s.GroupCommitment.HexStringToByteArray()).ToArray());

            return Ok(documentDto);
        }

        [HttpPost("AllowedSigner")]
        public async Task<IActionResult> AddAllowedSigner(long accountId, long documentId, [FromBody] AllowedSignerDto allowedSigner)
        {
            byte[] groupAssetId = await _assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_RELATIONGROUP, allowedSigner.GroupOwner + allowedSigner.GroupName, allowedSigner.GroupOwner).ConfigureAwait(false);
            byte[] blindingFactor = CryptoHelper.GetRandomSeed();
            byte[] groupCommitment = CryptoHelper.GetAssetCommitment(blindingFactor, groupAssetId);

            allowedSigner.AllowedSignerId = _dataAccessService.AddSpDocumentAllowedSigner(accountId, documentId, allowedSigner.GroupOwner, allowedSigner.GroupName, groupCommitment.ToHexString(), blindingFactor.ToHexString());

            SpDocument document = _dataAccessService.GetSpDocument(accountId, documentId);

            var persistency = _executionContextManager.ResolveExecutionServices(accountId);
            var transactionsService = persistency.Scope.ServiceProvider.GetService<IStateTransactionsService>();
            transactionsService.IssueDocumentRecord(document.Hash.HexStringToByteArray(), document.AllowedSigners.Select(s => s.GroupCommitment.HexStringToByteArray()).ToArray());

            return Ok(allowedSigner);
        }

        [HttpDelete("AllowedSigner")]
        public IActionResult DeleteAllowedSigner(long accountId, long allowedSignerId)
        {
            long documentId = _dataAccessService.RemoveSpDocumentAllowedSigner(accountId, allowedSignerId);
            SpDocument document = _dataAccessService.GetSpDocument(accountId, documentId);

            var persistency = _executionContextManager.ResolveExecutionServices(accountId);
            var transactionsService = persistency.Scope.ServiceProvider.GetService<IStateTransactionsService>();
            transactionsService.IssueDocumentRecord(document.Hash.HexStringToByteArray(), document.AllowedSigners.Select(s => s.GroupCommitment.HexStringToByteArray()).ToArray());

            return Ok();
        }

        [HttpGet("Documents")]
        public IActionResult GetDocuments(long accountId)
        {
            IEnumerable<DocumentDto> documents = _dataAccessService.GetSpDocuments(accountId).Select(d => new DocumentDto
            {
                DocumentId = d.SpDocumentId,
                DocumentName = d.DocumentName,
                Hash = d.Hash,
                AllowedSigners = (d.AllowedSigners?.Select(s => new AllowedSignerDto
                {
                    AllowedSignerId = s.SpDocumentAllowedSignerId,
                    GroupName = s.GroupName,
                    GroupOwner = s.GroupIssuer
                }) ?? Array.Empty<AllowedSignerDto>()).ToList(),
                Signatures = (d.DocumentSignatures?.Select(s => new DocumentSignatureDto
                {
                    DocumentId = s.Document.SpDocumentId,
                    SignatureId = s.SpDocumentSignatureId,
                    DocumentHash = s.Document.Hash,
                    DocumentRecordHeight = s.DocumentRecordHeight,
                    SignatureRecordHeight = s.SignatureRecordHeight
                }) ?? Array.Empty<DocumentSignatureDto>()).ToList()
            });

            return Ok(documents);
        }

        [HttpDelete("Document")]
        public IActionResult DeleteDocument(long accountId, long documentId)
        {
            _dataAccessService.RemoveSpDocument(accountId, documentId);

            return Ok();
        }

        [HttpPost("CalculateFileHash"), DisableRequestSizeLimit]
        public IActionResult CalculateFileHash()
        {
            var file = Request.Form.Files[0];

            if (file.Length > 0)
            {
                using var stream = new MemoryStream();
                file.CopyTo(stream);

                byte[] hash = _hashCalculation.CalculateHash(stream.ToArray());

                return Ok(new { documentName = file.FileName, hash = hash.ToHexString() });
            }
            else
            {
                return BadRequest();
            }
        }

        #endregion Documents

        /*[HttpPost("Employee")]
        public IActionResult UpdateEmployee(long accountId, [FromBody] EmployeeDto employee)
        {
            _dataAccessService.ChangeRelationGroup(accountId, employee.EmployeeId, employee.GroupId);
            var persistency = _executionContextManager.ResolveExecutionServices(accountId);
            var transactionsService = persistency.Scope.ServiceProvider.GetService<IStateTransactionsService>();
            transactionsService.IssueCancelEmployeeRecord(employee.RegistrationCommitment.HexStringToByteArray());

            return Ok(employee);
        }

        [HttpDelete("Employee")]
        public IActionResult DeleteEmployee(long accountId, long employeeId)
        {
            RelationRecord spEmployee = _dataAccessService.RemoveRelation(accountId, employeeId);
            var persistency = _executionContextManager.ResolveExecutionServices(accountId);
            var transactionsService = persistency.Scope.ServiceProvider.GetService<IStateTransactionsService>();

            if (!string.IsNullOrEmpty(spEmployee.RegistrationCommitment))
            {
                transactionsService.IssueCancelEmployeeRecord(spEmployee.RegistrationCommitment.HexStringToByteArray());
            }

            return Ok();
        }
*/

        [HttpGet("UserTransaction")]
        public IActionResult GetUserTransactions(long accountId)
        {
            IEnumerable<ServiceProviderRegistration> registrations = _dataAccessService.GetServiceProviderRegistrations(accountId);
            IEnumerable<SpUserTransactionDto> transactionDtos = _consentManagementService.GetUserTransactions(accountId);
            var result = registrations.Select(r =>
                        new ServiceProviderRegistrationDto
                        {
                            RegistrationId = r.ServiceProviderRegistrationId,
                            Commitment = r.Commitment.ToHexString(),
                            Transactions = transactionDtos.Where(t => t.RegistrationId == r.ServiceProviderRegistrationId).ToArray()
                        });

            return Ok(result);
        }

        [HttpGet("UserTransaction/{registrationId}")]
        public IActionResult GetUserTransactions(long accountId, long registrationId)
        {
            ServiceProviderRegistration serviceProviderRegistration = _dataAccessService.GetServiceProviderRegistration(registrationId);
            IEnumerable<SpUserTransactionDto> transactionDtos = _consentManagementService.GetUserTransactions(accountId);
            var result = new ServiceProviderRegistrationDto
            {
                RegistrationId = registrationId,
                Commitment = serviceProviderRegistration.Commitment.ToHexString(),
                Transactions = transactionDtos.Where(t => t.RegistrationId == registrationId).ToArray()
            };

            return Ok(result);
        }

        [HttpPost("UserTransaction")]
        public async Task<IActionResult> PushUserTransaction(long accountId, [FromBody] SpUserTransactionRequestDto spUserTransactionRequest)
        {
            AccountDescriptor account = _accountsService.GetById(accountId);
            string transactionKey = Guid.NewGuid().ToString();
            long registrationId = spUserTransactionRequest.RegistrationId;
            long userTransactionId = _dataAccessService.AddSpUserTransaction(accountId, registrationId, transactionKey, spUserTransactionRequest.Description);
            var registration = _dataAccessService.GetServiceProviderRegistration(registrationId);

            TransactionConsentRequest consentRequest = new TransactionConsentRequest
            {
                RegistrationCommitment = registration.Commitment.ToHexString(),
                TransactionId = transactionKey,
                WithKnowledgeProof = true,
                Description = spUserTransactionRequest.Description,
                PublicSpendKey = account.PublicSpendKey.ToHexString(),
                PublicViewKey = account.PublicViewKey?.ToHexString()
            };

            await _restApiConfiguration.ConsentManagementUri
                .AppendPathSegments("ConsentManagement", "TransactionForConsent")
                .PostJsonAsync(consentRequest).ContinueWith(t =>
                {
                    if (!t.IsCompletedSuccessfully)
                    {
                        _logger.Error($"Failed to register transaction for consent. {JsonConvert.SerializeObject(consentRequest)}", t.Exception);
                    }
                }, TaskScheduler.Current).ConfigureAwait(false);

            return Ok(new SpUserTransactionDto
            {
                SpUserTransactionId = userTransactionId,
                TransactionKey = transactionKey,
                RegistrationId = spUserTransactionRequest.RegistrationId,
                Description = spUserTransactionRequest.Description
            });
        }
    }
}
