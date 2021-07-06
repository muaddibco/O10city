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

        /*public async Task<RequestResult> SendRelationsProofs(RelationsProofsInput relationsProofsInput, AssociatedProofPreparation[] associatedProofPreparations, OutputSources[] outputModels, byte[][] issuanceCommitments)
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
        }*/


        //public async Task<RequestResult> SendDocumentSignRequest(DocumentSignRequestInput requestInput, AssociatedProofPreparation[] associatedProofPreparations, OutputSources[] outputModels, byte[][] issuanceCommitments)
        //{
        //    var packet = CreateDocumentSignRequest(requestInput, associatedProofPreparations, outputModels, issuanceCommitments);

        //    NextKeyImage = packet.KeyImage;

        //    var completionResult = PropagateTransaction(packet);

        //    RequestResult requestResult = new RequestResult
        //    {
        //        NewCommitment = packet.AssetCommitment,
        //        NewTransactionKey = packet.TransactionPublicKey,
        //        NewDestinationKey = packet.DestinationKey,
        //        Result = await completionResult.Task.ConfigureAwait(false) is SucceededNotification
        //    };

        //    return requestResult;
        //}

        public async Task<RequestResult> SendRevokeIdentity(RequestInput requestInput, byte[][] issuanceCommitments)
        {
            var packet = CreateRevokeIdentityPacket(requestInput, issuanceCommitments);

            NextKeyImage = packet.KeyImage;

            var completionResult = PropagateTransaction(packet);

            RequestResult requestResult = new RequestResult
            {
                NewCommitment = packet.AssetCommitment.Value,
                NewTransactionKey = packet.TransactionPublicKey.Value,
                NewDestinationKey = packet.DestinationKey.Value,
                Result = await completionResult.Task.ConfigureAwait(false) is SucceededNotification
            };

            return requestResult;
        }

        public async Task<RequestResult> SendCompromisedProofs(RequestInput requestInput, byte[] compromisedKeyImage, byte[] compromisedTransactionKey, byte[] destinationKey, OutputSources[] outputModels, byte[][] issuanceCommitments)
        {
            var transaction = CreateKeyImageCompromisedTransaction(requestInput, compromisedKeyImage, compromisedTransactionKey, destinationKey, outputModels, issuanceCommitments);

            NextKeyImage = transaction.KeyImage;

            var completionResult = PropagateTransaction(transaction);

            RequestResult requestResult = new RequestResult
            {
                NewCommitment = transaction.AssetCommitment.Value,
                NewTransactionKey = transaction.TransactionPublicKey.Value,
                NewDestinationKey = transaction.DestinationKey.Value,
                Result = await completionResult.Task.ConfigureAwait(false) is SucceededNotification
            };

            return requestResult;
        }

        public async Task<RequestResult> SendUniversalTransaction([NotNull] RequestInput requestInput, UniversalProofs universalProofs)
        {
            if (requestInput is null)
            {
                throw new ArgumentNullException(nameof(requestInput));
            }

            if (universalProofs is null)
            {
                throw new ArgumentNullException(nameof(universalProofs));
            }

            var transaction = CreateUniversalTransaction(requestInput);

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
                NewCommitment = transaction.AssetCommitment.Value,
                NewTransactionKey = transaction.TransactionPublicKey.Value,
                NewDestinationKey = transaction.DestinationKey.Value,
                Result = result is SucceededNotification
            };

            return requestResult;
        }

        public UserTransactionSecret PopLastTransactionSecrets()
        {
            var secrets = _dataAccessService.GetUserTransactionSecrets(_accountId, NextKeyImage.ToString());
            _dataAccessService.RemoveUserTransactionSecret(_accountId, NextKeyImage.ToString());
            return secrets;
        }

        #endregion Public Functions

        #region Private Functions

/*        private GroupsRelationsProofs CreateRelationsProofs(RelationsProofsInput requestInput, AssociatedProofPreparation[] associatedProofPreparations, OutputSources[] outputModels, byte[][] issuanceCommitments)
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
                EcdhTuple = new EcdhTupleProofs { Mask = new byte[32], AssetId = requestInput.ImageHash, AssetIssuer = requestInput.Issuer, Payload = requestInput.Payload } // CryptoHelper.CreateEcdhTupleProofs(relationsProofsInput.Payload, relationsProofsInput.ImageHash, secretKey, relationsProofsInput.PublicViewKey)
            };

            FillSyncData(block);
            FillAndSign(block, new StealthSignatureInput(requestInput.PrevTransactionKey, assetCommitments, pos));

            return block;
        }
*/
        //private DocumentSignRequest CreateDocumentSignRequest(DocumentSignRequestInput requestInput, AssociatedProofPreparation[] associatedProofPreparations, OutputSources[] outputModels, byte[][] issuanceCommitments)
        //{
        //    byte[] secretKey = CryptoHelper.GetRandomSeed();
        //    byte[] transactionKey = CryptoHelper.GetPublicKey(secretKey);
        //    byte[] destinationKey = CryptoHelper.GetDestinationKey(secretKey, _clientCryptoService.PublicKeys[0].ArraySegment.Array, _clientCryptoService.PublicKeys[1].ArraySegment.Array);
        //    _relationsBindingService.GetBoundedCommitment(requestInput.AssetId, requestInput.PublicSpendKey, out byte[] blindingFactor, out byte[] assetCommitment, requestInput.DocumentHash);

        //    byte[] onboardingToOwnershipBlindingFactor = CryptoHelper.GetDifferentialBlindingFactor(blindingFactor, requestInput.PrevBlindingFactor);
        //    GetAssetCommitmentsRing(requestInput.PrevAssetCommitment, outputModels, out int pos, out IKey[] assetCommitments);
        //    SurjectionProof ownershipProof = CryptoHelper.CreateSurjectionProof(assetCommitment, assetCommitments.Select(s => s.Value).ToArray(), pos, onboardingToOwnershipBlindingFactor);

        //    byte[] onboardingToEligibilityBlindingFactor = CryptoHelper.GetDifferentialBlindingFactor(blindingFactor, requestInput.EligibilityBlindingFactor);
        //    _eligibilityProofsProvider.GetEligibilityCommitmentAndProofs(requestInput.EligibilityCommitment, issuanceCommitments, out int actualAssetPos, out byte[][] commitments);
        //    SurjectionProof eligibilityProof = CryptoHelper.CreateSurjectionProof(assetCommitment, commitments, actualAssetPos, onboardingToEligibilityBlindingFactor);

        //    AssociatedProofs[] associatedProofs = GetAssociatedProofs(associatedProofPreparations, blindingFactor, assetCommitment);

        //    _relationsBindingService.GetBoundedCommitment(requestInput.AssetId, requestInput.GroupIssuer, out byte[] groupBlindingFactor, out byte[] groupEntryAssetCommitment, requestInput.GroupAssetId);
        //    byte[] signToGroupEntryBlindingFactor = CryptoHelper.GetDifferentialBlindingFactor(blindingFactor, groupBlindingFactor);

        //    SurjectionProof groupEntrySurjectionProof = CryptoHelper.CreateSurjectionProof(assetCommitment, new byte[][] { groupEntryAssetCommitment }, 0, signToGroupEntryBlindingFactor, requestInput.DocumentHash, BitConverter.GetBytes(requestInput.DocumentRecordHeight));

        //    _relationsBindingService.GetBoundedCommitment(requestInput.GroupAssetId, requestInput.GroupIssuer, out byte[] groupNameBlindingFactor, out byte[] groupNameCommitment, requestInput.GroupAssetId);

        //    byte[] allowedGroupNameBlindingFactor = CryptoHelper.GetRandomSeed();
        //    byte[] allowedGroupNameCommitment = CryptoHelper.GetAssetCommitment(allowedGroupNameBlindingFactor, requestInput.GroupAssetId);
        //    byte[] diffAllowedGroupNameBlindingFactor = CryptoHelper.GetDifferentialBlindingFactor(allowedGroupNameBlindingFactor, groupNameBlindingFactor);
        //    SurjectionProof allowedGroupNameSurjectionProof = CryptoHelper.CreateSurjectionProof(allowedGroupNameCommitment, new byte[][] { groupNameCommitment }, 0, diffAllowedGroupNameBlindingFactor);

        //    DocumentSignRequest block = new DocumentSignRequest
        //    {
        //        DestinationKey = destinationKey,
        //        DestinationKey2 = requestInput.PublicSpendKey,
        //        TransactionPublicKey = transactionKey,
        //        AssetCommitment = assetCommitment,
        //        OwnershipProof = ownershipProof,
        //        EligibilityProof = eligibilityProof,
        //        SignerGroupRelationProof = groupEntrySurjectionProof,
        //        AllowedGroupCommitment = allowedGroupNameCommitment,
        //        AllowedGroupNameSurjectionProof = allowedGroupNameSurjectionProof,
        //        BiometricProof = requestInput.BiometricProof,

        //        EcdhTuple = new EcdhTupleProofs { Mask = allowedGroupNameBlindingFactor, AssetId = requestInput.DocumentHash, AssetIssuer = requestInput.Issuer, Payload = requestInput.Payload } // CryptoHelper.CreateEcdhTupleProofs(blindingFactor, assetId, issuer, payload, secretKey, target)
        //    };

        //    FillSyncData(block);
        //    FillAndSign(block, new StealthSignatureInput(requestInput.PrevTransactionKey, assetPubs, pos));

        //    return block;
        //}

        private RevokeIdentityTransaction CreateRevokeIdentityPacket(RequestInput requestInput, byte[][] issuanceCommitments)
        {
            byte[] secretKey = CryptoHelper.GetRandomSeed();
            byte[] transactionKey = CryptoHelper.GetPublicKey(secretKey);
            byte[] destinationKey = CryptoHelper.GetDestinationKey(secretKey, _clientCryptoService.PublicKeys[0].ArraySegment.Array, _clientCryptoService.PublicKeys[1].ArraySegment.Array);
            byte[] destinationKey2 = requestInput.PublicViewKey != null ? CryptoHelper.GetDestinationKey(secretKey, requestInput.PublicSpendKey, requestInput.PublicViewKey) : requestInput.PublicSpendKey;
            byte[] blindingFactor = CryptoHelper.GetRandomSeed();
            byte[] blindingFactorToEligibility = CryptoHelper.GetDifferentialBlindingFactor(blindingFactor, requestInput.EligibilityBlindingFactor);
            byte[] assetCommitment = CryptoHelper.GetAssetCommitment(blindingFactor, requestInput.AssetId);

            _eligibilityProofsProvider.GetEligibilityCommitmentAndProofs(requestInput.EligibilityCommitment, issuanceCommitments, out int actualAssetPos, out byte[][] commitments);
            SurjectionProof eligibilityProof = CryptoHelper.CreateSurjectionProof(assetCommitment, commitments, actualAssetPos, blindingFactorToEligibility);

            RevokeIdentityTransaction transaction = new RevokeIdentityTransaction
            {
                DestinationKey = _identityKeyProvider.GetKey(destinationKey),
                DestinationKey2 = _identityKeyProvider.GetKey(destinationKey2),
                TransactionPublicKey = _identityKeyProvider.GetKey(transactionKey),
                AssetCommitment = _identityKeyProvider.GetKey(assetCommitment),
                EligibilityProof = eligibilityProof,
                BiometricProof = requestInput.BiometricProof
            };

            return transaction;
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

        private KeyImageCompromisedTransaction CreateKeyImageCompromisedTransaction(RequestInput requestInput, byte[] compromisedKeyImage, byte[] compromisedTransactionKey, byte[] destinationKey, OutputSources[] outputModels, byte[][] issuanceCommitments)
        {
            byte[] blindingFactor = CryptoHelper.GetRandomSeed();
            byte[] assetCommitment = CryptoHelper.GetAssetCommitment(blindingFactor, requestInput.AssetId);

            KeyImageCompromisedTransaction transaction = new KeyImageCompromisedTransaction
            {
                DestinationKey = _identityKeyProvider.GetKey(destinationKey),
                DestinationKey2 = _identityKeyProvider.GetKey(requestInput.PublicSpendKey),
                TransactionPublicKey = _identityKeyProvider.GetKey(compromisedTransactionKey),
                KeyImage = _identityKeyProvider.GetKey(compromisedKeyImage),
                AssetCommitment = _identityKeyProvider.GetKey(assetCommitment),
            };

            return transaction;
        }

        private UniversalStealthTransaction CreateUniversalTransaction(RequestInput requestInput)
        {
            byte[] secretKey = CryptoHelper.GetRandomSeed();
            byte[] transactionKey = CryptoHelper.GetPublicKey(secretKey);
            byte[] destinationKey = CryptoHelper.GetDestinationKey(secretKey, _clientCryptoService.PublicKeys[0].ArraySegment.Array, _clientCryptoService.PublicKeys[1].ArraySegment.Array);
            byte[] destinationKey2 = requestInput.PublicViewKey != null ? CryptoHelper.GetDestinationKey(secretKey, requestInput.PublicSpendKey, requestInput.PublicViewKey) : requestInput.PublicSpendKey;


            var transaction = new UniversalStealthTransaction
            {
                DestinationKey = _identityKeyProvider.GetKey(destinationKey),
                DestinationKey2 = _identityKeyProvider.GetKey(destinationKey2),
                TransactionPublicKey = _identityKeyProvider.GetKey(transactionKey),
                AssetCommitment = requestInput.AssetCommitment
            };

            return transaction;
        }

        #endregion Private Functions
    }
}
