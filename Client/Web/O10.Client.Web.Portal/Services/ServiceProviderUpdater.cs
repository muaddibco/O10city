using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using O10.Transactions.Core.Ledgers.Stealth;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Services;
using O10.Core.Cryptography;
using O10.Core.ExtensionMethods;
using O10.Crypto.ConfidentialAssets;
using O10.Client.Web.Portal.Dtos;
using O10.Transactions.Core.Ledgers.O10State;
using O10.Client.DataLayer.Model;
using O10.Transactions.Core.Parsers;
using O10.Core.Identity;
using System.Threading.Tasks.Dataflow;
using System.Globalization;
using Flurl.Http;
using O10.Client.Web.Portal.Dtos.ServiceProvider;
using O10.Core.Logging;
using O10.Client.Web.Common.Hubs;
using System.Threading.Tasks;
using O10.Client.DataLayer.AttributesScheme;
using System.Threading;
using Newtonsoft.Json;
using O10.Client.DataLayer.Entities;
using O10.Client.Common.Dtos.UniversalProofs;
using O10.Client.Web.Portal.ElectionCommittee;
using Newtonsoft.Json.Linq;
using O10.Client.Web.Portal.ElectionCommittee.Models;
using O10.Core.Architecture;
using O10.Client.Common.Exceptions;
using O10.Client.Common.Entities;
using O10.Core.Serialization;
using O10.Core.Notifications;
using O10.Transactions.Core.Ledgers;

namespace O10.Client.Web.Portal.Services
{
    [RegisterExtension(typeof(IUpdater), Lifetime = LifetimeManagement.Scoped)]
    public class ServiceProviderUpdater : IUpdater
    {
        private long _accountId;
        private readonly ILogger _logger;
        private readonly IStateClientCryptoService _clientCryptoService;
        private readonly IAssetsService _assetsService;
        private readonly ISchemeResolverService _schemeResolverService;
        private readonly IDataAccessService _dataAccessService;
        private readonly IIdentityAttributesService _identityAttributesService;
        private readonly IBlockParsersRepositoriesRepository _blockParsersRepositoriesRepository;
        private readonly IGatewayService _gatewayService;
        private readonly IStateTransactionsService _transactionsService;
        private readonly IProofsValidationService _proofsValidationService;
        private readonly IHubContext<IdentitiesHub> _idenitiesHubContext;
        private readonly IConsentManagementService _consentManagementService;
        private readonly IUniversalProofsPool _universalProofsPool;
        private readonly IElectionCommitteeService _electionCommitteeService;
        private readonly Dictionary<IKey, string> _keyImageToSessonKeyMap = new Dictionary<IKey, string>(new KeyEqualityComparer());

        public ServiceProviderUpdater(
                                IStateClientCryptoService clientCryptoService,
                                IAssetsService assetsService,
                                ISchemeResolverService schemeResolverService,
                                IDataAccessService dataAccessService,
                                IIdentityAttributesService identityAttributesService,
                                IBlockParsersRepositoriesRepository blockParsersRepositoriesRepository,
                                IGatewayService gatewayService,
                                IStateTransactionsService transactionsService,
                                IProofsValidationService proofsValidationService,
                                IHubContext<IdentitiesHub> idenitiesHubContext,
                                ILoggerService loggerService,
                                IConsentManagementService consentManagementService,
                                IUniversalProofsPool universalProofsPool,
                                IElectionCommitteeService electionCommitteeService)
        {
            _clientCryptoService = clientCryptoService;
            _assetsService = assetsService;
            _schemeResolverService = schemeResolverService;
            _dataAccessService = dataAccessService;
            _identityAttributesService = identityAttributesService;
            _blockParsersRepositoriesRepository = blockParsersRepositoriesRepository;
            _gatewayService = gatewayService;
            _transactionsService = transactionsService;
            _proofsValidationService = proofsValidationService;
            _idenitiesHubContext = idenitiesHubContext;
            _consentManagementService = consentManagementService;
            _universalProofsPool = universalProofsPool;
            _electionCommitteeService = electionCommitteeService;
            _logger = loggerService.GetLogger(nameof(ServiceProviderUpdater));

            PipeIn = new ActionBlock<PacketBase>(async p =>
            {
                if (p == null)
                {
                    _logger.Error($"[{_accountId}]: Obtained NULL packet");
                    return;
                }

                _logger.Info($"[{_accountId}]: Obtained {p.GetType().Name} packet");

                try
                {
                    if (p is DocumentSignRecord documentSignRecord)
                    {
                        ProcessDocumentSignRecord(documentSignRecord);
                    }

                    if (p is DocumentRecord documentRecord)
                    {
                        ProcessDocumentRecord(documentRecord);
                    }

                    if (p is DocumentSignRequest documentSignRequest)
                    {
                        ProcessDocumentSignRequest(documentSignRequest);
                    }

                    if (p is EmployeeRegistrationRequest employeeRegistrationRequest)
                    {
                        ProcessEmployeeRegistrationRequest(employeeRegistrationRequest);
                    }

                    if (p is IdentityProofs packet)
                    {
                        await ProcessIdentityProofs(packet).ConfigureAwait(false);
                    }

                    if (p is TransitionCompromisedProofs compromisedProofs)
                    {
                        ProcessCompromisedProofs(new Key32(compromisedProofs.CompromisedKeyImage));
                    }

                    if (p is TransferAsset transferAsset)
                    {
                        ProcessTransferAsset(transferAsset);
                    }

                    if (p is UniversalTransport universalTransport)
                    {
                        await ProcessUniversalTransport(universalTransport).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"[{_accountId}]: Failed to process packet {p.GetType().Name}", ex);
                }
            });
        }

        private async Task ProcessUniversalTransport(UniversalTransport universalTransport)
        {
            TaskCompletionSource<UniversalProofs> universalProofsTask = _universalProofsPool.Extract(universalTransport.KeyImage);

            try
            {
                UniversalProofs universalProofs = await universalProofsTask.Task.ConfigureAwait(false);
                var mainIssuer = universalProofs.RootIssuers.Find(i => i.Issuer.Equals(universalProofs.MainIssuer));
                IKey commitmentKey = mainIssuer.IssuersAttributes.FirstOrDefault(a => a.Issuer.Equals(mainIssuer.Issuer))?.RootAttribute.Commitment;
                SurjectionProof eligibilityProof = mainIssuer.IssuersAttributes.FirstOrDefault(a => a.Issuer.Equals(mainIssuer.Issuer))?.RootAttribute.BindingProof;
                bool isEligibilityCorrect = await CheckEligibilityProofs(commitmentKey.Value, eligibilityProof, mainIssuer.Issuer.Value).ConfigureAwait(false);

                if (!isEligibilityCorrect && !string.IsNullOrEmpty(universalProofs.SessionKey))
                {
                    await _idenitiesHubContext.Clients.Group(universalProofs.SessionKey).SendAsync("EligibilityCheckFailed").ConfigureAwait(false);
                    return;
                }

                try
                {
                    await _proofsValidationService.VerifyKnowledgeFactorProofs(universalProofs.RootIssuers).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;

                    if (ex is AggregateException aex)
                    {
                        if (aex.InnerException is FlurlHttpException fex)
                        {
                            _logger.Error($"[{_accountId}]: Failed request '{fex.Call.Request.RequestUri}' with body '{fex.Call.RequestBody}'");
                        }
                        msg = aex.InnerException.Message;

                        _logger.Error($"[{_accountId}]: Failure at {nameof(ProcessUniversalTransport)}", aex.InnerException);
                    }
                    else
                    {
                        _logger.Error($"[{_accountId}]: Failure at {nameof(ProcessUniversalTransport)}", ex);
                    }

                    await _idenitiesHubContext.Clients.Group(universalProofs.SessionKey).SendAsync("ProtectionCheckFailed", msg).ConfigureAwait(false);
                    return;
                }

                switch (universalProofs.Mission)
                {
                    case UniversalProofsMission.Authentication:
                        await ProcessUniversalProofsAuthentication(
                            universalProofs.RootIssuers.Find(i => i.Issuer.Equals(universalProofs.MainIssuer)), 
                            universalProofs.SessionKey,
                            universalProofs.KeyImage).ConfigureAwait(false);
                        break;
                    case UniversalProofsMission.Vote:
                        ProcessVote(universalProofs);
                        break;
                    default:
                        break;
                }

            }
            catch (TimeoutException)
            {
                _logger.Error($"[{_accountId}]: Timeout during obtaining {nameof(UniversalProofs)} for key image {universalTransport.KeyImage}");
            }
        }

        private void ProcessVote(UniversalProofs proofs)
        {
            var payload = ((JObject)proofs.Payload).ToObject<ElectionCommitteePayload>();
            _electionCommitteeService.UpdatePollSelection(payload.PollId, payload);
        }

        private async Task ProcessUniversalProofsAuthentication(RootIssuer rootIssuer, string sessionKey, IKey keyImage)
        {
            IEnumerable<SpIdenitityValidation> spIdenitityValidations = _dataAccessService.GetSpIdenitityValidations(_accountId);
            IKey commitment = rootIssuer.IssuersAttributes.
                                FirstOrDefault(a => a.Issuer.Equals(rootIssuer.Issuer))?.
                                    RootAttribute.Commitment;
            try
            {
                await _proofsValidationService.CheckAssociatedProofs(rootIssuer, ToValidationCriterias(spIdenitityValidations)).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is NoValidationProofsException || ex is ValidationProofFailedException || ex is ValidationProofsWereNotCompleteException)
            {
                _idenitiesHubContext.Clients.Group(_accountId.ToString(CultureInfo.InvariantCulture)).SendAsync("PushAuthorizationFailed", new { Code = 3, ex.Message }).Wait();
                _idenitiesHubContext.Clients.Group(sessionKey).SendAsync("PushSpAuthorizationFailed", new { Code = 3, ex.Message }).Wait();
                throw;
            }

            SurjectionProof registrationProof = rootIssuer.IssuersAttributes.FirstOrDefault(a => a.Issuer.Equals(rootIssuer.Issuer))?.RootAttribute.CommitmentProof.SurjectionProof;
            (long registrationId, bool isNew) = _proofsValidationService.HandleRegistration(_accountId, commitment.Value, registrationProof);

            var issuer = rootIssuer.Issuer.ToString();
            var issuerName = await _schemeResolverService.ResolveIssuer(issuer).ConfigureAwait(false);
            var rootAttributeDefinition = await _assetsService.GetRootAttributeDefinition(issuer).ConfigureAwait(false);
            

            if (isNew)
            {
                await _idenitiesHubContext
                    .Clients.Group(_accountId.ToString(CultureInfo.InvariantCulture))
                    .SendAsync("PushRegistration",
                        new ServiceProviderRegistrationExDto
                        {
                            Issuer = issuer,
                            IssuerName = issuerName,
                            RootAttributeName = rootAttributeDefinition.AttributeName,
                            ServiceProviderRegistrationId = registrationId.ToString(CultureInfo.InvariantCulture),
                            Commitment = registrationProof.AssetCommitments[0].ToHexString(),
                            IssuanceCommitments = rootIssuer.IssuersAttributes.Find(s => s.Issuer.Equals(rootIssuer.Issuer)).RootAttribute.BindingProof.AssetCommitments.Select(a => a.ToHexString()).ToList()
                        })
                    .ConfigureAwait(false);
                await _idenitiesHubContext
                    .Clients.Group(sessionKey)
                    .SendAsync("PushUserRegistration",
                        new ServiceProviderRegistrationDto
                        {
                            ServiceProviderRegistrationId = registrationId.ToString(CultureInfo.InvariantCulture),
                            Commitment = registrationProof.AssetCommitments[0].ToHexString()
                        })
                    .ConfigureAwait(false);
            }
            else
            {
                await _idenitiesHubContext
                    .Clients
                    .Group(_accountId.ToString(CultureInfo.InvariantCulture))
                    .SendAsync("PushAuthorizationSucceeded",
                        new ServiceProviderRegistrationExDto
                        {
                            Issuer = issuer,
                            IssuerName = issuerName,
                            RootAttributeName = rootAttributeDefinition.AttributeName,
                            ServiceProviderRegistrationId = registrationId.ToString(CultureInfo.InvariantCulture),
                            Commitment = registrationProof.AssetCommitments[0].ToHexString(),
                            IssuanceCommitments = rootIssuer.IssuersAttributes.Find(s => s.Issuer.Equals(rootIssuer.Issuer)).RootAttribute.BindingProof.AssetCommitments.Select(a => a.ToHexString()).ToList()
                        }).ConfigureAwait(false);
                ProceedCorrectAuthentication(keyImage, sessionKey);
            }
        }

        public ITargetBlock<PacketBase> PipeIn { get; set; }
        public ITargetBlock<NotificationBase> PipeInNotifications { get; }

        private void ProcessDocumentSignRecord(DocumentSignRecord packet)
        {
            if (_dataAccessService.UpdateSpDocumentSignature(_accountId, packet.DocumentHash.ToHexString(), packet.RecordHeight, packet.Height, packet.RawData.ToArray()))
            {
                _logger.Info($"[{_accountId}]: Document with hash {packet.DocumentHash.ToHexString()} was signed successfully");
            }
            else
            {
                _logger.Error($"[{_accountId}]: Failed to update raw signature record of Document with hash {packet.DocumentHash.ToHexString()}");
            }
        }

        private void ProcessDocumentRecord(DocumentRecord packet)
        {
            _dataAccessService.UpdateSpDocumentChangeRecord(_accountId, packet.DocumentHash.ToHexString(), packet.Height);
        }

        private async void ProcessDocumentSignRequest(DocumentSignRequest packet)
        {
            _clientCryptoService.DecodeEcdhTuple(packet.EcdhTuple, packet.TransactionPublicKey, out byte[] groupNameBlindingFactor, out byte[] documentHash, out byte[] issuer, out byte[] payload);
            string sessionKey = payload.ToHexString();
            SignedDocumentEntity spDocument = _dataAccessService.GetSpDocument(_accountId, documentHash.ToHexString());

            if (spDocument == null)
            {
                _logger.Error($"[{_accountId}]: Failed to find document for account {_accountId} with hash {documentHash.ToHexString()}");
                await _idenitiesHubContext.Clients.Group(sessionKey).SendAsync("PushDocumentNotFound").ConfigureAwait(false);
                return;
            }
            else
            {
                _logger.LogIfDebug(() => $"[{_accountId}]: Signing document {JsonConvert.SerializeObject(spDocument, new ByteArrayJsonConverter())}");
            }

            bool isEligibilityCorrect = await CheckEligibilityProofs(packet.AssetCommitment, packet.EligibilityProof, issuer).ConfigureAwait(false);

            if (!isEligibilityCorrect)
            {
                _logger.Error($"[{_accountId}]: Eligibility proofs were wrong");
                //await _idenitiesHubContext.Clients.Group(sessionKey).SendAsync("PushDocumentSignIncorrect", new { Code = 2, Message = "Eligibility proofs were wrong" }).ConfigureAwait(false);
                //return;
            }
            else
            {
                _logger.Debug($"[{_accountId}]: Eligibility proofs were correct");
            }

            if (!ConfidentialAssetsHelper.VerifySurjectionProof(packet.SignerGroupRelationProof, packet.AssetCommitment, documentHash, BitConverter.GetBytes(spDocument.LastChangeRecordHeight)))
            {
                _logger.Error($"[{_accountId}]: Signer group relation proofs were wrong");
                await _idenitiesHubContext.Clients.Group(sessionKey).SendAsync("PushDocumentSignIncorrect", new { Code = 2, Message = "Signer group relation proofs were wrong" }).ConfigureAwait(false);
                return;
            }

            _logger.Debug($"[{_accountId}]: Checking Allowed Signers");

            SurjectionProof signatureGroupProof = null;
            string groupIssuer = null;
            foreach (var allowedSigner in spDocument.AllowedSigners)
            {
                _logger.LogIfDebug(() => $"[{_accountId}]: Checking agains allowed signer definition {JsonConvert.SerializeObject(allowedSigner, new ByteArrayJsonConverter())}");

                byte[] groupAssetId = await _assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_EMPLOYEEGROUP, allowedSigner.GroupIssuer + allowedSigner.GroupName, allowedSigner.GroupIssuer).ConfigureAwait(false);
                byte[] expectedGroupCommitment = ConfidentialAssetsHelper.GetAssetCommitment(groupNameBlindingFactor, groupAssetId);
                if (packet.AllowedGroupCommitment.Equals32(expectedGroupCommitment))
                {
                    _logger.Debug($"[{_accountId}]: Checking allowed signer started");
                    byte[] groupCommitment = await _gatewayService.GetEmployeeRecordGroup(allowedSigner.GroupIssuer.HexStringToByteArray(), packet.SignerGroupRelationProof.AssetCommitments[0]).ConfigureAwait(false);
                    if (groupCommitment != null && ConfidentialAssetsHelper.VerifySurjectionProof(packet.AllowedGroupNameSurjectionProof, packet.AllowedGroupCommitment))
                    {
                        _logger.Debug($"[{_accountId}]: validation of signer succeeded");
                        byte[] diffBF = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(groupNameBlindingFactor, allowedSigner.BlindingFactor);
                        byte[][] commitments = spDocument.AllowedSigners.Select(s => s.GroupCommitment).ToArray();
                        byte[] allowedGroupCommitment = allowedSigner.GroupCommitment;
                        int index = 0;

                        for (; index < commitments.Length; index++)
                        {
                            if (commitments[index].Equals32(allowedGroupCommitment))
                            {
                                break;
                            }
                        }

                        signatureGroupProof = ConfidentialAssetsHelper.CreateSurjectionProof(packet.AllowedGroupCommitment, commitments, index, diffBF);
                        groupIssuer = allowedSigner.GroupIssuer;
                        break;
                    }
                    else
                    {
                        _logger.Error($"[{_accountId}]: validation of signer failed");
                    }
                }
                else
                {
                    _logger.Debug($"[{_accountId}]: Skipping");
                }
            }

            if (signatureGroupProof == null)
            {
                _logger.Error($"[{_accountId}]: Signer group relation proofs were wrong");
                await _idenitiesHubContext.Clients.Group(sessionKey).SendAsync("PushDocumentSignIncorrect", new { Code = 2, Message = "Signer group relation proofs were wrong" }).ConfigureAwait(false);
                return;
            }
            else
            {
                _logger.Debug($"[{_accountId}]: Signer group relation proofs were correct");
            }

            _logger.Info($"[{_accountId}]: All verifications for signing request of document {spDocument.DocumentName} at account {_accountId} were passed. Issuing document sign record");

            var issueDocumentSignTransaction = await _transactionsService.IssueDocumentSignRecord(documentHash, spDocument.LastChangeRecordHeight, packet.KeyImage.Value.ToArray(), packet.AssetCommitment, packet.EligibilityProof, issuer, packet.SignerGroupRelationProof, packet.AllowedGroupCommitment, groupIssuer.HexStringToByteArray(), packet.AllowedGroupNameSurjectionProof, signatureGroupProof).ConfigureAwait(false);
            long signatureId = _dataAccessService.AddSpDocumentSignature(_accountId, spDocument.DocumentId, spDocument.LastChangeRecordHeight, issueDocumentSignTransaction.Height);

            DocumentSignatureDto documentSignature = new DocumentSignatureDto
            {
                DocumentId = spDocument.DocumentId,
                DocumentHash = spDocument.Hash,
                DocumentRecordHeight = spDocument.LastChangeRecordHeight,
                SignatureRecordHeight = issueDocumentSignTransaction.Height
            };

            _logger.Info($"[{_accountId}]: DocumentSignature: {JsonConvert.SerializeObject(documentSignature)}");

            _logger.Info("[{_accountId}]: Sending PushDocumentSignature to the Back Office of SP");
            await _idenitiesHubContext.Clients.Group(_accountId.ToString(CultureInfo.InvariantCulture))
                .SendAsync("PushDocumentSignature", documentSignature).ConfigureAwait(false);

            _logger.Info("Sending PushDocumentSignature to the Front Office of SP");
            await _idenitiesHubContext.Clients.Group(sessionKey)
                .SendAsync("PushDocumentSignature", documentSignature).ConfigureAwait(false);
        }

        private async void ProcessEmployeeRegistrationRequest(EmployeeRegistrationRequest packet)
        {
            _clientCryptoService.DecodeEcdhTuple(packet.EcdhTuple, packet.TransactionPublicKey, out byte[] blindingFactor, out byte[] assetId, out byte[] issuer, out byte[] payload);
            string sessionKey = payload.ToHexString();
            List<SpEmployee> spEmployees = _dataAccessService.GetSpEmployees(_accountId);
            SpEmployee spEmployee = null;

            byte[] registrationCommitment = ConfidentialAssetsHelper.GetAssetCommitment(blindingFactor, assetId);
            if (!registrationCommitment.Equals32(packet.AssetCommitment))
            {
                await _idenitiesHubContext.Clients.Group(sessionKey).SendAsync("PushEmployeeIncorrectRegistration", new { Code = 1, Message = "RegistrationCommitment does not match to provided Asset Id" }).ConfigureAwait(false);
                return;
            }

            var rootAttributeDefinition = await _assetsService.GetRootAttributeDefinition(issuer.ToHexString()).ConfigureAwait(false);

            foreach (var item in spEmployees)
            {
                byte[] employeeAssetId = _assetsService.GenerateAssetId(rootAttributeDefinition.SchemeId, item.RootAttributeRaw);

                if (employeeAssetId.Equals32(assetId) && item.SpEmployeeGroup != null)
                {
                    spEmployee = item;
                    break;
                }
            }

            if (spEmployee != null)
            {
                if (!string.IsNullOrEmpty(spEmployee.RegistrationCommitment) && spEmployee.RegistrationCommitment.Equals(packet.AssetCommitment.ToHexString()))
                {
                    await _idenitiesHubContext.Clients.Group(sessionKey).SendAsync("PushRelationAlreadyRegistered").ConfigureAwait(false);
                    return;
                }

                byte[] groupAssetId = await _assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_EMPLOYEEGROUP, _clientCryptoService.PublicKeys[0].ArraySegment.Array.ToHexString() + spEmployee.SpEmployeeGroup.GroupName, _clientCryptoService.PublicKeys[0].ArraySegment.Array.ToHexString()).ConfigureAwait(false);

                long relationId;
                if (ConfidentialAssetsHelper.VerifyIssuanceSurjectionProof(packet.GroupSurjectionProof, packet.GroupCommitment, new byte[][] { groupAssetId }))
                {
                    relationId = spEmployee.SpEmployeeId;
                }
                else
                {
                    await _idenitiesHubContext.Clients.Group(sessionKey).SendAsync("PushEmployeeIncorrectRegistration", new { Code = 3, Message = "Group proofs were wrong" }).ConfigureAwait(false);
                    return;
                }

                bool isEligibilityCorrect = await CheckEligibilityProofs(packet.AssetCommitment, packet.EligibilityProof, issuer).ConfigureAwait(false);

                if (!isEligibilityCorrect)
                {
                    await _idenitiesHubContext.Clients.Group(sessionKey).SendAsync("PushEmployeeIncorrectRegistration", new { Code = 2, Message = "Eligibility proofs were wrong" }).ConfigureAwait(false);
                    return;
                }

                IEnumerable<SpIdenitityValidation> spIdenitityValidations = _dataAccessService.GetSpIdenitityValidations(_accountId);

                try
                {
                    await _proofsValidationService
                        .CheckSpIdentityValidations(
                            packet.AssetCommitment,
                            packet.AssociatedProofs,
                            ToValidationCriterias(spIdenitityValidations),
                            issuer.ToHexString()).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is NoValidationProofsException || ex is ValidationProofFailedException || ex is ValidationProofsWereNotCompleteException)
                {
                    _idenitiesHubContext.Clients.Group(sessionKey).SendAsync("PushSpAuthorizationFailed", new { Code = 3, Message = ex.Message }).Wait();
                    throw;
                }

                _dataAccessService.SetSpEmployeeRegistrationCommitment(_accountId, relationId, packet.AssetCommitment.ToHexString());
                _transactionsService.IssueEmployeeRecord(packet.AssetCommitment, packet.GroupCommitment);
                await _idenitiesHubContext.Clients.Group(_accountId.ToString(CultureInfo.InvariantCulture)).SendAsync("PushEmployeeUpdate", new EmployeeDto { AssetId = assetId.ToHexString(), RegistrationCommitment = packet.AssetCommitment.ToHexString() }).ConfigureAwait(false);
                await _idenitiesHubContext.Clients.Group(sessionKey).SendAsync("PushEmployeeRegistration", new { Commitment = packet.AssetCommitment.ToHexString() }).ConfigureAwait(false);
            }
            else
            {
                await _idenitiesHubContext.Clients.Group(sessionKey).SendAsync("PushEmployeeNotRegistered").ConfigureAwait(false);
                return;
            }
        }

        private static IEnumerable<ValidationCriteria> ToValidationCriterias(IEnumerable<SpIdenitityValidation> spIdenitityValidations)
        {
            return spIdenitityValidations
                                            .Select(v =>
                                                new ValidationCriteria { SchemeName = v.SchemeName, ValidationType = v.ValidationType, NumericCriterion = v.NumericCriterion, GroupIdCriterion = v.GroupIdCriterion });
        }

        private async Task ProcessIdentityProofs(IdentityProofs packet)
        {
            _clientCryptoService.DecodeEcdhTuple(packet.EncodedPayload, packet.TransactionPublicKey, out byte[] blindingFactor, out byte[] assetId, out byte[] issuer, out byte[] payload);
            string sessionKey = payload.ToHexString();

            if (_consentManagementService.TryGetTransactionRequestByKey(sessionKey, packet.KeyImage.ToString(), out string transactionId, out bool isConfirmed))
            {
                if (isConfirmed)
                {
                    _dataAccessService.SetSpUserTransactionConfirmed(_accountId, transactionId);
                }
                else
                {
                    _dataAccessService.SetSpUserTransactionDeclined(_accountId, transactionId);
                }

                return;
            }

            bool isEligibilityCorrect = await CheckEligibilityProofs(packet.AssetCommitment, packet.EligibilityProof, issuer).ConfigureAwait(false);

            if (!isEligibilityCorrect)
            {
                _idenitiesHubContext.Clients.Group(sessionKey).SendAsync("PushSpAuthorizationFailed", new { Code = 2, Message = "Eligibility proofs were wrong" }).Wait();
                return;
            }

            IEnumerable<SpIdenitityValidation> spIdenitityValidations = _dataAccessService.GetSpIdenitityValidations(_accountId);
            try
            {
                await _proofsValidationService.CheckSpIdentityValidations(packet.AssetCommitment, packet.AssociatedProofs, ToValidationCriterias(spIdenitityValidations), issuer.ToHexString()).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is NoValidationProofsException || ex is ValidationProofFailedException || ex is ValidationProofsWereNotCompleteException)
            {
                _idenitiesHubContext.Clients.Group(sessionKey).SendAsync("PushSpAuthorizationFailed", new { Code = 3, Message = ex.Message }).Wait();
                throw;
            }

            (long registrationId, bool isNew) = _proofsValidationService.HandleRegistration(_accountId, packet.AssetCommitment, packet.AuthenticationProof);

            if (isNew)
            {
                await _idenitiesHubContext.Clients.Group(_accountId.ToString(CultureInfo.InvariantCulture)).SendAsync("PushRegistration", new ServiceProviderRegistrationDto { ServiceProviderRegistrationId = registrationId.ToString(CultureInfo.InvariantCulture), Commitment = packet.AuthenticationProof.AssetCommitments[0].ToHexString() }).ConfigureAwait(false);
                await _idenitiesHubContext.Clients.Group(sessionKey).SendAsync("PushUserRegistration", new ServiceProviderRegistrationDto { ServiceProviderRegistrationId = registrationId.ToString(CultureInfo.InvariantCulture), Commitment = packet.AuthenticationProof.AssetCommitments[0].ToHexString() }).ConfigureAwait(false);
            }
            else
            {
                ProceedCorrectAuthentication(packet.KeyImage, sessionKey);
            }
        }

        private void ProcessTransferAsset(TransferAsset transferAsset)
        {
            _clientCryptoService.DecodeEcdhTuple(transferAsset.TransferredAsset.EcdhTuple, null, out byte[] blindingFactor, out byte[] assetId);
            _assetsService.GetAttributeSchemeName(assetId, transferAsset.Source.ToString()).ContinueWith(t =>
            {
                if (t.IsCompleted && !t.IsFaulted)
                {
                    _dataAccessService.StoreSpAttribute(_accountId, t.Result, assetId, transferAsset.Source.Value.ToHexString(), blindingFactor, transferAsset.TransferredAsset.AssetCommitment, transferAsset.SurjectionProof.AssetCommitments[0]);

                    _idenitiesHubContext.Clients.Group(_accountId.ToString(CultureInfo.InvariantCulture)).SendAsync("PushAttribute", new SpAttributeDto { SchemeName = t.Result, Source = transferAsset.Source.ArraySegment.Array.ToHexString(), AssetId = assetId.ToHexString(), OriginalBlindingFactor = blindingFactor.ToHexString(), OriginalCommitment = transferAsset.TransferredAsset.AssetCommitment.ToHexString(), IssuingCommitment = transferAsset.SurjectionProof.AssetCommitments[0].ToHexString(), Validated = false, IsOverriden = false });
                }
            }, TaskScheduler.Current);
        }

        private void ProcessCompromisedProofs(IKey compromisedKeyImage)
        {
            _logger.LogIfDebug(() => $"[{_accountId}]: {nameof(ProcessCompromisedProofs)} CompromisedKeyImage = {compromisedKeyImage}");

            string transactionId = _consentManagementService.GetTransactionRequestByKeyImage(compromisedKeyImage.ToString());

            _logger.Debug($"[{_accountId}]: {nameof(transactionId)} from GetTransactionRequestByKeyImage is '{transactionId}'");

            if (!string.IsNullOrEmpty(transactionId))
            {
                _dataAccessService.SetSpUserTransactionCompromised(_accountId, transactionId);
                return;
            }

            if (_keyImageToSessonKeyMap.ContainsKey(compromisedKeyImage))
            {
                string sessionKey = _keyImageToSessonKeyMap[compromisedKeyImage];
                _logger.Debug($"[{_accountId}]: Compromised session found: {sessionKey}");
                _idenitiesHubContext.Clients.Group(sessionKey).SendAsync("PushAuthorizationCompromised");
            }
            else
            {
                _logger.Error($"[{_accountId}]: Compromised session not found");
            }
        }

        private void ProceedCorrectAuthentication(IKey keyImage, string sessionKey)
        {
            _logger.LogIfDebug(() => $"[{_accountId}]: {nameof(ProceedCorrectAuthentication)}, storing {nameof(sessionKey)} {sessionKey} with {nameof(keyImage)} {keyImage}");
            if (!_keyImageToSessonKeyMap.ContainsKey(keyImage))
            {
                _keyImageToSessonKeyMap.Add(keyImage, sessionKey);
            }

            _idenitiesHubContext.Clients.Group(sessionKey).SendAsync("PushSpAuthorizationSucceeded", new { Token = string.Empty });
        }

        private async Task<bool> CheckEligibilityProofs(Memory<byte> assetCommitment, SurjectionProof eligibilityProofs, Memory<byte> issuer)
        {
            _logger.LogIfDebug(() => $"[{_accountId}]: {nameof(CheckEligibilityProofs)} with assetCommitment={assetCommitment.ToHexString()}, issuer={issuer.ToHexString()}, eligibilityProofs={JsonConvert.SerializeObject(eligibilityProofs, new ByteArrayJsonConverter())}");

            try
            {
                await _proofsValidationService.CheckEligibilityProofs(assetCommitment, eligibilityProofs, issuer).ConfigureAwait(false);
            }
            catch (CommitmentNotEligibleException)
            {
                _logger.Error($"[{_accountId}]: {nameof(CheckEligibilityProofs)} found commitment not eligible");
                return false;
            }

            return true;
        }

        public void Initialize(long accountId, CancellationToken cancellationToken)
        {
            _accountId = accountId;
            cancellationToken.Register(() => 
            {
                PipeIn?.Complete();
                PipeInNotifications?.Complete();
            });
        }
    }
}
