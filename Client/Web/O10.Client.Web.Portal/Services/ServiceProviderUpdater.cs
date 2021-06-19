using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Services;
using O10.Core.Cryptography;
using O10.Core.ExtensionMethods;
using O10.Crypto.ConfidentialAssets;
using O10.Client.Web.Portal.Dtos;
using O10.Client.DataLayer.Model;
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
using O10.Crypto.Models;
using O10.Transactions.Core.Ledgers.Stealth.Transactions;
using O10.Client.DataLayer.Model.ServiceProviders;
using O10.Transactions.Core.Ledgers.O10State.Transactions;

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
        private readonly IGatewayService _gatewayService;
        private readonly IStateTransactionsService _transactionsService;
        private readonly IProofsValidationService _proofsValidationService;
        private readonly IHubContext<IdentitiesHub> _idenitiesHubContext;
        private readonly IConsentManagementService _consentManagementService;
        private readonly IUniversalProofsPool _universalProofsPool;
        private readonly IElectionCommitteeService _electionCommitteeService;
        private readonly IIdentityKeyProvider _identityKeyProvider;
        private readonly Dictionary<IKey, string> _keyImageToSessonKeyMap = new Dictionary<IKey, string>(new KeyEqualityComparer());

        public ServiceProviderUpdater(
                                IStateClientCryptoService clientCryptoService,
                                IAssetsService assetsService,
                                ISchemeResolverService schemeResolverService,
                                IDataAccessService dataAccessService,
                                IIdentityAttributesService identityAttributesService,
                                IGatewayService gatewayService,
                                IStateTransactionsService transactionsService,
                                IProofsValidationService proofsValidationService,
                                IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
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
            _gatewayService = gatewayService;
            _transactionsService = transactionsService;
            _proofsValidationService = proofsValidationService;
            _idenitiesHubContext = idenitiesHubContext;
            _consentManagementService = consentManagementService;
            _universalProofsPool = universalProofsPool;
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
            _electionCommitteeService = electionCommitteeService;
            _logger = loggerService.GetLogger(nameof(ServiceProviderUpdater));

            PipeIn = new ActionBlock<TransactionBase>(async p =>
            {
                if (p == null)
                {
                    _logger.Error($"[{_accountId}]: Obtained NULL packet");
                    return;
                }

                _logger.Info($"[{_accountId}]: Obtained {p.GetType().Name} packet");

                try
                {
                    /*if (p is DocumentSignRecord documentSignRecord)
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
                    }*/

                    if (p is KeyImageCompromisedTransaction compromisedProofs)
                    {
                        ProcessCompromisedProofs(compromisedProofs.KeyImage);
                    }

                    if (p is TransferAssetTransaction transferAsset)
                    {
                        ProcessTransferAsset(transferAsset);
                    }

                    if (p is UniversalStealthTransaction universalTransport)
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

        private async Task ProcessUniversalTransport(UniversalStealthTransaction universalTransport)
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
                            _logger.Error($"[{_accountId}]: Failed request '{fex.Call.Request.Url}' with body '{fex.Call.RequestBody}'");
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
                    case UniversalProofsMission.RelationCreation:
                        ProcessRelationRegistrationRequest(universalProofs);
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

        private void ProcessVote(UniversalProofs proofs) => 
            _electionCommitteeService.UpdatePollSelection(((ElectionCommitteePayload)proofs.Payload).PollId, proofs.Payload as ElectionCommitteePayload);

        private async Task ProcessUniversalProofsAuthentication(RootIssuer rootIssuer, string sessionKey, IKey keyImage)
        {
            await ValidateAssociatedProofs(rootIssuer, ex => 
            {
                _idenitiesHubContext.Clients.Group(_accountId.ToString(CultureInfo.InvariantCulture)).SendAsync("PushAuthorizationFailed", new { Code = 3, ex.Message }).Wait();
                _idenitiesHubContext.Clients.Group(sessionKey).SendAsync("PushSpAuthorizationFailed", new { Code = 3, ex.Message }).Wait();
            }).ConfigureAwait(false);

            SurjectionProof registrationProof = rootIssuer.IssuersAttributes.FirstOrDefault(a => a.Issuer.Equals(rootIssuer.Issuer))?.RootAttribute.CommitmentProof.SurjectionProof;
            (long registrationId, bool isNew) = _proofsValidationService.HandleRegistration(_accountId, rootIssuer.GetRootAttributeProofs().Commitment.Value, registrationProof);

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

        private async Task ValidateAssociatedProofs(RootIssuer rootIssuer, Action<Exception> onValidationFailure = null)
        {
            IEnumerable<SpIdenitityValidation> spIdenitityValidations = _dataAccessService.GetSpIdenitityValidations(_accountId);

            try
            {
                await _proofsValidationService.CheckAssociatedProofs(rootIssuer, ToValidationCriterias(spIdenitityValidations)).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is NoValidationProofsException || ex is ValidationProofFailedException || ex is ValidationProofsWereNotCompleteException)
            {
                onValidationFailure?.Invoke(ex);
                throw;
            }
        }

        public ITargetBlock<TransactionBase> PipeIn { get; set; }
        public ITargetBlock<NotificationBase> PipeInNotifications { get; }

        /*private void ProcessDocumentSignRecord(DocumentSignRecord packet)
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

            if (!CryptoHelper.VerifySurjectionProof(packet.SignerGroupRelationProof, packet.AssetCommitment, documentHash, BitConverter.GetBytes(spDocument.LastChangeRecordHeight)))
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

                byte[] groupAssetId = await _assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_RELATIONGROUP, allowedSigner.GroupIssuer + allowedSigner.GroupName, allowedSigner.GroupIssuer).ConfigureAwait(false);
                byte[] expectedGroupCommitment = CryptoHelper.GetAssetCommitment(groupNameBlindingFactor, groupAssetId);
                if (packet.AllowedGroupCommitment.Equals32(expectedGroupCommitment))
                {
                    _logger.Debug($"[{_accountId}]: Checking allowed signer started");
                    byte[] groupCommitment = await _gatewayService.GetEmployeeRecordGroup(allowedSigner.GroupIssuer.HexStringToByteArray(), packet.SignerGroupRelationProof.AssetCommitments[0]).ConfigureAwait(false);
                    if (groupCommitment != null && CryptoHelper.VerifySurjectionProof(packet.AllowedGroupNameSurjectionProof, packet.AllowedGroupCommitment))
                    {
                        _logger.Debug($"[{_accountId}]: validation of signer succeeded");
                        byte[] diffBF = CryptoHelper.GetDifferentialBlindingFactor(groupNameBlindingFactor, allowedSigner.BlindingFactor);
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

                        signatureGroupProof = CryptoHelper.CreateSurjectionProof(packet.AllowedGroupCommitment, commitments, index, diffBF);
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
        }*/

        /// <summary>
        /// Relation registration: 
        /// - a user sends a commitment that is a registration commitment
        /// - user also sends the content of the root identity 
        /// - service provider must specify in DB that for received root identity should be established a relationship with group X
        /// - ServiceProviderUpdater obtains this information from the DB and creates an on-chain record with the following:
        ///   * Registration Commitment - seems already includes non-blinded group commitment
        ///   * Commitment to the group ID (??)
        ///   * Eligibility proofs (??)
        /// </summary>
        /// <param name="universalProofs"></param>
        private async void ProcessRelationRegistrationRequest(UniversalProofs universalProofs)
        {
            string sessionKey = universalProofs.SessionKey;

            var rootAttributeProofs = universalProofs.GetMainRootIssuer().GetRootAttributeProofs();

            var assetId = await _assetsService.GenerateAssetId(rootAttributeProofs.SchemeName, rootAttributeProofs.CommitmentProof.DidUrls[0], universalProofs.MainIssuer.ToString()).ConfigureAwait(false);
            if (!CryptoHelper.VerifyIssuanceSurjectionProof(rootAttributeProofs.CommitmentProof.SurjectionProof, rootAttributeProofs.Commitment.Value.Span, new byte[][] { assetId }))
            {
                await _idenitiesHubContext.Clients.Group(sessionKey).SendAsync("PushEmployeeIncorrectRegistration", new { Code = 1, Message = "RegistrationCommitment does not match to provided Asset Id" }).ConfigureAwait(false);
                return;
            }

            List<RelationRecord> relationRecords = _dataAccessService.GetRelationRecords(_accountId, rootAttributeProofs.CommitmentProof.DidUrls[0]);

            bool isEligibilityCorrect = await CheckEligibilityProofs(universalProofs.GetMainRootIssuer().GetRootAttributeProofs().Commitment.Value, universalProofs.GetMainRootIssuer().GetRootAttributeProofs().BindingProof, universalProofs.GetMainRootIssuer().Issuer.Value).ConfigureAwait(false);

            if (!isEligibilityCorrect)
            {
                await _idenitiesHubContext.Clients.Group(sessionKey).SendAsync("PushEmployeeIncorrectRegistration", new { Code = 2, Message = "Eligibility proofs were wrong" }).ConfigureAwait(false);
                return;
            }

            await ValidateAssociatedProofs(universalProofs.GetMainRootIssuer(), ex =>
            {
                _idenitiesHubContext.Clients.Group(sessionKey).SendAsync("PushSpAuthorizationFailed", new { Code = 3, ex.Message }).Wait();
            }).ConfigureAwait(false);

            relationRecords.ForEach(async relationRecord =>
            {
                if (relationRecord != null)
                {
                    var registrationCommitment = rootAttributeProofs.CommitmentProof.SurjectionProof.AssetCommitments[0];
                    if (relationRecord.RegistrationCommitment != null && relationRecord.RegistrationCommitment.Commitment.Equals(registrationCommitment.ToHexString()))
                    {
                        // TODO: need to adjust this logic
                        await _idenitiesHubContext.Clients.Group(sessionKey).SendAsync("PushRelationAlreadyRegistered").ConfigureAwait(false);
                        return;
                    }

                    byte[] groupAssetId = await _assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_RELATIONGROUP, _clientCryptoService.PublicKeys[0].ToString() + relationRecord.RelationGroup.GroupName, _clientCryptoService.PublicKeys[0].ToString()).ConfigureAwait(false);

                    byte[] groupNonBlindedCommitment = CryptoHelper.GetNonblindedAssetCommitment(groupAssetId);
                    byte[] commitmentForVerification = CryptoHelper.SubCommitments(registrationCommitment, groupNonBlindedCommitment);
                    if (!CryptoHelper.VerifySurjectionProof(rootAttributeProofs.CommitmentProof.SurjectionProof, commitmentForVerification))
                    {
                        //await _idenitiesHubContext.Clients.Group(sessionKey).SendAsync("PushEmployeeIncorrectRegistration", new { Code = 3, Message = "Group proofs were wrong" }).ConfigureAwait(false);
                        return;
                    }

                    _dataAccessService.SetRelationRegistrationCommitment(_accountId, relationRecord.RelationRecordId, registrationCommitment.ToHexString());
                    await _transactionsService.IssueRelationRecordTransaction(_identityKeyProvider.GetKey(registrationCommitment)).ConfigureAwait(false);
                    await _idenitiesHubContext.Clients.Group(_accountId.ToString(CultureInfo.InvariantCulture)).SendAsync("PushEmployeeUpdate", new EmployeeDto { AssetId = assetId.ToHexString(), RegistrationCommitment = registrationCommitment.ToHexString() }).ConfigureAwait(false);
                    await _idenitiesHubContext.Clients.Group(sessionKey).SendAsync("PushEmployeeRegistration", new { Commitment = registrationCommitment.ToHexString() }).ConfigureAwait(false);
                }
                else
                {
                    await _idenitiesHubContext.Clients.Group(sessionKey).SendAsync("PushEmployeeNotRegistered").ConfigureAwait(false);
                    return;
                }
            });
        }

        private static IEnumerable<ValidationCriteria> ToValidationCriterias(IEnumerable<SpIdenitityValidation> spIdenitityValidations)
        {
            return spIdenitityValidations
                                            .Select(v =>
                                                new ValidationCriteria { SchemeName = v.SchemeName, ValidationType = v.ValidationType, NumericCriterion = v.NumericCriterion, GroupIdCriterion = v.GroupIdCriterion });
        }

        private void ProcessTransferAsset(TransferAssetTransaction transferAsset)
        {
            _clientCryptoService.DecodeEcdhTuple(transferAsset.TransferredAsset.EcdhTuple, null, out byte[] blindingFactor, out byte[] assetId);
            _assetsService.GetAttributeSchemeName(assetId, transferAsset.Source.ToString()).ContinueWith(t =>
            {
                if (t.IsCompleted && !t.IsFaulted)
                {
                    _dataAccessService.StoreSpAttribute(_accountId, t.Result, assetId, transferAsset.Source.Value.ToHexString(), blindingFactor, transferAsset.TransferredAsset.AssetCommitment.ToByteArray(), transferAsset.SurjectionProof.AssetCommitments[0]);

                    _idenitiesHubContext.Clients.Group(_accountId.ToString(CultureInfo.InvariantCulture)).SendAsync("PushAttribute", new SpAttributeDto { SchemeName = t.Result, Source = transferAsset.Source.ArraySegment.Array.ToHexString(), AssetId = assetId.ToHexString(), OriginalBlindingFactor = blindingFactor.ToHexString(), OriginalCommitment = transferAsset.TransferredAsset.AssetCommitment.ToString(), IssuingCommitment = transferAsset.SurjectionProof.AssetCommitments[0].ToHexString(), Validated = false, IsOverriden = false });
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
