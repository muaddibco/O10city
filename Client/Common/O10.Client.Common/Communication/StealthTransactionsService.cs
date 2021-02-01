using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.DataModel;
using O10.Transactions.Core.DataModel.Stealth;
using O10.Transactions.Core.DataModel.Stealth.Internal;
using O10.Transactions.Core.Parsers;
using O10.Transactions.Core.Serializers;
using O10.Client.Common.Dtos.UniversalProofs;
using O10.Client.Common.Identities;
using O10.Client.Common.Interfaces;
using O10.Client.Common.Interfaces.Inputs;
using O10.Core.Architecture;
using O10.Core.Cryptography;
using O10.Core.ExtensionMethods;
using O10.Core.HashCalculations;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Crypto.ConfidentialAssets;
using O10.Core.Notifications;

namespace O10.Client.Common.Communication
{
    [RegisterDefaultImplementation(typeof(IStealthTransactionsService), Lifetime = LifetimeManagement.Scoped)]
    public class StealthTransactionsService : TransactionsServiceBase, IStealthTransactionsService
    {
        private IStealthClientCryptoService _clientCryptoService;
        private IBoundedAssetsService _relationsBindingService;
        private readonly IEligibilityProofsProvider _eligibilityProofsProvider;
        private readonly IPropagatorBlock<byte[], byte[]> _pipeOutKeyImages;

        public StealthTransactionsService(IHashCalculationsRepository hashCalculationsRepository,
                                       IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
                                       IStealthClientCryptoService clientCryptoService,
                                       IBoundedAssetsService relationsBindingService,
                                       ISerializersFactory serializersFactory,
                                       IBlockParsersRepositoriesRepository blockParsersRepositoriesRepository,
                                       IEligibilityProofsProvider eligibilityProofsProvider,
                                       IGatewayService gatewayService,
                                       ILoggerService loggerService)
            : base(hashCalculationsRepository,
                   identityKeyProvidersRegistry,
                   serializersFactory,
                   blockParsersRepositoriesRepository,
                   clientCryptoService,
                   gatewayService,
                   loggerService)
        {
            _pipeOutKeyImages = new TransformBlock<byte[], byte[]>(w => w);
            _clientCryptoService = clientCryptoService;
            _relationsBindingService = relationsBindingService;
            _eligibilityProofsProvider = eligibilityProofsProvider;
        }

        public ISourceBlock<byte[]> PipeOutKeyImages => _pipeOutKeyImages;

        #region Public Functions

        public override ISourceBlock<T> GetSourcePipe<T>(string name = null)
        {
            if(typeof(T) == typeof(byte[]))
            {
                return (ISourceBlock<T>)_pipeOutKeyImages;
            }

            return base.GetSourcePipe<T>(name);
        }

        public void Initialize(long accountId)
        {
            _accountId = accountId;
        }

        public async Task<RequestResult> SendRelationsProofs(RelationsProofsInput relationsProofsInput, AssociatedProofPreparation[] associatedProofPreparations, OutputModel[] outputModels, byte[][] issuanceCommitments)
        {
            var packet = CreateRelationsProofs(relationsProofsInput, associatedProofPreparations, outputModels, issuanceCommitments);

            await _pipeOutKeyImages.SendAsync(packet.KeyImage.Value.ToArray()).ConfigureAwait(false);

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


        public async Task<RequestResult> SendDocumentSignRequest(DocumentSignRequestInput requestInput, AssociatedProofPreparation[] associatedProofPreparations, OutputModel[] outputModels, byte[][] issuanceCommitments)
        {
            var packet = CreateDocumentSignRequest(requestInput, associatedProofPreparations, outputModels, issuanceCommitments);

			await _pipeOutKeyImages.SendAsync(packet.KeyImage.Value.ToArray()).ConfigureAwait(false);

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

        public async Task<RequestResult> SendEmployeeRegistrationRequest(EmployeeRequestInput requestInput, AssociatedProofPreparation[] associatedProofPreparations, OutputModel[] outputModels, byte[][] issuanceCommitments)
        {
            var packet = CreateEmployeeRegistrationRequest(requestInput, associatedProofPreparations, outputModels, issuanceCommitments);

			await _pipeOutKeyImages.SendAsync(packet.KeyImage.Value.ToArray()).ConfigureAwait(false);

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

        public async Task<RequestResult> SendIdentityProofs(RequestInput requestInput, AssociatedProofPreparation[] associatedProofPreparations, OutputModel[] outputModels, byte[] issuer)
        {
            var packet = await CreateIdentityProofsPacket(requestInput, associatedProofPreparations, outputModels, issuer).ConfigureAwait(false);

			await _pipeOutKeyImages.SendAsync(packet.KeyImage.Value.ToArray()).ConfigureAwait(false);

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

		public async Task<RequestResult> SendRevokeIdentity(RequestInput requestInput, OutputModel[] outputModels, byte[][] issuanceCommitments)
		{
			RevokeIdentity packet = CreateRevokeIdentityPacket(requestInput, outputModels, issuanceCommitments);

			await _pipeOutKeyImages.SendAsync(packet.KeyImage.Value.ToArray()).ConfigureAwait(false);

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

		public async Task<RequestResult> SendCompromisedProofs(RequestInput requestInput, byte[] compromisedKeyImage, byte[] compromisedTransactionKey, byte[] destinationKey, OutputModel[] outputModels, byte[][] issuanceCommitments)
        {
            var packet = CreateTransitionCompromisedProofs(requestInput, compromisedKeyImage, compromisedTransactionKey, destinationKey, outputModels, issuanceCommitments);

			await _pipeOutKeyImages.SendAsync(packet.KeyImage.Value.ToArray()).ConfigureAwait(false);
            
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

        public async Task<RequestResult> SendUniversalTransport([NotNull] RequestInput requestInput, [NotNull] OutputModel[] outputModels, UniversalProofs universalProofs)
        {
            Contract.Requires(requestInput != null);
            Contract.Requires(outputModels != null);
            Contract.Requires(universalProofs != null);

            var packet = CreateUniversalTransportPacket(requestInput, outputModels, universalProofs);

            await _pipeOutKeyImages.SendAsync(packet.KeyImage.Value.ToArray()).ConfigureAwait(false);
            
            var completionSource = PropagateTransaction(packet);

            RequestResult requestResult = new RequestResult
            {
                NewBlindingFactor = requestInput.BlindingFactor.ToArraySegment().Array,
                NewCommitment = packet.AssetCommitment,
                NewTransactionKey = packet.TransactionPublicKey,
                NewDestinationKey = packet.DestinationKey,
                Result = await completionSource.Task.ConfigureAwait(false) is SucceededNotification
            };

            return requestResult;
        }

        #endregion Public Functions

        #region Private Functions

        private GroupsRelationsProofs CreateRelationsProofs(RelationsProofsInput requestInput, AssociatedProofPreparation[] associatedProofPreparations, OutputModel[] outputModels, byte[][] issuanceCommitments)
        {
            byte[] secretKey = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] transactionKey = ConfidentialAssetsHelper.GetPublicKey(secretKey);
            byte[] destinationKey = ConfidentialAssetsHelper.GetDestinationKey(secretKey, _clientCryptoService.PublicKeys[0].ArraySegment.Array, _clientCryptoService.PublicKeys[1].ArraySegment.Array);
            byte[] destinationKey2 = ConfidentialAssetsHelper.GetDestinationKey(secretKey, requestInput.PublicSpendKey, requestInput.PublicViewKey);
            byte[] blindingFactor = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] assetCommitment = ConfidentialAssetsHelper.GetAssetCommitment(blindingFactor, requestInput.AssetId);


            byte[] onboardingToOwnershipBlindingFactor = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(blindingFactor, requestInput.PrevBlindingFactor);
            GetCommitmentAndProofs(requestInput.PrevAssetCommitment, requestInput.PrevDestinationKey, outputModels, out int pos, out byte[][] assetCommitments, out byte[][] assetPubs);
            SurjectionProof ownershipProof = ConfidentialAssetsHelper.CreateSurjectionProof(assetCommitment, assetCommitments, pos, onboardingToOwnershipBlindingFactor);

            byte[] onboardingToEligibilityBlindingFactor = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(blindingFactor, requestInput.EligibilityBlindingFactor);
            _eligibilityProofsProvider.GetEligibilityCommitmentAndProofs(requestInput.EligibilityCommitment, issuanceCommitments, out int actualAssetPos, out byte[][] commitments);
            SurjectionProof eligibilityProof = ConfidentialAssetsHelper.CreateSurjectionProof(assetCommitment, commitments, actualAssetPos, onboardingToEligibilityBlindingFactor);

            GroupRelationProof[] groupRelationProofs = new GroupRelationProof[requestInput.Relations.Length];
            int i = 0;
            foreach (var relation in requestInput.Relations)
            {
                _relationsBindingService.GetBoundedCommitment(requestInput.AssetId, relation.RelatedAssetOwner, out byte[] groupEntryBlindingFactor, out byte[] groupEntryCommitment, relation.RelatedAssetId);
                byte[] diffBF = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(blindingFactor, groupEntryBlindingFactor);
                SurjectionProof groupEntryProof = ConfidentialAssetsHelper.CreateSurjectionProof(assetCommitment, new byte[][] { groupEntryCommitment }, 0, diffBF);
                _relationsBindingService.GetBoundedCommitment(relation.RelatedAssetId, relation.RelatedAssetOwner, out byte[] groupNameBlindingFactor, out byte[] groupNameCommitment, relation.RelatedAssetId);
                SurjectionProof groupNameProof = ConfidentialAssetsHelper.CreateNewIssuanceSurjectionProof(groupNameCommitment, new byte[][] { relation.RelatedAssetId }, 0, groupNameBlindingFactor);

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
                OwnershipProof = ownershipProof,
                EligibilityProof = eligibilityProof,
                RelationProofs = groupRelationProofs,
                AssociatedProofs = associatedProofs,
                BiometricProof = requestInput.BiometricProof,
                EcdhTuple = new EcdhTupleProofs { Mask = new byte[32], AssetId = requestInput.ImageHash, AssetIssuer = requestInput.Issuer, Payload = requestInput.Payload } // ConfidentialAssetsHelper.CreateEcdhTupleProofs(relationsProofsInput.Payload, relationsProofsInput.ImageHash, secretKey, relationsProofsInput.PublicViewKey)
            };

            FillSyncData(block);
            FillAndSign(block, new StealthSignatureInput(requestInput.PrevTransactionKey, assetPubs, pos));

            return block;
        }

        private DocumentSignRequest CreateDocumentSignRequest(DocumentSignRequestInput requestInput, AssociatedProofPreparation[] associatedProofPreparations, OutputModel[] outputModels, byte[][] issuanceCommitments)
		{
			byte[] secretKey = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] transactionKey = ConfidentialAssetsHelper.GetPublicKey(secretKey);
			byte[] destinationKey = ConfidentialAssetsHelper.GetDestinationKey(secretKey, _clientCryptoService.PublicKeys[0].ArraySegment.Array, _clientCryptoService.PublicKeys[1].ArraySegment.Array);
            _relationsBindingService.GetBoundedCommitment(requestInput.AssetId, requestInput.PublicSpendKey, out byte[] blindingFactor, out byte[] assetCommitment, requestInput.DocumentHash);

			byte[] onboardingToOwnershipBlindingFactor = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(blindingFactor, requestInput.PrevBlindingFactor);
			GetCommitmentAndProofs(requestInput.PrevAssetCommitment, requestInput.PrevDestinationKey, outputModels, out int pos, out byte[][] assetCommitments, out byte[][] assetPubs);
			SurjectionProof ownershipProof = ConfidentialAssetsHelper.CreateSurjectionProof(assetCommitment, assetCommitments, pos, onboardingToOwnershipBlindingFactor);

			byte[] onboardingToEligibilityBlindingFactor = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(blindingFactor, requestInput.EligibilityBlindingFactor);
			_eligibilityProofsProvider.GetEligibilityCommitmentAndProofs(requestInput.EligibilityCommitment, issuanceCommitments, out int actualAssetPos, out byte[][] commitments);
			SurjectionProof eligibilityProof = ConfidentialAssetsHelper.CreateSurjectionProof(assetCommitment, commitments, actualAssetPos, onboardingToEligibilityBlindingFactor);

			AssociatedProofs[] associatedProofs = GetAssociatedProofs(associatedProofPreparations, blindingFactor, assetCommitment);

            _relationsBindingService.GetBoundedCommitment(requestInput.AssetId, requestInput.GroupIssuer, out byte[] groupBlindingFactor, out byte[] groupEntryAssetCommitment, requestInput.GroupAssetId);
			byte[] signToGroupEntryBlindingFactor = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(blindingFactor, groupBlindingFactor);

			SurjectionProof groupEntrySurjectionProof = ConfidentialAssetsHelper.CreateSurjectionProof(assetCommitment, new byte[][] { groupEntryAssetCommitment }, 0, signToGroupEntryBlindingFactor, requestInput.DocumentHash, BitConverter.GetBytes(requestInput.DocumentRecordHeight));

            _relationsBindingService.GetBoundedCommitment(requestInput.GroupAssetId, requestInput.GroupIssuer, out byte[] groupNameBlindingFactor, out byte[] groupNameCommitment, requestInput.GroupAssetId);

			byte[] allowedGroupNameBlindingFactor = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] allowedGroupNameCommitment = ConfidentialAssetsHelper.GetAssetCommitment(allowedGroupNameBlindingFactor, requestInput.GroupAssetId);
			byte[] diffAllowedGroupNameBlindingFactor = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(allowedGroupNameBlindingFactor, groupNameBlindingFactor);
			SurjectionProof allowedGroupNameSurjectionProof = ConfidentialAssetsHelper.CreateSurjectionProof(allowedGroupNameCommitment, new byte[][] { groupNameCommitment }, 0, diffAllowedGroupNameBlindingFactor);

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

		private EmployeeRegistrationRequest CreateEmployeeRegistrationRequest(EmployeeRequestInput requestInput, AssociatedProofPreparation[] associatedProofPreparations, OutputModel[] outputModels, byte[][] issuanceCommitments)
        {
            byte[] secretKey = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] transactionKey = ConfidentialAssetsHelper.GetPublicKey(secretKey);
            byte[] destinationKey = ConfidentialAssetsHelper.GetDestinationKey(secretKey, _clientCryptoService.PublicKeys[0].ArraySegment.Array, _clientCryptoService.PublicKeys[1].ArraySegment.Array);
            _relationsBindingService.GetBoundedCommitment(requestInput.AssetId, requestInput.PublicSpendKey, out byte[] blindingFactor, out byte[] assetCommitment, requestInput.GroupAssetId);

            byte[] onboardingToOwnershipBlindingFactor = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(blindingFactor, requestInput.PrevBlindingFactor);
            GetCommitmentAndProofs(requestInput.PrevAssetCommitment, requestInput.PrevDestinationKey, outputModels, out int pos, out byte[][] assetCommitments, out byte[][] assetPubs);
            SurjectionProof ownershipProof = ConfidentialAssetsHelper.CreateSurjectionProof(assetCommitment, assetCommitments, pos, onboardingToOwnershipBlindingFactor);

            byte[] onboardingToEligibilityBlindingFactor = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(blindingFactor, requestInput.EligibilityBlindingFactor);
            _eligibilityProofsProvider.GetEligibilityCommitmentAndProofs(requestInput.EligibilityCommitment, issuanceCommitments, out int actualAssetPos, out byte[][] commitments);
            SurjectionProof eligibilityProof = ConfidentialAssetsHelper.CreateSurjectionProof(assetCommitment, commitments, actualAssetPos, onboardingToEligibilityBlindingFactor);

            AssociatedProofs[] associatedProofs = GetAssociatedProofs(associatedProofPreparations, blindingFactor, assetCommitment);

            _relationsBindingService.GetBoundedCommitment(requestInput.GroupAssetId, requestInput.PublicSpendKey, out byte[] groupBlindingFactor, out byte[] groupCommitment, requestInput.GroupAssetId);

            SurjectionProof groupSurjectionProof = ConfidentialAssetsHelper.CreateNewIssuanceSurjectionProof(groupCommitment, new byte[][] { requestInput.GroupAssetId }, 0, groupBlindingFactor);

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
            FillAndSign(block, new StealthSignatureInput(requestInput.PrevTransactionKey, assetPubs, pos));

            return block;
        }

		private RevokeIdentity CreateRevokeIdentityPacket(RequestInput requestInput, OutputModel[] outputModels, byte[][] issuanceCommitments)
		{
			byte[] secretKey = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] transactionKey = ConfidentialAssetsHelper.GetPublicKey(secretKey);
			byte[] destinationKey = ConfidentialAssetsHelper.GetDestinationKey(secretKey, _clientCryptoService.PublicKeys[0].ArraySegment.Array, _clientCryptoService.PublicKeys[1].ArraySegment.Array);
			byte[] destinationKey2 = requestInput.PublicViewKey != null ? ConfidentialAssetsHelper.GetDestinationKey(secretKey, requestInput.PublicSpendKey, requestInput.PublicViewKey) : requestInput.PublicSpendKey;
			byte[] blindingFactor = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] blindingFactorToOwnership = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(blindingFactor, requestInput.PrevBlindingFactor);
			byte[] blindingFactorToEligibility = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(blindingFactor, requestInput.EligibilityBlindingFactor);
			byte[] assetCommitment = ConfidentialAssetsHelper.GetAssetCommitment(blindingFactor, requestInput.AssetId);

			GetCommitmentAndProofs(requestInput.PrevAssetCommitment, requestInput.PrevDestinationKey, outputModels, out int pos, out byte[][] assetCommitments, out byte[][] assetPubs);
			SurjectionProof ownershipProof = ConfidentialAssetsHelper.CreateSurjectionProof(assetCommitment, assetCommitments, pos, blindingFactorToOwnership);

			_eligibilityProofsProvider.GetEligibilityCommitmentAndProofs(requestInput.EligibilityCommitment, issuanceCommitments, out int actualAssetPos, out byte[][] commitments);
			SurjectionProof eligibilityProof = ConfidentialAssetsHelper.CreateSurjectionProof(assetCommitment, commitments, actualAssetPos, blindingFactorToEligibility);

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
			FillAndSign(block, new StealthSignatureInput(requestInput.PrevTransactionKey, assetPubs, pos));

			return block;
		}

		private async Task<IdentityProofs> CreateIdentityProofsPacket(RequestInput requestInput, AssociatedProofPreparation[] associatedProofPreparations, OutputModel[] outputModels, byte[] issuer)
        {
            byte[] secretKey = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] transactionKey = ConfidentialAssetsHelper.GetPublicKey(secretKey);
            byte[] destinationKey = ConfidentialAssetsHelper.GetDestinationKey(secretKey, _clientCryptoService.PublicKeys[0].ArraySegment.Array, _clientCryptoService.PublicKeys[1].ArraySegment.Array);
            byte[] destinationKey2 = requestInput.PublicViewKey != null ? ConfidentialAssetsHelper.GetDestinationKey(secretKey, requestInput.PublicSpendKey, requestInput.PublicViewKey) : requestInput.PublicSpendKey;
            byte[] blindingFactor = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] blindingFactorToOwnership = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(blindingFactor, requestInput.PrevBlindingFactor);
            byte[] assetCommitment = ConfidentialAssetsHelper.GetAssetCommitment(blindingFactor, requestInput.AssetId);

            GetCommitmentAndProofs(requestInput.PrevAssetCommitment, requestInput.PrevDestinationKey, outputModels, out int pos, out byte[][] assetCommitments, out byte[][] assetPubs);
            SurjectionProof ownershipProof = ConfidentialAssetsHelper.CreateSurjectionProof(assetCommitment, assetCommitments, pos, blindingFactorToOwnership);
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
            FillAndSign(block, new StealthSignatureInput(requestInput.PrevTransactionKey, assetPubs, pos));

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
            byte[] rootBlindingFactorDiff = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(blindingFactor, associatedProofPreparations[i].OriginatingBlindingFactor);
            AssociatedProofs associatedProof;

            if (associatedProofPreparations[i].Commitment == null)
            {
                byte[] associatedBlindingFactorDiff = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(blindingFactor, associatedProofPreparations[i].OriginatingBlindingFactor);

                associatedProof = new AssociatedProofs
                {
                    AssociatedAssetGroupId = associatedProofPreparations[i].GroupId,
                    AssociationProofs = ConfidentialAssetsHelper.CreateSurjectionProof(assetCommitment, new byte[][] { associatedProofPreparations[i].OriginatingAssociatedCommitment }, 0, associatedBlindingFactorDiff),
                    RootProofs = ConfidentialAssetsHelper.CreateSurjectionProof(assetCommitment, new byte[][] { associatedProofPreparations[i].OriginatingRootCommitment }, 0, rootBlindingFactorDiff)
                };
            }
            else
            {
                byte[] associatedBlindingFactorDiff = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(associatedProofPreparations[i].CommitmentBlindingFactor, associatedProofPreparations[i].OriginatingBlindingFactor);

                associatedProof = new AssociatedAssetProofs
                {
                    AssociatedAssetCommitment = associatedProofPreparations[i].Commitment,
                    AssociatedAssetGroupId = associatedProofPreparations[i].GroupId,
                    AssociationProofs = ConfidentialAssetsHelper.CreateSurjectionProof(associatedProofPreparations[i].Commitment, new byte[][] { associatedProofPreparations[i].OriginatingAssociatedCommitment }, 0, associatedBlindingFactorDiff),
                    RootProofs = ConfidentialAssetsHelper.CreateSurjectionProof(assetCommitment, new byte[][] { associatedProofPreparations[i].OriginatingRootCommitment }, 0, rootBlindingFactorDiff)
                };
            }

            return associatedProof;
        }

        private TransitionCompromisedProofs CreateTransitionCompromisedProofs(RequestInput requestInput, byte[] compromisedKeyImage, byte[] compromisedTransactionKey, byte[] destinationKey, OutputModel[] outputModels, byte[][] issuanceCommitments)
        {
            byte[] blindingFactor = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] blindingFactorToOwnership = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(blindingFactor, requestInput.PrevBlindingFactor);
            byte[] blindingFactorToEligibility = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(blindingFactor, requestInput.EligibilityBlindingFactor);
            byte[] assetCommitment = ConfidentialAssetsHelper.GetAssetCommitment(blindingFactor, requestInput.AssetId);
            //_relationsBindingService.GetBoundedCommitment(requestInput.AssetId, requestInput.PublicSpendKey, out byte[] registrationBlindingFactor, out byte[] registrationCommitment);
            //byte[] blindingFactorToRegistration = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(blindingFactor, registrationBlindingFactor);
            //byte[] encodedRegistrationCommitment = ConfidentialAssetsHelper.CreateEncodedCommitment(registrationCommitment, secretKey, target);

            GetCommitmentAndProofs(requestInput.PrevAssetCommitment, requestInput.PrevDestinationKey, outputModels, out int pos, out byte[][] assetCommitments, out byte[][] assetPubs);

            _eligibilityProofsProvider.GetEligibilityCommitmentAndProofs(requestInput.EligibilityCommitment, issuanceCommitments, out int actualAssetPos, out byte[][] commitments);

            SurjectionProof ownershipProof = ConfidentialAssetsHelper.CreateSurjectionProof(assetCommitment, assetCommitments, pos, blindingFactorToOwnership);
            SurjectionProof eligibilityProof = ConfidentialAssetsHelper.CreateSurjectionProof(assetCommitment, commitments, actualAssetPos, blindingFactorToEligibility);
            //SurjectionProof authenticationProof = ConfidentialAssetsHelper.CreateSurjectionProof(assetCommitment, new byte[][] { registrationCommitment }, 0, blindingFactorToRegistration);
            //authenticationProof.AssetCommitments[0] = encodedRegistrationCommitment;

            TransitionCompromisedProofs block = new TransitionCompromisedProofs
            {
                DestinationKey = destinationKey,
                DestinationKey2 = requestInput.PublicSpendKey,
                TransactionPublicKey = compromisedTransactionKey,
                CompromisedKeyImage = compromisedKeyImage,
                AssetCommitment = assetCommitment,
                OwnershipProof = ownershipProof,
                EligibilityProof = eligibilityProof,
                EcdhTuple = ConfidentialAssetsHelper.CreateEcdhTupleCA(blindingFactor, requestInput.AssetId, compromisedTransactionKey, requestInput.PublicSpendKey),
            };

            FillSyncData(block);
            FillAndSign(block, new StealthSignatureInput(requestInput.PrevTransactionKey, assetPubs, pos));

            return block;
        }

        private UniversalTransport CreateUniversalTransportPacket(RequestInput requestInput, OutputModel[] outputModels, UniversalProofs universalProofs)
        {
            byte[] secretKey = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] transactionKey = ConfidentialAssetsHelper.GetPublicKey(secretKey);
            byte[] destinationKey = ConfidentialAssetsHelper.GetDestinationKey(secretKey, _clientCryptoService.PublicKeys[0].ArraySegment.Array, _clientCryptoService.PublicKeys[1].ArraySegment.Array);
            byte[] destinationKey2 = requestInput.PublicViewKey != null ? ConfidentialAssetsHelper.GetDestinationKey(secretKey, requestInput.PublicSpendKey, requestInput.PublicViewKey) : requestInput.PublicSpendKey;
            byte[] blindingFactorToOwnership = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(requestInput.BlindingFactor.Span, requestInput.PrevBlindingFactor);

            GetCommitmentAndProofs(requestInput.PrevAssetCommitment, requestInput.PrevDestinationKey, outputModels, out int pos, out byte[][] assetCommitments, out byte[][] assetPubs);
            SurjectionProof ownershipProof = ConfidentialAssetsHelper.CreateSurjectionProof(requestInput.AssetCommitment.Span, assetCommitments, pos, blindingFactorToOwnership);

            UniversalTransport block = new UniversalTransport
            {
                DestinationKey = destinationKey,
                DestinationKey2 = destinationKey2,
                TransactionPublicKey = transactionKey,
                AssetCommitment = requestInput.AssetCommitment.ToArray(),
                OwnershipProof = ownershipProof
            };

            FillSyncData(block);

            FillAndSign(block, 
                new StealthSignatureInput(
                    requestInput.PrevTransactionKey,
                    assetPubs,
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

            return block;
        }

        /// <summary>
        /// Returns existing Asset Commitments
        /// </summary>
        /// <param name="prevCommitment"></param>
        /// <param name="prevDestinationKey"></param>
        /// <param name="ringSize"></param>
        /// <param name="tagId"></param>
        /// <param name="random"></param>
        /// <param name="actualAssetPos"></param>
        /// <param name="commitments"></param>
        /// <param name="pubs"></param>
        private static void GetCommitmentAndProofs(byte[] prevCommitment, byte[] prevDestinationKey, OutputModel[] outputModels, out int actualAssetPos, out byte[][] commitments, out byte[][] pubs)
        {
            Random random = new Random(BitConverter.ToInt32(prevCommitment, 0));
            int totalItems = outputModels.Length;
            actualAssetPos = random.Next(totalItems);
            commitments = new byte[totalItems][];
            pubs = new byte[totalItems][];
            List<int> pickedPositions = new List<int>();

            for (int i = 0; i < totalItems; i++)
            {
                if (i == actualAssetPos)
                {
                    commitments[i] = prevCommitment;
                    pubs[i] = prevDestinationKey;
                }
                else
                {
                    bool found = false;
                    do
                    {
                        int randomPos = random.Next(totalItems);
                        if (pickedPositions.Contains(randomPos))
                        {
                            continue;
                        }

                        OutputModel outputModel = outputModels[randomPos];
                        if (outputModel.Commitment.Equals32(prevCommitment))
                        {
                            continue;
                        }

                        commitments[i] = outputModel.Commitment;
                        pubs[i] = outputModel.DestinationKey;
                        pickedPositions.Add(randomPos);
                        found = true;
                    } while (!found);
                }
            }
        }

        #endregion Private Functions
    }
}
