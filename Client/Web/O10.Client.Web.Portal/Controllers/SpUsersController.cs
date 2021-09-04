using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using O10.Crypto.ConfidentialAssets;
using O10.Core.ExtensionMethods;
using O10.Client.DataLayer.Model;
using O10.Client.DataLayer.Services;
using System.Collections.Generic;
using O10.Client.Common.Interfaces;
using System.Linq;
using System;
using O10.Client.Web.DataContracts.ServiceProvider;
using O10.Client.Web.Common.Services;
using O10.Client.DataLayer.AttributesScheme;
using System.IO;
using O10.Core.HashCalculations;
using O10.Core;
using O10.Client.Web.Portal.Services;
using O10.Client.DataLayer.Entities;
using O10.Client.Common.Entities;
using O10.Client.Common.Dtos.UniversalProofs;
using O10.Client.DataLayer.Model.ServiceProviders;
using O10.Client.Web.DataContracts;
using System.Threading.Tasks;

namespace O10.Client.Web.Portal.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SpUsersController : ControllerBase
    {
        private readonly IAccountsServiceEx _accountsService;
        private readonly IDataAccessService _dataAccessService;
        private readonly IExecutionContextManager _executionContextManager;
        private readonly IAssetsService _assetsService;
        private readonly IUniversalProofsPool _universalProofsPool;
        private readonly IDocumentSignatureVerifier _documentSignatureVerifier;
        private readonly ICorsPolicyAccessor _corsPolicyAccessor;
        private readonly IHashCalculation _hashCalculation;

        public SpUsersController(IAccountsServiceEx accountsService,
                           IDataAccessService dataAccessService,
                           IHashCalculationsRepository hashCalculationsRepository,
                           IExecutionContextManager executionContextManager,
                           IAssetsService assetsService,
                           IUniversalProofsPool universalProofsPool,
                           IDocumentSignatureVerifier documentSignatureVerifier,
                           ICorsPolicyAccessor corsPolicyAccessor)
        {
            _accountsService = accountsService;
            _dataAccessService = dataAccessService;
            _executionContextManager = executionContextManager;
            _assetsService = assetsService;
            _universalProofsPool = universalProofsPool;
            _documentSignatureVerifier = documentSignatureVerifier;
            _corsPolicyAccessor = corsPolicyAccessor;
            _hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
        }


        [AllowAnonymous]
        [HttpGet("{spId}/SessionInfo")]
        public IActionResult GetSessionInfo(long spId, string origin)
        {
            string nonce = CryptoHelper.GetRandomSeed().ToHexString();
            AccountDescriptor spAccount = _accountsService.GetById(spId);

            if(!string.IsNullOrEmpty(origin) &&  !_corsPolicyAccessor.GetPolicy("Public").Origins.Contains(origin))
            {
                _corsPolicyAccessor.GetPolicy("Public").Origins.Add(origin);
            }

            return Ok(new
            {
                publicKey = spAccount.PublicSpendKey.ToHexString(),
                sessionKey = nonce,
            });
        }

        [AllowAnonymous]
        [HttpGet("GetDocuments/{spId}")]
        public IActionResult GetDocuments(long spId)
        {
            IEnumerable<DocumentDto> documents = _dataAccessService.GetSpDocuments(spId)
                .Select(d =>
                new DocumentDto
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
                        DocumentId = d.SpDocumentId,
                        DocumentHash = d.Hash,
                        SignatureId = s.SpDocumentSignatureId,
                        DocumentRecordHeight = s.DocumentRecordHeight,
                        SignatureRecordHeight = s.SignatureRecordHeight
                    }) ?? Array.Empty<DocumentSignatureDto>()).ToList()
                });

            return Ok(documents);
        }

        /*[HttpGet("DocumentSignatures/{spId}")]
        public async Task<IActionResult> GetDocumentSignatures(long spId)
        {
            AccountDescriptor account = _accountsService.GetById(spId);
            IEnumerable<DocumentDto> documents = await Task.WhenAll(_dataAccessService.GetSpDocuments(spId)
                .Select(async d =>
                new DocumentDto
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
                    Signatures = ((await Task.WhenAll(d.DocumentSignatures?.Select(async s => new DocumentSignatureDto
                    {
                        DocumentId = d.SpDocumentId,
                        DocumentHash = d.Hash,
                        SignatureId = s.SpDocumentSignatureId,
                        DocumentRecordHeight = s.DocumentRecordHeight,
                        SignatureRecordHeight = s.SignatureRecordHeight,
                        SignatureVerification = await _documentSignatureVerifier.Verify(account.PublicSpendKey, d.Hash.HexStringToByteArray(), s.DocumentRecordHeight, s.SignatureRecordHeight).ConfigureAwait(false)
                    })).ConfigureAwait(false)) ?? Array.Empty<DocumentSignatureDto>()).ToList()
                })).ConfigureAwait(false);

            return Ok(documents);
        }

        [HttpPost("AddDocument/{spId}")]
        public IActionResult AddDocument(long spId, [FromBody] DocumentDto documentDto)
        {
            documentDto.DocumentId = _dataAccessService.AddSpDocument(spId, documentDto.DocumentName, documentDto.Hash);
            SpDocument document = _dataAccessService.GetSpDocument(spId, documentDto.DocumentId);

            var persistency = _executionContextManager.ResolveExecutionServices(spId);
            var transactionsService = persistency.Scope.ServiceProvider.GetService<IStateTransactionsService>();
            transactionsService.IssueDocumentRecord(document.Hash.HexStringToByteArray(), document.AllowedSigners?.Select(s => s.GroupCommitment.HexStringToByteArray()).ToArray());

            return Ok(documentDto);
        }

        [HttpDelete("DeleteDocument/{spId}/{documentId}")]
        public IActionResult DeleteDocument(long spId, long documentId)
        {
            _dataAccessService.RemoveSpDocument(spId, documentId);

            return Ok();
        }

        [HttpPost("AddAllowedSigner/{spId}/{documentId}")]
        public async Task<IActionResult> AddAllowedSigner(long spId, long documentId, [FromBody] AllowedSignerDto allowedSigner)
        {
            byte[] groupAssetId = await _assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_RELATIONGROUP, allowedSigner.GroupOwner + allowedSigner.GroupName, allowedSigner.GroupOwner).ConfigureAwait(false);
            byte[] blindingFactor = CryptoHelper.GetRandomSeed();
            byte[] groupCommitment = CryptoHelper.GetAssetCommitment(blindingFactor, groupAssetId);

            allowedSigner.AllowedSignerId = _dataAccessService.AddSpDocumentAllowedSigner(spId, documentId, allowedSigner.GroupOwner, allowedSigner.GroupName, groupCommitment.ToHexString(), blindingFactor.ToHexString());

            SpDocument document = _dataAccessService.GetSpDocument(spId, documentId);

            var persistency = _executionContextManager.ResolveExecutionServices(spId);
            var transactionsService = persistency.Scope.ServiceProvider.GetService<IStateTransactionsService>();
            transactionsService.IssueDocumentRecord(document.Hash.HexStringToByteArray(), document.AllowedSigners.Select(s => s.GroupCommitment.HexStringToByteArray()).ToArray());

            return Ok(allowedSigner);
        }

        [HttpDelete("DeleteAllowedSigner/{spId}/{allowedSignerId}")]
        public IActionResult DeleteAllowedSigner(long spId, long allowedSignerId)
        {
            long documentId = _dataAccessService.RemoveSpDocumentAllowedSigner(spId, allowedSignerId);
            SpDocument document = _dataAccessService.GetSpDocument(spId, documentId);

            var persistency = _executionContextManager.ResolveExecutionServices(spId);
            var transactionsService = persistency.Scope.ServiceProvider.GetService<IStateTransactionsService>();
            transactionsService.IssueDocumentRecord(document.Hash.HexStringToByteArray(), document.AllowedSigners.Select(s => s.GroupCommitment.HexStringToByteArray()).ToArray());

            return Ok();
        }*/

        [AllowAnonymous]
        [HttpGet("Action")]
        public IActionResult GetActionInfo([FromQuery(Name = "t")] ActionTypeDto actionType, [FromQuery(Name = "pk")] string publicKey, [FromQuery(Name = "sk")] string sessionKey, [FromQuery(Name = "rk")] string registrationKey)
        {
            AccountDescriptor spAccount = _accountsService.GetByPublicKey(publicKey.HexStringToByteArray());
            bool isRegistered = false;
            var requiredValidations = new Dictionary<string, ValidationTypeDto>();
            var permittedRelations = new Dictionary<string, List<string>>();
            var existingRelations = new HashSet<string>();
            string[] details = Array.Empty<string>();

            ActionDetailsDto actionInfo = new ActionDetailsDto
            {
                ActionType = actionType,
                AccountInfo = spAccount.AccountInfo,
                IsRegistered = isRegistered,
                PublicKey = publicKey,
                SessionKey = sessionKey,
                IsBiometryRequired = false,
                RequiredValidations = requiredValidations,
                PermittedRelations = permittedRelations
            };

            // Onboarding & Login
            //if (actionType == 0)
            //{
            //    ServiceProviderRegistration serviceProviderRegistration = _dataAccessService.GetServiceProviderRegistration(spAccount.AccountId, registrationKey.HexStringToByteArray());
            //    ;
            //    isRegistered = serviceProviderRegistration != null;
            //}
            // Employee registration
            //else 
            if (actionType == ActionTypeDto.Relation)
            {
                List<RelationRecord> spEmployees = _dataAccessService.GetRelationRecords(spAccount.AccountId, registrationKey.DecodeFromString64());

                foreach (RelationRecord spEmployee in spEmployees.Where(s => s.RegistrationCommitment != null))
                {
                    existingRelations.Add(spEmployee.RelationGroup.GroupName);
                }

                isRegistered = spEmployees.Count > 0;
            }
            else if (actionType == ActionTypeDto.DocumentSign)
            {
                SignedDocumentEntity spDocument = _dataAccessService.GetSpDocument(spAccount.AccountId, registrationKey);
                if (spDocument != null)
                {
                    isRegistered = true;
                    actionInfo.ActionItemKey = $"{spDocument.DocumentName}|{spDocument.Hash}|{spDocument.LastChangeRecordHeight}";

                    foreach (var allowedSigner in spDocument.AllowedSigners)
                    {
                        if(!permittedRelations.ContainsKey(allowedSigner.GroupIssuer))
                        {
                            permittedRelations.Add(allowedSigner.GroupIssuer, new List<string>());
                        }
                        
                        permittedRelations[allowedSigner.GroupIssuer].Add(allowedSigner.GroupName);
                    }
                }
            }

            if (actionType == ActionTypeDto.Identification || actionType == ActionTypeDto.Relation)
            {
                IEnumerable<SpIdenitityValidation> spIdenitityValidations = _dataAccessService.GetSpIdenitityValidations(spAccount.AccountId);

                if (spIdenitityValidations != null && spIdenitityValidations.Count() > 0)
                {
                    foreach (SpIdenitityValidation spIdenitityValidation in spIdenitityValidations)
                    {
                        if (!AttributesSchemes.ATTR_SCHEME_NAME_PASSPORTPHOTO.Equals(spIdenitityValidation.SchemeName))
                        {
                            requiredValidations.Add(spIdenitityValidation.SchemeName, (ValidationTypeDto)spIdenitityValidation.ValidationType);
                        }
                        else
                        {
                            actionInfo.IsBiometryRequired = true;
                        }
                    }
                }
            }

            return Ok(actionInfo);
        }

        [AllowAnonymous]
        [HttpPost("FileHash"), DisableRequestSizeLimit]
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

        [AllowAnonymous]
        [HttpGet("FileHash"), DisableRequestSizeLimit]
        public async Task<IActionResult> CalculateFileHash(string fileName)
        {
            using var stream = new MemoryStream();
            await Request.Body.CopyToAsync(stream).ConfigureAwait(false);

            if (stream.Length > 0)
            {
                byte[] hash = _hashCalculation.CalculateHash(stream.ToArray());

                return Ok(new { documentName = fileName, hash = hash.ToHexString() });
            }
            else
            {
                return BadRequest();
            }
        }

        [AllowAnonymous]
        [HttpPost("UniversalProofs")]
        public IActionResult PostUniversalProofs([FromBody] UniversalProofs universalProofs)
        {
            //UniversalProofs universalProofs = (UniversalProofs)JsonConvert.DeserializeObject(body, typeof(UniversalProofs));
            _universalProofsPool.Store(universalProofs);

            return Ok();
        }
    }
}
