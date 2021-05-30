using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.Ledgers.Stealth.Internal;
using O10.Client.Common.Dtos.UniversalProofs;
using O10.Client.Common.Identities;
using O10.Client.Common.Interfaces;
using O10.Client.Common.Interfaces.Inputs;
using O10.Core.Architecture;
using O10.Core.Cryptography;
using O10.Core.HashCalculations;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Crypto.ConfidentialAssets;
using O10.Core.Notifications;
using O10.Transactions.Core.DTOs;
using O10.Crypto.Models;
using System.Linq;
using O10.Transactions.Core.Ledgers.Stealth.Transactions;
using O10.Client.Common.Communication.LedgerWriters;
using O10.Client.DataLayer.Model;
using O10.Client.DataLayer.Services;
using O10.Core.ExtensionMethods;

namespace O10.Client.Common.Communication
{
    [RegisterDefaultImplementation(typeof(IStealthTransactionsService), Lifetime = LifetimeManagement.Scoped)]
    public class StealthTransactionsService : TransactionsServiceBase, IStealthTransactionsService
    {
        private IStealthClientCryptoService _clientCryptoService;
        private IBoundedAssetsService _relationsBindingService;
        private readonly IEligibilityProofsProvider _eligibilityProofsProvider;
        private readonly IDataAccessService _dataAccessService;

        public StealthTransactionsService(IHashCalculationsRepository hashCalculationsRepository,
                                          IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
                                          IStealthClientCryptoService clientCryptoService,
                                          IBoundedAssetsService relationsBindingService,
                                          IEligibilityProofsProvider eligibilityProofsProvider,
                                          IDataAccessService dataAccessService,
                                          IGatewayService gatewayService,
                                          ILoggerService loggerService)
            : base(hashCalculationsRepository,
                   identityKeyProvidersRegistry,
                   clientCryptoService,
                   gatewayService,
                   loggerService)
        {
            _clientCryptoService = clientCryptoService;
            _relationsBindingService = relationsBindingService;
            _eligibilityProofsProvider = eligibilityProofsProvider;
            _dataAccessService = dataAccessService;
        }

        public IKey NextKeyImage { get; private set; }

        #region Public Functions

        public void Initialize(long accountId)
        {
            _accountId = accountId;
        }

        public async Task<RequestResult> SendRelationsProofs(RelationsProofsInput relationsProofsInput, AssociatedProofPreparation[] associatedProofPreparations, OutputSources[] outputModels, byte[][] issuanceCommitments)
        {
            var packet = CreateRelationsProofs(relationsProofsInput, associatedProofPreparations, outputModels, issuanceCommitments);

            NextKeyImage = packet.KeyImage;

            var completionResult = PropagateTransaction(packet);

            var requestResult = new RequestResult
            {
                NewCommitment = packet.AssetCommitment,
                NewTransactionKey = packet.TransactionPublicKey,
                NewDestinationKey = packet.DestinationKey,
                Result = await completionResult.Task.ConfigureAwait(false) is SucceededNotification
            };

            return requestResult;
        }


        public async Task<RequestResult> SendDocumentSignRequest(DocumentSignRequestInput requestInput, AssociatedProofPreparation[] associatedProofPreparations, OutputSources[] outputModels, byte[][] issuanceCommitments)
        {
            var packet = CreateDocumentSignRequest(requestInput, associatedProofPreparations, outputModels, issuanceCommitments);

            NextKeyImage = packet.KeyImage;

            var completionResult = PropagateTransaction(packet);

            RequestResult requestResult = new RequestResult
            {
                NewCommitment = packet.AssetCommitment,
                NewTransactionKey = packet.TransactionPublicKey,
                NewDestinationKey = packet.DestinationKey,
                Result = await completionResult.Task.ConfigureAwait(false) is SucceededNotification
            };

            return requestResult;
        }

        public async Task<RequestResult> SendEmployeeRegistrationRequest(EmployeeRequestInput requestInput, AssociatedProofPreparation[] associatedProofPreparations, OutputSources[] outputModels, byte[][] issuanceCommitments)
        {
            var packet = CreateEmployeeRegistrationRequest(requestInput, associatedProofPreparations, outputModels, issuanceCommitments);

            NextKeyImage = packet.KeyImage;

            var completionResult = PropagateTransaction(packet);

            var requestResult = new RequestResult
            {
                NewCommitment = packet.AssetCommitment,
                NewTransactionKey = packet.TransactionPublicKey,
                NewDestinationKey = packet.DestinationKey,
                Result = await completionResult.Task.ConfigureAwait(false) is SucceededNotification
            };

            return requestResult;
        }

        public async Task<RequestResult> SendIdentityProofs(RequestInput requestInput, AssociatedProofPreparation[] associatedProofPreparations, OutputSources[] outputModels, byte[] issuer)
        {
            var packet = await CreateIdentityProofsPacket(requestInput, associatedProofPreparations, outputModels, issuer).ConfigureAwait(false);

            NextKeyImage = packet.KeyImage;

            var completionResult = PropagateTransaction(packet);

            var requestResult = new RequestResult
            {
                NewCommitment = packet.AssetCommitment,
                NewTransactionKey = packet.TransactionPublicKey,
                NewDestinationKey = packet.DestinationKey,
                Result = await completionResult.Task.ConfigureAwait(false) is SucceededNotification
            };

            return requestResult;
        }

        public async Task<RequestResult> SendRevokeIdentity(RequestInput requestInput, OutputSources[] outputModels, byte[][] issuanceCommitments)
        {
            RevokeIdentity packet = CreateRevokeIdentityPacket(requestInput, outputModels, issuanceCommitments);

            NextKeyImage = packet.KeyImage;

            var completionResult = PropagateTransaction(packet);

            RequestResult requestResult = new RequestResult
            {
                NewCommitment = packet.AssetCommitment,
                NewTransactionKey = packet.TransactionPublicKey,
                NewDestinationKey = packet.DestinationKey,
                Result = await completionResult.Task.ConfigureAwait(false) is SucceededNotification
            };

            return requestResult;
        }

        public async Task<RequestResult> SendCompromisedProofs(RequestInput requestInput, byte[] compromisedKeyImage, byte[] compromisedTransactionKey, byte[] destinationKey, OutputSources[] outputModels, byte[][] issuanceCommitments)
        {
            var packet = CreateTransitionCompromisedProofs(requestInput, compromisedKeyImage, compromisedTransactionKey, destinationKey, outputModels, issuanceCommitments);

            NextKeyImage = packet.KeyImage;

            var completionResult = PropagateTransaction(packet);

            RequestResult requestResult = new RequestResult
            {
                NewCommitment = packet.AssetCommitment,
                NewTransactionKey = packet.TransactionPublicKey,
                NewDestinationKey = packet.DestinationKey,
                Result = await completionResult.Task.ConfigureAwait(false) is SucceededNotification
            };

            return requestResult;
        }

        public async Task<RequestResult> SendUniversalTransport([NotNull] RequestInput requestInput, [NotNull] OutputSources[] outputModels, UniversalProofs universalProofs)
        {
            if (requestInput is null)
            {
                throw new ArgumentNullException(nameof(requestInput));
            }

            if (outputModels is null)
            {
                throw new ArgumentNullException(nameof(outputModels));
            }

            if (universalProofs is null)
            {
                throw new ArgumentNullException(nameof(universalProofs));
            }

            var transaction = CreateUniversalTransportPacket(requestInput, outputModels, universalProofs);

            var completionSource = PropagateTransaction(
                transaction,
                new StealthPropagationArgument(
                    _identityKeyProvider.GetKey(requestInput.PrevDestinationKey),
                    _identityKeyProvider.GetKey(requestInput.PrevTransactionKey),
                    p =>
                    {
                        if (p is O10StealthTransactionBase o10StealthTransaction)
                        {
                            universalProofs.KeyImage = o10StealthTransaction.KeyImage;
                            string proofs = JsonConvert.SerializeObject(universalProofs);
                            byte[] proofsBytes = Encoding.UTF8.GetBytes(proofs);
                            o10StealthTransaction.ProofsHash = _hashCalculation.CalculateHash(proofsBytes);
                        }
                    }));

            var result = await completionSource.Task.ConfigureAwait(false);
            if(result is SucceededNotification)
            {
                NextKeyImage = transaction.KeyImage;
                _dataAccessService.AddUserTransactionSecret(_accountId, transaction.KeyImage.ToString(), universalProofs.MainIssuer.ToString(), requestInput.AssetId.ToHexString());
            }

            RequestResult requestResult = new RequestResult
            {
                KeyImage = transaction.KeyImage.Value,
                NewBlindingFactor = requestInput.BlindingFactor,
                NewCommitment = transaction.AssetCommitment.Value,
                NewTransactionKey = transaction.TransactionPublicKey.Value,
                NewDestinationKey = transaction.DestinationKey.Value,
                Result = result is SucceededNotification
            };

            return requestResult;
        }

        public UserTransactionSecrets PopLastTransactionSecrets()
        {
            var secrets = _dataAccessService.GetUserTransactionSecrets(_accountId, NextKeyImage.ToString());
            _dataAccessService.RemoveUserTransactionSecret(_accountId, NextKeyImage.ToString());
            return secrets;
        }

        #endregion Public Functions

        #region Private Functions

        private GroupsRelationsProofs CreateRelationsProofs(RelationsProofsInput requestInput, AssociatedProofPreparation[] associatedProofPreparations, OutputSources[] outputModels, byte[][] issuanceCommitments)
        {
            byte[] secretKey = CryptoHelper.GetRandomSeed();
            byte[] transactionKey = CryptoHelper.GetPublicKey(secretKey);
            byte[] destinationKey = CryptoHelper.GetDestinationKey(secretKey, _clientCryptoService.PublicKeys[0].ArraySegment.Array, _clientCryptoService.PublicKeys[1].ArraySegment.Array);
            byte[] destinationKey2 = CryptoHelper.GetDestinationKey(secretKey, requestInput.PublicSpendKey, requestInput.PublicViewKey);
            byte[] blindingFactor = CryptoHelper.GetRandomSeed();
            byte[] assetCommitment = CryptoHelper.GetAssetCommitment(blindingFactor, requestInput.AssetId);


            byte[] onboardingToOwnershipBlindingFactor = CryptoHelper.GetDifferentialBlindingFactor(blindingFactor, requestInput.PrevBlindingFactor);
            GetAssetCommitmentsRing(requestInput.PrevAssetCommitment, outputModels, out int pos, out IKey[] assetCommitments);

            byte[] onboardingToEligibilityBlindingFactor = CryptoHelper.GetDifferentialBlindingFactor(blindingFactor, requestInput.EligibilityBlindingFactor);
            _eligibilityProofsProvider.GetEligibilityCommitmentAndProofs(requestInput.EligibilityCommitment, issuanceCommitments, out int actualAssetPos, out byte[][] commitments);
            SurjectionProof eligibilityProof = CryptoHelper.CreateSurjectionProof(assetCommitment, commitments, actualAssetPos, onboardingToEligibilityBlindingFactor);

            GroupRelationProof[] groupRelationProofs = new GroupRelationProof[requestInput.Relations.Length];
            int i = 0;
            foreach (var relation in requestInput.Relations)
            {
                _relationsBindingService.GetBoundedCommitment(requestInput.AssetId, relation.RelatedAssetOwner, out byte[] groupEntryBlindingFactor, out byte[] groupEntryCommitment, relation.RelatedAssetId);
                byte[] diffBF = CryptoHelper.GetDifferentialBlindingFactor(blindingFactor, groupEntryBlindingFactor);
                SurjectionProof groupEntryProof = CryptoHelper.CreateSurjectionProof(assetCommitment, new byte[][] { groupEntryCommitment }, 0, diffBF);
                _relationsBindingService.GetBoundedCommitment(relation.RelatedAssetId, relation.RelatedAssetOwner, out byte[] groupNameBlindingFactor, out byte[] groupNameCommitment, relation.RelatedAssetId);
                SurjectionProof groupNameProof = CryptoHelper.CreateNewIssuanceSurjectionProof(groupNameCommitment, new byte[][] { relation.RelatedAssetId }, 0, groupNameBlindingFactor);

                GroupRelationProof groupRelationProof = new GroupRelationProof
                {
                    GroupOwner = relation.RelatedAssetOwner,
                    RelationProof = groupEntryProof,
                    GroupNameProof = groupNameProof
                };

                groupRelationProofs[i++] = groupRelationProof;
            }

            AssociatedProofs[] associatedProofs = GetAssociatedProofs(associatedProofPreparations, blindingFactor, assetCommitment);

            GroupsRelationsProofs block = new GroupsRelationsProofs
            {
                DestinationKey = destinationKey,
                DestinationKey2 = destinationKey2,
                TransactionPublicKey = transactionKey,
                AssetCommitment = assetCommitment,
                EligibilityProof = eligibilityProof,
                RelationProofs = groupRelationProofs,
                AssociatedProofs = associatedProofs,
                BiometricProof = requestInput.BiometricProof,
                EcdhTuple = new EcdhTupleProofs { Mask = new byte[32], AssetId = requestInput.ImageHash, AssetIssuer = requestInput.Issuer, Payload = requestInput.Payload } // ConfidentialAssetsHelper.CreateEcdhTupleProofs(relationsProofsInput.Payload, relationsProofsInput.ImageHash, secretKey, relationsProofsInput.PublicViewKey)
            };

            FillSyncData(block);
            FillAndSign(block, new StealthSignatureInput(requestInput.PrevTransactionKey, assetCommitments, pos));

            return block;
        }

        private DocumentSignRequest CreateDocumentSignRequest(DocumentSignRequestInput requestInput, AssociatedProofPreparation[] associatedProofPreparations, OutputSources[] outputModels, byte[][] issuanceCommitments)
        {
            byte[] secretKey = CryptoHelper.GetRandomSeed();
            byte[] transactionKey = CryptoHelper.GetPublicKey(secretKey);
            byte[] destinationKey = CryptoHelper.GetDestinationKey(secretKey, _clientCryptoService.PublicKeys[0].ArraySegment.Array, _clientCryptoService.PublicKeys[1].ArraySegment.Array);
            _relationsBindingService.GetBoundedCommitment(requestInput.AssetId, requestInput.PublicSpendKey, out byte[] blindingFactor, out byte[] assetCommitment, requestInput.DocumentHash);

            byte[] onboardingToOwnershipBlindingFactor = CryptoHelper.GetDifferentialBlindingFactor(blindingFactor, requestInput.PrevBlindingFactor);
            GetAssetCommitmentsRing(requestInput.PrevAssetCommitment, outputModels, out int pos, out IKey[] assetCommitments);
            SurjectionProof ownershipProof = CryptoHelper.CreateSurjectionProof(assetCommitment, assetCommitments.Select(s => s.Value).ToArray(), pos, onboardingToOwnershipBlindingFactor);

            byte[] onboardingToEligibilityBlindingFactor = CryptoHelper.GetDifferentialBlindingFactor(blindingFactor, requestInput.EligibilityBlindingFactor);
            _eligibilityProofsProvider.GetEligibilityCommitmentAndProofs(requestInput.EligibilityCommitment, issuanceCommitments, out int actualAssetPos, out byte[][] commitments);
            SurjectionProof eligibilityProof = CryptoHelper.CreateSurjectionProof(assetCommitment, commitments, actualAssetPos, onboardingToEligibilityBlindingFactor);

            AssociatedProofs[] associatedProofs = GetAssociatedProofs(associatedProofPreparations, blindingFactor, assetCommitment);

            _relationsBindingService.GetBoundedCommitment(requestInput.AssetId, requestInput.GroupIssuer, out byte[] groupBlindingFactor, out byte[] groupEntryAssetCommitment, requestInput.GroupAssetId);
            byte[] signToGroupEntryBlindingFactor = CryptoHelper.GetDifferentialBlindingFactor(blindingFactor, groupBlindingFactor);

            SurjectionProof groupEntrySurjectionProof = CryptoHelper.CreateSurjectionProof(assetCommitment, new byte[][] { groupEntryAssetCommitment }, 0, signToGroupEntryBlindingFactor, requestInput.DocumentHash, BitConverter.GetBytes(requestInput.DocumentRecordHeight));

            _relationsBindingService.GetBoundedCommitment(requestInput.GroupAssetId, requestInput.GroupIssuer, out byte[] groupNameBlindingFactor, out byte[] groupNameCommitment, requestInput.GroupAssetId);

            byte[] allowedGroupNameBlindingFactor = CryptoHelper.GetRandomSeed();
            byte[] allowedGroupNameCommitment = CryptoHelper.GetAssetCommitment(allowedGroupNameBlindingFactor, requestInput.GroupAssetId);
            byte[] diffAllowedGroupNameBlindingFactor = CryptoHelper.GetDifferentialBlindingFactor(allowedGroupNameBlindingFactor, groupNameBlindingFactor);
            SurjectionProof allowedGroupNameSurjectionProof = CryptoHelper.CreateSurjectionProof(allowedGroupNameCommitment, new byte[][] { groupNameCommitment }, 0, diffAllowedGroupNameBlindingFactor);

            DocumentSignRequest block = new DocumentSignRequest
            {
                DestinationKey = destinationKey,
                DestinationKey2 = requestInput.PublicSpendKey,
                TransactionPublicKey = transactionKey,
                AssetCommitment = assetCommitment,
                OwnershipProof = ownershipProof,
                EligibilityProof = eligibilityProof,
                SignerGroupRelationProof = groupEntrySurjectionProof,
                AllowedGroupCommitment = allowedGroupNameCommitment,
                AllowedGroupNameSurjectionProof = allowedGroupNameSurjectionProof,
                BiometricProof = requestInput.BiometricProof,

                EcdhTuple = new EcdhTupleProofs { Mask = allowedGroupNameBlindingFactor, AssetId = requestInput.DocumentHash, AssetIssuer = requestInput.Issuer, Payload = requestInput.Payload } // ConfidentialAssetsHelper.CreateEcdhTupleProofs(blindingFactor, assetId, issuer, payload, secretKey, target)
            };

            FillSyncData(block);
            FillAndSign(block, new StealthSignatureInput(requestInput.PrevTransactionKey, assetPubs, pos));

            return block;
        }

        private EmployeeRegistrationRequest CreateEmployeeRegistrationRequest(EmployeeRequestInput requestInput, AssociatedProofPreparation[] associatedProofPreparations, OutputSources[] outputModels, byte[][] issuanceCommitments)
        {
            byte[] secretKey = CryptoHelper.GetRandomSeed();
            byte[] transactionKey = CryptoHelper.GetPublicKey(secretKey);
            byte[] destinationKey = CryptoHelper.GetDestinationKey(secretKey, _clientCryptoService.PublicKeys[0].ArraySegment.Array, _clientCryptoService.PublicKeys[1].ArraySegment.Array);
            _relationsBindingService.GetBoundedCommitment(requestInput.AssetId, requestInput.PublicSpendKey, out byte[] blindingFactor, out byte[] assetCommitment, requestInput.GroupAssetId);

            byte[] onboardingToOwnershipBlindingFactor = CryptoHelper.GetDifferentialBlindingFactor(blindingFactor, requestInput.PrevBlindingFactor);
            GetAssetCommitmentsRing(requestInput.PrevAssetCommitment, outputModels, out int pos, out IKey[] assetCommitments);
            SurjectionProof ownershipProof = CryptoHelper.CreateSurjectionProof(assetCommitment, assetCommitments.Select(s => s.Value).ToArray(), pos, onboardingToOwnershipBlindingFactor);

            byte[] onboardingToEligibilityBlindingFactor = CryptoHelper.GetDifferentialBlindingFactor(blindingFactor, requestInput.EligibilityBlindingFactor);
            _eligibilityProofsProvider.GetEligibilityCommitmentAndProofs(requestInput.EligibilityCommitment, issuanceCommitments, out int actualAssetPos, out byte[][] commitments);
            SurjectionProof eligibilityProof = CryptoHelper.CreateSurjectionProof(assetCommitment, commitments, actualAssetPos, onboardingToEligibilityBlindingFactor);

            AssociatedProofs[] associatedProofs = GetAssociatedProofs(associatedProofPreparations, blindingFactor, assetCommitment);

            _relationsBindingService.GetBoundedCommitment(requestInput.GroupAssetId, requestInput.PublicSpendKey, out byte[] groupBlindingFactor, out byte[] groupCommitment, requestInput.GroupAssetId);

            SurjectionProof groupSurjectionProof = CryptoHelper.CreateNewIssuanceSurjectionProof(groupCommitment, new byte[][] { requestInput.GroupAssetId }, 0, groupBlindingFactor);

            EmployeeRegistrationRequest block = new EmployeeRegistrationRequest
            {
                DestinationKey = destinationKey,
                DestinationKey2 = requestInput.PublicSpendKey,
                TransactionPublicKey = transactionKey,
                AssetCommitment = assetCommitment,
                OwnershipProof = ownershipProof,
                EligibilityProof = eligibilityProof,
                AssociatedProofs = associatedProofs,
                GroupCommitment = groupCommitment,
                GroupSurjectionProof = groupSurjectionProof,
                BiometricProof = requestInput.BiometricProof,
                EcdhTuple = new EcdhTupleProofs { Mask = blindingFactor, AssetId = requestInput.AssetId, AssetIssuer = requestInput.Issuer, Payload = requestInput.Payload } // ConfidentialAssetsHelper.CreateEcdhTupleProofs(blindingFactor, assetId, issuer, payload, secretKey, target)
            };

            FillSyncData(block);
            FillAndSign(block, new StealthSignatureInput(requestInput.PrevTransactionKey, assetCommitments, pos));

            return block;
        }

        private RevokeIdentity CreateRevokeIdentityPacket(RequestInput requestInput, OutputSources[] outputModels, byte[][] issuanceCommitments)
        {
            byte[] secretKey = CryptoHelper.GetRandomSeed();
            byte[] transactionKey = CryptoHelper.GetPublicKey(secretKey);
            byte[] destinationKey = CryptoHelper.GetDestinationKey(secretKey, _clientCryptoService.PublicKeys[0].ArraySegment.Array, _clientCryptoService.PublicKeys[1].ArraySegment.Array);
            byte[] destinationKey2 = requestInput.PublicViewKey != null ? CryptoHelper.GetDestinationKey(secretKey, requestInput.PublicSpendKey, requestInput.PublicViewKey) : requestInput.PublicSpendKey;
            byte[] blindingFactor = CryptoHelper.GetRandomSeed();
            byte[] blindingFactorToOwnership = CryptoHelper.GetDifferentialBlindingFactor(blindingFactor, requestInput.PrevBlindingFactor);
            byte[] blindingFactorToEligibility = CryptoHelper.GetDifferentialBlindingFactor(blindingFactor, requestInput.EligibilityBlindingFactor);
            byte[] assetCommitment = CryptoHelper.GetAssetCommitment(blindingFactor, requestInput.AssetId);

            GetAssetCommitmentsRing(requestInput.PrevAssetCommitment, outputModels, out int pos, out IKey[] assetCommitments);
            SurjectionProof ownershipProof = CryptoHelper.CreateSurjectionProof(assetCommitment, assetCommitments.Select(s => s.Value).ToArray(), pos, blindingFactorToOwnership);

            _eligibilityProofsProvider.GetEligibilityCommitmentAndProofs(requestInput.EligibilityCommitment, issuanceCommitments, out int actualAssetPos, out byte[][] commitments);
            SurjectionProof eligibilityProof = CryptoHelper.CreateSurjectionProof(assetCommitment, commitments, actualAssetPos, blindingFactorToEligibility);

            RevokeIdentity block = new RevokeIdentity
            {
                DestinationKey = destinationKey,
                DestinationKey2 = destinationKey2,
                TransactionPublicKey = transactionKey,
                AssetCommitment = assetCommitment,
                OwnershipProof = ownershipProof,
                EligibilityProof = eligibilityProof,
                BiometricProof = requestInput.BiometricProof
            };

            FillSyncData(block);
            FillAndSign(block, new StealthSignatureInput(requestInput.PrevTransactionKey, assetCommitments, pos));

            return block;
        }

        private async Task<IdentityProofs> CreateIdentityProofsPacket(RequestInput requestInput, AssociatedProofPreparation[] associatedProofPreparations, OutputSources[] outputModels, byte[] issuer)
        {
            byte[] secretKey = CryptoHelper.GetRandomSeed();
            byte[] transactionKey = CryptoHelper.GetPublicKey(secretKey);
            byte[] destinationKey = CryptoHelper.GetDestinationKey(secretKey, _clientCryptoService.PublicKeys[0].ArraySegment.Array, _clientCryptoService.PublicKeys[1].ArraySegment.Array);
            byte[] destinationKey2 = requestInput.PublicViewKey != null ? CryptoHelper.GetDestinationKey(secretKey, requestInput.PublicSpendKey, requestInput.PublicViewKey) : requestInput.PublicSpendKey;
            byte[] blindingFactor = CryptoHelper.GetRandomSeed();
            byte[] blindingFactorToOwnership = CryptoHelper.GetDifferentialBlindingFactor(blindingFactor, requestInput.PrevBlindingFactor);
            byte[] assetCommitment = CryptoHelper.GetAssetCommitment(blindingFactor, requestInput.AssetId);

            GetAssetCommitmentsRing(requestInput.PrevAssetCommitment, outputModels, out int pos, out IKey[] assetCommitments);
            SurjectionProof ownershipProof = CryptoHelper.CreateSurjectionProof(assetCommitment, assetCommitments.Select(s => s.Value).ToArray(), pos, blindingFactorToOwnership);
            SurjectionProof eligibilityProof = await _eligibilityProofsProvider.CreateEligibilityProof(requestInput.EligibilityCommitment, requestInput.EligibilityBlindingFactor, assetCommitment, blindingFactor, issuer).ConfigureAwait(false);
            SurjectionProof authenticationProof = await _relationsBindingService.CreateProofToRegistration(requestInput.PublicSpendKey, blindingFactor, assetCommitment, requestInput.AssetId).ConfigureAwait(false);
            AssociatedProofs[] associatedProofs = GetAssociatedProofs(associatedProofPreparations, blindingFactor, assetCommitment);

            IdentityProofs block = new IdentityProofs
            {
                DestinationKey = destinationKey,
                DestinationKey2 = destinationKey2,
                TransactionPublicKey = transactionKey,
                AssetCommitment = assetCommitment,
                OwnershipProof = ownershipProof,
                EligibilityProof = eligibilityProof,
                AuthenticationProof = authenticationProof,
                AssociatedProofs = associatedProofs,
                BiometricProof = requestInput.BiometricProof,
                EncodedPayload = new EcdhTupleProofs { Mask = blindingFactor, AssetId = requestInput.AssetId, AssetIssuer = requestInput.Issuer, Payload = requestInput.Payload } // ConfidentialAssetsHelper.CreateEcdhTupleProofs(blindingFactor, assetId, issuer, payload, secretKey, target)
            };

            FillSyncData(block);
            FillAndSign(block, new StealthSignatureInput(requestInput.PrevTransactionKey, assetCommitments, pos));

            return block;
        }

        private static AssociatedProofs[] GetAssociatedProofs(AssociatedProofPreparation[] associatedProofPreparations, byte[] blindingFactor, byte[] assetCommitment)
        {
            AssociatedProofs[] associatedProofs = null;
            if (associatedProofPreparations != null && associatedProofPreparations.Length > 0)
            {
                associatedProofs = new AssociatedProofs[associatedProofPreparations.Length];

                for (int i = 0; i < associatedProofPreparations.Length; i++)
                {
                    AssociatedProofs associatedProof = GetAssociatedProof(associatedProofPreparations, blindingFactor, assetCommitment, i);

                    associatedProofs[i] = associatedProof;
                }
            }

            return associatedProofs;
        }

        private static AssociatedProofs GetAssociatedProof(AssociatedProofPreparation[] associatedProofPreparations, byte[] blindingFactor, byte[] assetCommitment, int i)
        {
            byte[] rootBlindingFactorDiff = CryptoHelper.GetDifferentialBlindingFactor(blindingFactor, associatedProofPreparations[i].OriginatingBlindingFactor);
            AssociatedProofs associatedProof;

            if (associatedProofPreparations[i].Commitment == null)
            {
                byte[] associatedBlindingFactorDiff = CryptoHelper.GetDifferentialBlindingFactor(blindingFactor, associatedProofPreparations[i].OriginatingBlindingFactor);

                associatedProof = new AssociatedProofs
                {
                    SchemeName = associatedProofPreparations[i].SchemeName,
                    AssociationProofs = CryptoHelper.CreateSurjectionProof(assetCommitment, new byte[][] { associatedProofPreparations[i].OriginatingAssociatedCommitment }, 0, associatedBlindingFactorDiff),
                    RootProofs = CryptoHelper.CreateSurjectionProof(assetCommitment, new byte[][] { associatedProofPreparations[i].OriginatingRootCommitment }, 0, rootBlindingFactorDiff)
                };
            }
            else
            {
                byte[] associatedBlindingFactorDiff = CryptoHelper.GetDifferentialBlindingFactor(associatedProofPreparations[i].CommitmentBlindingFactor, associatedProofPreparations[i].OriginatingBlindingFactor);

                associatedProof = new AssociatedAssetProofs
                {
                    AssociatedAssetCommitment = associatedProofPreparations[i].Commitment,
                    SchemeName = associatedProofPreparations[i].SchemeName,
                    AssociationProofs = CryptoHelper.CreateSurjectionProof(associatedProofPreparations[i].Commitment, new byte[][] { associatedProofPreparations[i].OriginatingAssociatedCommitment }, 0, associatedBlindingFactorDiff),
                    RootProofs = CryptoHelper.CreateSurjectionProof(assetCommitment, new byte[][] { associatedProofPreparations[i].OriginatingRootCommitment }, 0, rootBlindingFactorDiff)
                };
            }

            return associatedProof;
        }

        private TransitionCompromisedProofs CreateTransitionCompromisedProofs(RequestInput requestInput, byte[] compromisedKeyImage, byte[] compromisedTransactionKey, byte[] destinationKey, OutputSources[] outputModels, byte[][] issuanceCommitments)
        {
            byte[] blindingFactor = CryptoHelper.GetRandomSeed();
            byte[] blindingFactorToOwnership = CryptoHelper.GetDifferentialBlindingFactor(blindingFactor, requestInput.PrevBlindingFactor);
            byte[] blindingFactorToEligibility = CryptoHelper.GetDifferentialBlindingFactor(blindingFactor, requestInput.EligibilityBlindingFactor);
            byte[] assetCommitment = CryptoHelper.GetAssetCommitment(blindingFactor, requestInput.AssetId);
            //_relationsBindingService.GetBoundedCommitment(requestInput.AssetId, requestInput.PublicSpendKey, out byte[] registrationBlindingFactor, out byte[] registrationCommitment);
            //byte[] blindingFactorToRegistration = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(blindingFactor, registrationBlindingFactor);
            //byte[] encodedRegistrationCommitment = ConfidentialAssetsHelper.CreateEncodedCommitment(registrationCommitment, secretKey, target);

            GetAssetCommitmentsRing(requestInput.PrevAssetCommitment, outputModels, out int pos, out IKey[] assetCommitments);

            _eligibilityProofsProvider.GetEligibilityCommitmentAndProofs(requestInput.EligibilityCommitment, issuanceCommitments, out int actualAssetPos, out byte[][] commitments);

            SurjectionProof ownershipProof = CryptoHelper.CreateSurjectionProof(assetCommitment, assetCommitments.Select(s => s.Value).ToArray(), pos, blindingFactorToOwnership);
            SurjectionProof eligibilityProof = CryptoHelper.CreateSurjectionProof(assetCommitment, commitments, actualAssetPos, blindingFactorToEligibility);
            //SurjectionProof authenticationProof = ConfidentialAssetsHelper.CreateSurjectionProof(assetCommitment, new byte[][] { registrationCommitment }, 0, blindingFactorToRegistration);
            //authenticationProof.AssetCommitments[0] = encodedRegistrationCommitment;

            TransitionCompromisedProofs block = new TransitionCompromisedProofs
            {
                DestinationKey = destinationKey,
                DestinationKey2 = requestInput.PublicSpendKey,
                TransactionPublicKey = compromisedTransactionKey,
                CompromisedKeyImage = compromisedKeyImage,
                AssetCommitment = assetCommitment,
                EligibilityProof = eligibilityProof,
                EcdhTuple = CryptoHelper.CreateEcdhTupleCA(blindingFactor, requestInput.AssetId, compromisedTransactionKey, requestInput.PublicSpendKey),
            };

            FillSyncData(block);
            FillAndSign(block, new StealthSignatureInput(requestInput.PrevTransactionKey, assetCommitments, pos));

            return block;
        }

        private UniversalStealthTransaction CreateUniversalTransportPacket(RequestInput requestInput, OutputSources[] outputModels, UniversalProofs universalProofs)
        {
            byte[] secretKey = CryptoHelper.GetRandomSeed();
            byte[] transactionKey = CryptoHelper.GetPublicKey(secretKey);
            byte[] destinationKey = CryptoHelper.GetDestinationKey(secretKey, _clientCryptoService.PublicKeys[0].ArraySegment.Array, _clientCryptoService.PublicKeys[1].ArraySegment.Array);
            byte[] destinationKey2 = requestInput.PublicViewKey != null ? CryptoHelper.GetDestinationKey(secretKey, requestInput.PublicSpendKey, requestInput.PublicViewKey) : requestInput.PublicSpendKey;

            GetAssetCommitmentsRing(requestInput.PrevAssetCommitment, outputModels, out int pos, out IKey[] assetCommitments);


            var transaction = new UniversalStealthTransaction
            {
                DestinationKey = _identityKeyProvider.GetKey(destinationKey),
                DestinationKey2 = _identityKeyProvider.GetKey(destinationKey2),
                TransactionPublicKey = _identityKeyProvider.GetKey(transactionKey),
                AssetCommitment = requestInput.AssetCommitment.ToArray(),
            };

            FillSyncData(block);

            FillAndSign(block,
                new StealthSignatureInput(
                    requestInput.PrevTransactionKey,
                    assetCommitments,
                    pos,
                    p =>
                    {
                        if (p is UniversalTransport universalTransport)
                        {
                            universalProofs.KeyImage = universalTransport.KeyImage;
                            string proofs = JsonConvert.SerializeObject(universalProofs);
                            byte[] proofsBytes = Encoding.UTF8.GetBytes(proofs);
                            byte[] hash = _hashCalculation.CalculateHash(proofsBytes);
                            universalTransport.MessageHash = hash;

                            using ISerializer serializer = _serializersFactory.Create(p);
                            serializer.SerializeBody();
                        }
                    }));

            return transaction;
        }

        #endregion Private Functions
    }
}
