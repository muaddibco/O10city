using O10.Client.DataLayer.Services;
using O10.Client.Web.Portal.ElectionCommittee.Models;
using O10.Core.Architecture;
using O10.Core.Identity;
using O10.Core.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using O10.Client.Common.Interfaces;
using O10.Core.Logging;
using O10.Client.Web.Portal.Dtos.IdentityProvider;
using O10.Client.DataLayer.Model;
using O10.Client.Web.Portal.Services.Idps;
using O10.Client.Common.Entities;
using O10.Client.Web.Portal.Exceptions;
using O10.Transactions.Core.Ledgers.O10State;
using O10.Client.DataLayer.AttributesScheme;
using O10.Client.Web.Portal.Services;
using O10.Core.Translators;
using O10.Client.DataLayer.ElectionCommittee;
using O10.Crypto.ConfidentialAssets;
using O10.Core.Cryptography;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using O10.Transactions.Core.Ledgers.O10State.Transactions;

namespace O10.Client.Web.Portal.ElectionCommittee
{
    [RegisterDefaultImplementation(typeof(IElectionCommitteeService), Lifetime = LifetimeManagement.Singleton)]
    public class ElectionCommitteeService : IElectionCommitteeService
    {
        private readonly IDataAccessService _dataAccessService;
        private readonly IAccountsService _accountsService;
        private readonly IAssetsService _assetsService;
        private readonly IIdentityAttributesService _identityAttributesService;
        private readonly IIdentityKeyProvider _identityKeyProvider;
        private readonly ITranslatorsRepository _translatorsRepository;
        private readonly ISchemeResolverService _schemeResolverService;
        private readonly IExecutionContextManager _executionContextManager;
        private readonly ILogger _logger;

        private readonly Dictionary<long, ConcurrentDictionary<IKey, TaskCompletionSource<bool>>> _castedVotes = new Dictionary<long, ConcurrentDictionary<IKey, TaskCompletionSource<bool>>>();

        public ElectionCommitteeService(
            IDataAccessService dataAccessService, 
            IAccountsService accountsService,
            IAssetsService assetsService,
            IIdentityAttributesService identityAttributesService,
            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
            ITranslatorsRepository translatorsRepository,
            ISchemeResolverService schemeResolverService,
            IExecutionContextManager executionContextManager,
            ILoggerService loggerService)
        {
            _dataAccessService = dataAccessService;
            _accountsService = accountsService;
            _assetsService = assetsService;
            _identityAttributesService = identityAttributesService;
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
            _translatorsRepository = translatorsRepository;
            _schemeResolverService = schemeResolverService;
            _executionContextManager = executionContextManager;
            _logger = loggerService.GetLogger(nameof(ElectionCommitteeService));
        }

        public void Initialize()
        {
            var polls = _dataAccessService.GetEcPolls((int)PollState.Started);
            
            foreach (var poll in polls)
            {
                var account = _dataAccessService.GetAccount(poll.AccountId);
                _castedVotes.Add(poll.EcPollRecordId, new ConcurrentDictionary<IKey, TaskCompletionSource<bool>>(new Key32()));
                _executionContextManager.InitializeStateExecutionServices(account.AccountId, account.SecretSpendKey);
            }
        }

        public Candidate AddCandidateToPoll(long pollId, string name)
        {
            IKey assetId = _identityKeyProvider.GetKey(CryptoHelper.GetRandomSeed());

            var id = _dataAccessService.AddCandidateToPoll(pollId, name, assetId.ToString());

            return _translatorsRepository.GetInstance<EcCandidateRecord, Candidate>().Translate(_dataAccessService.GetCandidateRecord(id));
        }

        public async Task IssueVotersRegistrations(long pollId, long issuerAccountId)
        {
            var poll = _dataAccessService.GetEcPoll(pollId);
            var account = _accountsService.GetById(poll.AccountId);
            var accountSource = _accountsService.GetById(issuerAccountId);
            var issuer = account.PublicSpendKey.ToHexString();
            var persistency = _executionContextManager.ResolveExecutionServices(poll.AccountId);
            IEnumerable<AttributeDefinition> attributeDefinitions = _dataAccessService.GetAttributesSchemeByIssuer(issuer, true)
                .Select(a => new AttributeDefinition
                {
                    SchemeId = a.IdentitiesSchemeId,
                    AttributeName = a.AttributeName,
                    SchemeName = a.AttributeSchemeName,
                    Alias = a.Alias,
                    Description = a.Description,
                    IsActive = a.IsActive,
                    IsRoot = a.CanBeRoot
                });

            var rootScheme = await _schemeResolverService.GetRootAttributeScheme(accountSource.PublicSpendKey.ToHexString()).ConfigureAwait(false);

            var identitiesSource = _dataAccessService.GetIdentities(issuerAccountId);
            foreach (var identitySource in identitiesSource)
            {
                try
                {
                    var identityTarget = _dataAccessService.GetIdentityTarget(identitySource.IdentityId);
                    var targetAccount = new ConfidentialAccount
                    {
                        PublicSpendKey = identityTarget.PublicSpendKey.HexStringToByteArray(),
                        PublicViewKey = identityTarget.PublicViewKey.HexStringToByteArray()
                    };
                    var rootAttr = identitySource.Attributes.FirstOrDefault(a => a.AttributeName == rootScheme.AttributeName);

                    List<AttributeIssuanceDetails> attributeIssuances = new List<AttributeIssuanceDetails>
                    {
                        new AttributeIssuanceDetails
                        {
                            Definition = attributeDefinitions.FirstOrDefault(d => d.IsRoot),
                            Value = new IssueAttributesRequestDTO.AttributeValue
                            {
                                Value = rootAttr.Content
                            }
                        }
                    };

                    var identity = CreateIdentityInDb(account, attributeIssuances);

                    var issuanceDetails = await IssueIdpAttributesAsRoot(issuer, targetAccount, identity, attributeIssuances, account, persistency.Scope.ServiceProvider).ConfigureAwait(false);

                }
                catch (Exception ex)
                {
                    _logger.Error("Error during issuing an identity for a voter", ex);
                }
            }
        }

        public Poll RegisterPoll(string name)
        {
            var accountId = _accountsService.Create(DataLayer.Enums.AccountType.IdentityProvider, name, "qqq", true);
            var account = _accountsService.GetById(accountId);
            var issuer = account.PublicSpendKey.ToHexString();

            var attributeDefitions = new List<AttributeDefinition> { 
                new AttributeDefinition
                {
                    IsRoot = true,
                    AttributeName = "VoterNumber",
                    SchemeName = AttributesSchemes.ATTR_SCHEME_NAME_IDCARD,
                    Alias = "Voter Number"
                }
            };

            attributeDefitions.ForEach(a =>
            {
                var schemeId = _dataAccessService.AddAttributeToScheme(issuer, a.AttributeName, a.SchemeName, a.Alias, a.Description);
                if(a.IsRoot)
                {
                    _dataAccessService.ToggleOnRootAttributeScheme(schemeId);
                }
            });


            var id = _dataAccessService.AddPoll(name, accountId);

            return FetchPoll(id);
        }

        public Candidate SetCandidateState(long candidateId, bool isActive)
        {
            _dataAccessService.SetCandidateStatus(candidateId, isActive);
            return _translatorsRepository.GetInstance<EcCandidateRecord, Candidate>().Translate(_dataAccessService.GetCandidateRecord(candidateId));
        }

        public Poll SetPollState(long pollId, PollState pollState)
        {
            if(pollState == PollState.Cancelled || pollState == PollState.Completed)
            {
                _castedVotes.Remove(pollId);
            } 
            else if(pollState == PollState.Started)
            {
                _castedVotes.Add(pollId, new ConcurrentDictionary<IKey, TaskCompletionSource<bool>>(new Key32()));
            }

            _dataAccessService.SetPollState(pollId, (int)pollState);
            return FetchPoll(pollId);
        }

        public Poll GetPoll(long pollId)
        {
            return FetchPoll(pollId);
        }

        private Poll FetchPoll(long pollId)
        {
            var pollDb = _dataAccessService.GetEcPoll(pollId, true);
            return EnrichPollWithAccount(pollDb);
        }

        private Poll EnrichPollWithAccount(EcPollRecord pollDb)
        {
            var poll = _translatorsRepository.GetInstance<EcPollRecord, Poll>().Translate(pollDb);
            poll.Issuer = _accountsService.GetById(pollDb.AccountId).PublicSpendKey.ToHexString();
            return poll;
        }

        public List<Poll> GetPolls(PollState? pollState)
        {
            if(pollState.HasValue)
            {
                return _dataAccessService.GetEcPolls((int)pollState)?.Select(p => EnrichPollWithAccount(p)).ToList();
            }

            return _dataAccessService.GetEcPolls()?.Select(p => EnrichPollWithAccount(p)).ToList();
        }

        public SignedEcCommitment GenerateDerivedCommitment(long pollId, SelectionCommitmentRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            EcPollRecord poll = _dataAccessService.GetEcPoll(pollId);
            byte[][] assetIds = poll.Candidates.Select(c => c.AssetId.HexStringToByteArray()).ToArray();

            bool res1 = CryptoHelper.VerifySurjectionProof(request.CandidateCommitmentProofs, request.Commitment);
            if(!res1)
            {
                throw new ArgumentException("Verification to candidate commitments failed");
            }

            foreach (var candidateCommitment in request.CandidateCommitments)
            {
                bool res2 = CryptoHelper.VerifyIssuanceSurjectionProof(candidateCommitment.IssuanceProof, candidateCommitment.Commitment, assetIds);
                if(!res2)
                {
                    throw new ArgumentException($"Verification of candidate's {candidateCommitment.Commitment.ToHexString()} issuance proof failed");
                }
            }

            byte[] ecBlindingFactor = CryptoHelper.GetRandomSeed();
            byte[] ecCommitment = CryptoHelper.BlindAssetCommitment(request.Commitment, ecBlindingFactor);

            _dataAccessService.AddPollSelection(pollId, ecCommitment.ToHexString(), ecBlindingFactor.ToHexString());

            var persistency = _executionContextManager.ResolveExecutionServices(poll.AccountId);
            var clientCryptoService = persistency.Scope.ServiceProvider.GetService<IStateClientCryptoService>();
            var signature = clientCryptoService.Sign(ecCommitment);

            return new SignedEcCommitment
            {
                EcCommitment = ecCommitment,
                Signature = signature
            };
        }

        public SurjectionProof CalculateEcCommitmentProof(long pollId, EcSurjectionProofRequest proofRequest)
        {
            if (proofRequest is null)
            {
                throw new ArgumentNullException(nameof(proofRequest));
            }

            string ecCommitment = proofRequest.EcCommitment.ToHexString();
            var pollSelection = _dataAccessService.GetPollSelection(pollId, ecCommitment);
            byte[] ecBf = pollSelection.EcBlindingFactor.HexStringToByteArray();
            byte[] bf = CryptoHelper.SumScalars(ecBf, proofRequest.PartialBlindingFactor);
            var sp = CryptoHelper.CreateSurjectionProof(proofRequest.EcCommitment, proofRequest.CandidateCommitments, proofRequest.Index, bf);

            return sp;
        }

        public void UpdatePollSelection(long pollId, ElectionCommitteePayload payload)
        {
            if (payload is null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            var voteCastTask = _castedVotes[pollId].GetOrAdd(payload.EcCommitment, new TaskCompletionSource<bool>());
            voteCastTask.SetResult(true);

            _dataAccessService.UpdatePollSelection(pollId, payload.EcCommitment.ToString(), payload.PartialBf.ToHexString());
        }

        public async Task<bool> WaitForVoteCast(long pollId, IKey ecCommitment)
        {
            if(!_castedVotes.ContainsKey(pollId))
            {
                throw new ArgumentException("Poll not started yet");
            }

            var voteCastTask = _castedVotes[pollId].GetOrAdd(ecCommitment, new TaskCompletionSource<bool>());

            try
            {
                var result = await voteCastTask.Task.TimeoutAfter(60000).ConfigureAwait(false);
                return result;
            }
            catch (TimeoutException)
            {
                return false;
            }
        }

        public IEnumerable<PollResult> CalculateResults(long pollId)
        {
            var poll = _dataAccessService.GetEcPoll(pollId, true, true);
            Dictionary<byte[], PollResult> votes = new Dictionary<byte[], PollResult>(new Byte32EqualityComparer());

            foreach (var candidate in poll.Candidates)
            {
                byte[] assetId = candidate.AssetId.HexStringToByteArray();
                byte[] commitment = CryptoHelper.GetNonblindedAssetCommitment(assetId);
                votes.Add(commitment, new PollResult
                { 
                    Candidate = _translatorsRepository.GetInstance<EcCandidateRecord, Candidate>().Translate(candidate),
                    Votes = 0
                });
            }

            foreach (var vote in poll.PollSelections.Where(s => !string.IsNullOrEmpty(s.VoterBlindingFactor)))
            {
                byte[] ecCommitment = vote.EcCommitment.HexStringToByteArray();
                byte[] ecBf = vote.EcBlindingFactor.HexStringToByteArray();
                byte[] voterBf = vote.VoterBlindingFactor.HexStringToByteArray();
                byte[] bf = CryptoHelper.SumScalars(ecBf, voterBf);
                byte[] blindingPoint = CryptoHelper.GetPublicKey(bf);
                byte[] commitment = CryptoHelper.SubCommitments(ecCommitment, blindingPoint);

                if(votes.ContainsKey(commitment))
                {
                    votes[commitment].Votes++;
                }
            }

            return votes.Select(d => d.Value);
        }

        #region Private Functions

        private Identity CreateIdentityInDb(AccountDescriptor account, IEnumerable<AttributeIssuanceDetails> issuanceInputDetails)
        {
            var rootAttributeDetails = issuanceInputDetails.First(a => a.Definition.IsRoot);
            Identity identity = _dataAccessService.GetIdentityByAttribute(account.AccountId, rootAttributeDetails.Definition.AttributeName, rootAttributeDetails.Value.Value);
            if (identity == null)
            {
                _dataAccessService.CreateIdentity(account.AccountId,
                                   rootAttributeDetails.Value.Value,
                                   issuanceInputDetails.Select(d => (d.Definition.AttributeName, d.Value.Value)).ToArray());
                identity = _dataAccessService.GetIdentityByAttribute(account.AccountId, rootAttributeDetails.Definition.AttributeName, rootAttributeDetails.Value.Value);
            }

            return identity;
        }

        async Task<IssuanceDetailsDto> IssueIdpAttributesAsRoot(
            string issuer,
            ConfidentialAccount confidentialAccount,
            Identity identity,
            IEnumerable<AttributeIssuanceDetails> attributeIssuanceDetails,
            AccountDescriptor account,
            IServiceProvider serviceProvider)
        {
            IssuanceDetailsDto issuanceDetails = new IssuanceDetailsDto();

            IEnumerable<IdentitiesScheme> identitiesSchemes = _dataAccessService.GetAttributesSchemeByIssuer(issuer, true);

            var rootAttributeDetails = attributeIssuanceDetails.First(a => a.Definition.IsRoot);
            var transactionsService = serviceProvider.GetService<IStateTransactionsService>();

            byte[] rootAssetId = await _assetsService.GenerateAssetId(rootAttributeDetails.Definition.SchemeName, rootAttributeDetails.Value.Value, issuer).ConfigureAwait(false);
            IdentityAttribute rootAttribute = identity.Attributes.FirstOrDefault(a => a.AttributeName == rootAttributeDetails.Definition.AttributeName);
            var issueBlindedAsset = await transactionsService.IssueBlindedAsset(rootAssetId).ConfigureAwait(false);
            _dataAccessService.UpdateIdentityAttributeCommitment(rootAttribute.AttributeId, issueBlindedAsset.AssetCommitment);
                issuanceDetails.AssociatedAttributes
                    = await IssueAssociatedAttributes(
                        attributeIssuanceDetails.Where(a => !a.Definition.IsRoot)
                            .ToDictionary(d => identity.Attributes.First(a => a.AttributeName == d.Definition.AttributeName).AttributeId, d => d),
                        transactionsService,
                        issuer, rootAssetId).ConfigureAwait(false);

            var transferAsset = await transactionsService.TransferAssetToStealth(rootAssetId, confidentialAccount).ConfigureAwait(false);

            if (transferAsset == null)
            {
                _logger.Error($"[{account.AccountId}]: failed to transfer Root Attribute");
                throw new RootAttributeTransferFailedException();
            }

            issuanceDetails.RootAttribute = new IssuanceDetailsDto.IssuanceDetailsRoot
            {
                AttributeName = rootAttribute.AttributeName,
                OriginatingCommitment = transferAsset.SurjectionProof.AssetCommitments[0].ToHexString(),
                AssetCommitment = transferAsset.TransferredAsset.AssetCommitment.ToString(),
                SurjectionProof = $"{transferAsset.SurjectionProof.Rs.E.ToHexString()}{transferAsset.SurjectionProof.Rs.S[0].ToHexString()}"
            };

            return issuanceDetails;
        }
        private async Task<IEnumerable<IssuanceDetailsDto.IssuanceDetailsAssociated>> IssueAssociatedAttributes(Dictionary<long, AttributeIssuanceDetails> attributes, IStateTransactionsService transactionsService, string issuer, byte[] rootAssetId = null)
        {
            List<IssuanceDetailsDto.IssuanceDetailsAssociated> issuanceDetails = new List<IssuanceDetailsDto.IssuanceDetailsAssociated>();

            if (attributes.Any(kv => kv.Value.Definition.IsRoot))
            {
                var rootKv = attributes.FirstOrDefault(kv => kv.Value.Definition.IsRoot);
                var packet = await IssueAssociatedAttribute(rootKv.Value.Definition.SchemeName, rootKv.Value.Value.Value, rootKv.Value.Value.BlindingPointValue, rootKv.Value.Value.BlindingPointRoot, issuer, transactionsService).ConfigureAwait(false);
                _dataAccessService.UpdateIdentityAttributeCommitment(rootKv.Key, packet.AssetCommitment);
                issuanceDetails.Add(new IssuanceDetailsDto.IssuanceDetailsAssociated
                {
                    AttributeName = rootKv.Value.Definition.AttributeName,
                    AssetCommitment = packet.AssetCommitment.ToString(),
                    BindingToRootCommitment = packet.RootAssetCommitment.ToString()
                });
                rootAssetId = _assetsService.GenerateAssetId(rootKv.Value.Definition.SchemeId, rootKv.Value.Value.Value);
            }

            if (rootAssetId == null)
            {
                throw new ArgumentException("Either rootAssetId must be provided outside or one of attributes must be root one");
            }

            foreach (var kv in attributes.Where(a => !a.Value.Definition.IsRoot))
            {
                byte[] rootCommitment = _assetsService.GetCommitmentBlindedByPoint(rootAssetId, kv.Value.Value.BlindingPointRoot);

                var packet = await IssueAssociatedAttribute(kv.Value.Definition.SchemeName, kv.Value.Value.Value, kv.Value.Value.BlindingPointValue, rootCommitment, issuer, transactionsService).ConfigureAwait(false);
                issuanceDetails.Add(new IssuanceDetailsDto.IssuanceDetailsAssociated
                {
                    AttributeName = kv.Value.Definition.AttributeName,
                    AssetCommitment = packet.AssetCommitment.ToString(),
                    BindingToRootCommitment = packet.RootAssetCommitment.ToString()
                });
                _dataAccessService.UpdateIdentityAttributeCommitment(kv.Key, packet.AssetCommitment);
            }

            return issuanceDetails;
        }

        private async Task<IssueAssociatedBlindedAssetTransaction> IssueAssociatedAttribute(string schemeName,
                                                      string content,
                                                      byte[] blindingPointValue,
                                                      byte[] blindingPointRoot,
                                                      string issuer,
                                                      IStateTransactionsService transactionsService)
        {
            byte[] assetId = await _assetsService.GenerateAssetId(schemeName, content, issuer).ConfigureAwait(false);
            
            return await transactionsService.IssueAssociatedAsset(assetId, blindingPointValue, blindingPointRoot).ConfigureAwait(false);
        }

        #endregion Private Functions
    }
}
