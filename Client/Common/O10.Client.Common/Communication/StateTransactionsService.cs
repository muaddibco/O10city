using System;
using System.Collections.Generic;
using O10.Transactions.Core.Ledgers.O10State;
using O10.Transactions.Core.Ledgers.O10State.Internal;
using O10.Client.Common.Entities;
using O10.Client.Common.Interfaces;
using O10.Core.Architecture;
using O10.Core.Cryptography;
using O10.Core.ExtensionMethods;
using O10.Core.HashCalculations;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Crypto.ConfidentialAssets;
using O10.Core;
using System.Threading.Tasks;
using O10.Core.Notifications;

namespace O10.Client.Common.Communication
{
    [RegisterDefaultImplementation(typeof(IStateTransactionsService), Lifetime = LifetimeManagement.Scoped)]
    public class StateTransactionsService : TransactionsServiceBase, IStateTransactionsService
    {
        private readonly IDictionary<byte[], ulong> _heightsDictionary;
        private readonly IStateClientCryptoService _clientCryptoService;
        private long _lastHeight;

        public StateTransactionsService(
            IHashCalculationsRepository hashCalculationsRepository,
            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
            IStateClientCryptoService clientCryptoService,
            IGatewayService gatewayService,
            ILoggerService loggerService)
			: base(
                  hashCalculationsRepository,
                  identityKeyProvidersRegistry,
                  clientCryptoService,
                  gatewayService,
                  loggerService)
        {
            _clientCryptoService = clientCryptoService;
            _heightsDictionary = new Dictionary<byte[], ulong>();
        }

        #region ============ PUBLIC FUNCTIONS =============  

        public async Task Initialize(long accountId)
        {
            _accountId = accountId;
            long lastBlockHeight = (await _gatewayService.GetLastPacketInfo(_clientCryptoService.GetPublicKey()).ConfigureAwait(false)).Height;
            _lastHeight = lastBlockHeight + 1;
        }

		public async Task<DocumentSignRecord> IssueDocumentSignRecord(byte[] documentHash, ulong recordHeight, byte[] keyImage, byte[] signerCommitment, SurjectionProof eligibilityProof, byte[] issuer, SurjectionProof signerGroupRelationProof, byte[] signerGroupCommitment, byte[] groupIssuer, SurjectionProof signerGroupProof, SurjectionProof signerAllowedGroupsProof)
		{
			DocumentSignRecord packet = CreateDocumentSignRecord(documentHash, recordHeight, keyImage, signerCommitment, eligibilityProof, issuer, signerGroupRelationProof, signerGroupCommitment, groupIssuer, signerGroupProof, signerAllowedGroupsProof);
            
            var completionResult = PropagateTransaction(packet);

            return (await completionResult.Task.ConfigureAwait(false) is SucceededNotification) ? packet : null;
		}

		public async Task<DocumentRecord> IssueDocumentRecord(byte[] documentHash, byte[][] allowedSignerCommitments)
		{
			DocumentRecord packet = CreateDocumentRecord(documentHash, allowedSignerCommitments);

            var completionResult = PropagateTransaction(packet);

            return (await completionResult.Task.ConfigureAwait(false) is SucceededNotification) ? packet : null;
        }

        public async Task<CancelEmployeeRecord> IssueCancelEmployeeRecord(byte[] registrationCommitment)
        {
            CancelEmployeeRecord packet = CreateCancelEmployeeRecord(registrationCommitment);

            var completionResult = PropagateTransaction(packet);

            return (await completionResult.Task.ConfigureAwait(false) is SucceededNotification) ? packet : null;
        }

        public async Task<EmployeeRecord> IssueEmployeeRecord(byte[] registrationCommitment, byte[] groupCommitment)
        {
            EmployeeRecord packet = CreateEmployeeRecord(registrationCommitment, groupCommitment);

            var completionResult = PropagateTransaction(packet);

            return (await completionResult.Task.ConfigureAwait(false) is SucceededNotification) ? packet : null;
        }

        public async Task<IssueBlindedAsset> IssueBlindedAsset(byte[] assetId)
        {
            IssueBlindedAsset packet = CreateIssueBlindedAsset(assetId);

            var completionResult = PropagateTransaction(packet);

            return (await completionResult.Task.ConfigureAwait(false) is SucceededNotification) ? packet : null;
        }

        public async Task<IssueBlindedAsset> IssueBlindedAsset2(byte[] assetId, byte[] blindingFactor)
        {
            IssueBlindedAsset packet = CreateIssueBlindedAsset2(assetId, blindingFactor);

            var completionResult = PropagateTransaction(packet);

            return (await completionResult.Task.ConfigureAwait(false) is SucceededNotification) ? packet : null;
        }

        /// <summary>
        /// originatingCommitment = blindingPointValue + assetId * G
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="blindingPointValue"></param>
        /// <param name="blindingPointRoot"></param>
        /// <param name="originatingCommitment"></param>
        /// <returns></returns>
        public async Task<IssueAssociatedBlindedAsset> IssueAssociatedAsset(byte[] assetId,
                                         byte[] blindingPointValue,
                                         byte[] blindingPointRoot)
		{
			IssueAssociatedBlindedAsset packet = CreateIssueAssociatedBlindedAsset(assetId, blindingPointValue, blindingPointRoot);

            var completionResult = PropagateTransaction(packet);

            return (await completionResult.Task.ConfigureAwait(false) is SucceededNotification) ? packet : null;
        }

        public async Task<TransferAssetToStealth> TransferAssetToStealth(byte[] assetId, ConfidentialAccount receiver)
        {
            if (receiver is null)
            {
                throw new ArgumentNullException(nameof(receiver));
            }

            TransferAssetToStealth packet = CreateTransferAssetToStealth(assetId, receiver);

            var completionResult = PropagateTransaction(packet);

            return (await completionResult.Task.ConfigureAwait(false) is SucceededNotification) ? packet : null;
        }

        public async Task<TransferAssetToStealth> TransferAssetToStealth2(byte[] assetId, byte[] issuanceCommitment, ConfidentialAccount receiver)
        {
            TransferAssetToStealth packet = CreateTransferAssetToStealth2(assetId, issuanceCommitment, receiver);

            var completionResult = PropagateTransaction(packet);

            return (await completionResult.Task.ConfigureAwait(false) is SucceededNotification) ? packet : null;
        }

        #endregion

        #region ============ PRIVATE FUNCTIONS ============ 

        private DocumentSignRecord CreateDocumentSignRecord(byte[] documentHash, ulong recordHeight, byte[] keyImage, byte[] signerCommitment, SurjectionProof eligibilityProof, byte[] issuer, SurjectionProof signerGroupRelationProof, byte[] signerGroupCommitment, byte[] groupIssuer, SurjectionProof signerGroupProof, SurjectionProof signerAllowedGroupsProof)
		{
            DocumentSignRecord issueEmployeeRecord = new DocumentSignRecord
            {
                DocumentHash = documentHash,
                RecordHeight = recordHeight,
                KeyImage = keyImage,
                SignerCommitment = signerCommitment,
                EligibilityProof = eligibilityProof,
                Issuer = issuer,
                SignerGroupRelationProof = signerGroupRelationProof,
                SignerGroupCommitment = signerGroupCommitment,
                GroupIssuer = groupIssuer,
                SignerGroupProof = signerGroupProof,
                SignerAllowedGroupsProof = signerAllowedGroupsProof
            };

            FillHeightInfo(issueEmployeeRecord);
            FillSyncData(issueEmployeeRecord);
			FillAndSign(issueEmployeeRecord);

			return issueEmployeeRecord;
		}

		private DocumentRecord CreateDocumentRecord(byte[] documentHash, byte[][] allowedSignerCommitments)
		{
			DocumentRecord issueEmployeeRecord = new DocumentRecord
			{
				DocumentHash = documentHash,
				AllowedSignerGroupCommitments = allowedSignerCommitments?? Array.Empty<byte[]>()
            };

			FillHeightInfo(issueEmployeeRecord);
			FillSyncData(issueEmployeeRecord);
			FillAndSign(issueEmployeeRecord);

			return issueEmployeeRecord;
		}

        private CancelEmployeeRecord CreateCancelEmployeeRecord(byte[] registrationCommitment)
        {
            CancelEmployeeRecord issueEmployeeRecord = new CancelEmployeeRecord
            {
                RegistrationCommitment = registrationCommitment
            };

            FillHeightInfo(issueEmployeeRecord);
            FillSyncData(issueEmployeeRecord);
            FillAndSign(issueEmployeeRecord);

            return issueEmployeeRecord;
        }

        private EmployeeRecord CreateEmployeeRecord(byte[] registrationCommitment, byte[] groupCommitment)
        {
            EmployeeRecord issueEmployeeRecord = new EmployeeRecord
            {
                GroupCommitment = groupCommitment,
                RegistrationCommitment = registrationCommitment
			};

            FillHeightInfo(issueEmployeeRecord);
            FillSyncData(issueEmployeeRecord);
            FillAndSign(issueEmployeeRecord);

            return issueEmployeeRecord;
        }

        private IssueAssociatedBlindedAsset CreateIssueAssociatedBlindedAsset(byte[] assetId,
                                                                              byte[] blindingPointValue,
                                                                              byte[] blindingPointRoot)
		{
			byte[] nonBlindedAssociatedCommitment = O10.Crypto.ConfidentialAssets.CryptoHelper.GetNonblindedAssetCommitment(assetId);
			byte[] commitmentToValue = O10.Crypto.ConfidentialAssets.CryptoHelper.SumCommitments(blindingPointValue, nonBlindedAssociatedCommitment);
			byte[] commitmentToBinding = O10.Crypto.ConfidentialAssets.CryptoHelper.SumCommitments(blindingPointRoot, nonBlindedAssociatedCommitment);

			IssueAssociatedBlindedAsset issueAssociatedBlindedAsset = new IssueAssociatedBlindedAsset
			{
				AssetCommitment = commitmentToValue,
				RootAssetCommitment = commitmentToBinding
			};

			FillHeightInfo(issueAssociatedBlindedAsset);
			FillSyncData(issueAssociatedBlindedAsset);
			FillAndSign(issueAssociatedBlindedAsset);

			return issueAssociatedBlindedAsset;
		}

		private IssueBlindedAsset CreateIssueBlindedAsset(byte[] assetId)
		{
			//TODO: must be replaced with usage of constant secret key of issuing
			_clientCryptoService.GetBoundedCommitment(assetId, out byte[] assetCommitment, out byte[] keyImage, out RingSignature ringSignature);

            IssueBlindedAsset issueBlindedAsset = new IssueBlindedAsset
            {
                AssetCommitment = assetCommitment,
                KeyImage = keyImage,
                UniquencessProof = ringSignature
            };

			FillHeightInfo(issueBlindedAsset);
			FillSyncData(issueBlindedAsset);
			FillAndSign(issueBlindedAsset);

			return issueBlindedAsset;
		}

        private IssueBlindedAsset CreateIssueBlindedAsset2(byte[] assetId, byte[] blindingFactor)
        {
            //TODO: must be replaced with usage of constant secret key of issuing
            byte[] issuanceNonBlindedCommitment = O10.Crypto.ConfidentialAssets.CryptoHelper.GetNonblindedAssetCommitment(assetId);
            byte[] issuanceCommitment = O10.Crypto.ConfidentialAssets.CryptoHelper.BlindAssetCommitment(issuanceNonBlindedCommitment, blindingFactor);
            byte[] keyImage = O10.Crypto.ConfidentialAssets.CryptoHelper.GenerateKeyImage(blindingFactor);
            byte[] pk = O10.Crypto.ConfidentialAssets.CryptoHelper.SubCommitments(issuanceCommitment, issuanceNonBlindedCommitment);
            RingSignature ringSignature = O10.Crypto.ConfidentialAssets.CryptoHelper.GenerateRingSignature(issuanceCommitment, keyImage, new byte[][] { pk }, blindingFactor, 0)[0];

            IssueBlindedAsset issueBlindedAsset = new IssueBlindedAsset
            {
                AssetCommitment = issuanceCommitment,
                KeyImage = keyImage,
                UniquencessProof = ringSignature
            };

            FillHeightInfo(issueBlindedAsset);
            FillSyncData(issueBlindedAsset);
            FillAndSign(issueBlindedAsset);

            return issueBlindedAsset;
        }

        private TransferAssetToStealth CreateTransferAssetToStealth(byte[] assetId, ConfidentialAccount receiver)
        {
            _clientCryptoService.GetBoundedCommitment(assetId, out byte[] issuedAssetCommitment, out byte[] keyImage, out RingSignature ringSignature);
            byte[] secretKey = O10.Crypto.ConfidentialAssets.CryptoHelper.GetRandomSeed();
            byte[] transactionKey = O10.Crypto.ConfidentialAssets.CryptoHelper.GetPublicKey(secretKey);
            byte[] destinationKey = O10.Crypto.ConfidentialAssets.CryptoHelper.GetDestinationKey(secretKey, receiver.PublicSpendKey, receiver.PublicViewKey);
            byte[] blindingFactor = O10.Crypto.ConfidentialAssets.CryptoHelper.GetRandomSeed();
            byte[] assetCommitment = O10.Crypto.ConfidentialAssets.CryptoHelper.GetAssetCommitment(blindingFactor, assetId);

            SurjectionProof surjectionProof = O10.Crypto.ConfidentialAssets.CryptoHelper.CreateSurjectionProof(assetCommitment, new byte[][] { issuedAssetCommitment }, 0, blindingFactor);

            _logger.LogIfDebug(() => $"{nameof(TransferAssetToStealth)} with secretKey={secretKey.ToHexString()}, transactionKey={transactionKey.ToHexString()}, destinationKey={destinationKey.ToHexString()}");

            TransferAssetToStealth transferAssetToStealth = new TransferAssetToStealth
            {
                TransactionPublicKey = transactionKey,
                DestinationKey = destinationKey,
                TransferredAsset = new EncryptedAsset
                {
                    AssetCommitment = assetCommitment,
                    EcdhTuple = O10.Crypto.ConfidentialAssets.CryptoHelper.CreateEcdhTupleCA(blindingFactor, assetId, secretKey, receiver.PublicViewKey)
                },
                SurjectionProof = surjectionProof
            };

            FillHeightInfo(transferAssetToStealth);
            FillSyncData(transferAssetToStealth);
            FillAndSign(transferAssetToStealth);

            return transferAssetToStealth;
        }

        private TransferAssetToStealth CreateTransferAssetToStealth2(byte[] assetId, byte[] issuanceAssetCommitment, ConfidentialAccount receiver)
        {
            byte[] secretKey = O10.Crypto.ConfidentialAssets.CryptoHelper.GetRandomSeed();
            byte[] transactionKey = O10.Crypto.ConfidentialAssets.CryptoHelper.GetPublicKey(secretKey);
            byte[] destinationKey = O10.Crypto.ConfidentialAssets.CryptoHelper.GetDestinationKey(secretKey, receiver.PublicSpendKey, receiver.PublicViewKey);
            byte[] blindingFactor = O10.Crypto.ConfidentialAssets.CryptoHelper.GetRandomSeed();
            byte[] assetCommitment = O10.Crypto.ConfidentialAssets.CryptoHelper.GetAssetCommitment(blindingFactor, assetId);

            SurjectionProof surjectionProof = O10.Crypto.ConfidentialAssets.CryptoHelper.CreateSurjectionProof(assetCommitment, new byte[][] { issuanceAssetCommitment }, 0, blindingFactor);

            TransferAssetToStealth transferAssetToStealth = new TransferAssetToStealth
            {
                TransactionPublicKey = transactionKey,
                DestinationKey = destinationKey,
                TransferredAsset = new EncryptedAsset
                {
                    AssetCommitment = assetCommitment,
                    EcdhTuple = O10.Crypto.ConfidentialAssets.CryptoHelper.CreateEcdhTupleCA(blindingFactor, assetId, secretKey, receiver.PublicViewKey)
                },
                SurjectionProof = surjectionProof
            };

            FillHeightInfo(transferAssetToStealth);
            FillSyncData(transferAssetToStealth);
            FillAndSign(transferAssetToStealth);

            return transferAssetToStealth;
        }

        private void FillHeightInfo(O10StatePacket transactionalBlockBase)
        {
            transactionalBlockBase.Height = _lastHeight++;
        }

		#endregion

	}
}
