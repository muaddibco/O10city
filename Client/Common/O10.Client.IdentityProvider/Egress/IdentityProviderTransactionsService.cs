using System;
using System.Collections.Generic;
using O10.Transactions.Core.Ledgers.O10State.Internal;
using O10.Client.Common.Interfaces;
using O10.Core.Architecture;
using O10.Core.ExtensionMethods;
using O10.Core.HashCalculations;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Crypto.ConfidentialAssets;
using System.Threading.Tasks;
using O10.Core.Notifications;
using O10.Transactions.Core.Ledgers.O10State.Transactions;
using O10.Crypto.Models;
using O10.Client.Common.Dtos;

namespace O10.Client.Common.Communication
{
    [RegisterDefaultImplementation(typeof(IIdentityProviderTransactionsService), Lifetime = LifetimeManagement.Scoped)]
    public class IdentityProviderTransactionsService : TransactionsServiceBase, IIdentityProviderTransactionsService
    {
        private readonly IDictionary<byte[], ulong> _heightsDictionary;
        private readonly IStateClientCryptoService _clientCryptoService;
        private long _lastHeight;

        public IdentityProviderTransactionsService(
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

        /// <summary>
        /// originatingCommitment = blindingPointValue + assetId * G
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="blindingPointValue"></param>
        /// <param name="blindingPointRoot"></param>
        /// <param name="originatingCommitment"></param>
        /// <returns></returns>
        public async Task<IssueAssociatedBlindedAssetTransaction?> IssueAssociatedAsset(byte[] assetId, byte[] blindingPointValue, byte[] blindingPointRoot)
        {
            var packet = CreateIssueAssociatedBlindedAsset(assetId, blindingPointValue, blindingPointRoot);

            var completionResult = PropagateTransaction(packet);

            return (await completionResult.Task.ConfigureAwait(false) is SucceededNotification) ? packet : null;
        }

        public async Task<IssueBlindedAssetTransaction?> IssueBlindedAsset(byte[] assetId)
        {
            var packet = CreateIssueBlindedAsset(assetId);

            var completionResult = PropagateTransaction(packet);

            return (await completionResult.Task.ConfigureAwait(false) is SucceededNotification) ? packet : null;
        }

        

        public async Task<IssueBlindedAssetTransaction?> IssueBlindedAsset2(byte[] assetId, byte[] blindingFactor)
        {
            var packet = CreateIssueBlindedAsset2(assetId, blindingFactor);

            var completionResult = PropagateTransaction(packet);

            return (await completionResult.Task.ConfigureAwait(false) is SucceededNotification) ? packet : null;
        }

        public async Task<TransferAssetToStealthTransaction?> TransferAssetToStealth(byte[] assetId, ConfidentialAccountDTO receiver)
        {
            if (receiver is null)
            {
                throw new ArgumentNullException(nameof(receiver));
            }

            var packet = CreateTransferAssetToStealth(assetId, receiver);

            var completionResult = PropagateTransaction(packet);

            return (await completionResult.Task.ConfigureAwait(false) is SucceededNotification) ? packet : null;
        }

        public async Task<TransferAssetToStealthTransaction?> TransferAssetToStealth2(byte[] assetId, byte[] issuanceCommitment, ConfidentialAccountDTO receiver)
        {
            if (receiver is null)
            {
                throw new ArgumentNullException(nameof(receiver));
            }

            var packet = CreateTransferAssetToStealth2(assetId, issuanceCommitment, receiver);

            var completionResult = PropagateTransaction(packet);

            return (await completionResult.Task.ConfigureAwait(false) is SucceededNotification) ? packet : null;
        }

        #endregion

        #region ============ PRIVATE FUNCTIONS ============ 

        private IssueBlindedAssetTransaction CreateIssueBlindedAsset(byte[] assetId)
        {
            //TODO: must be replaced with usage of constant secret key of issuing
            _clientCryptoService.GetBoundedCommitment(assetId, out byte[] assetCommitment, out byte[] keyImage, out RingSignature ringSignature);

            var issueBlindedAsset = new IssueBlindedAssetTransaction
            {
                AssetCommitment = _identityKeyProvider.GetKey(assetCommitment)
            };

            /*            FillHeightInfo(issueBlindedAsset);
                        FillSyncData(issueBlindedAsset);
                        FillAndSign(issueBlindedAsset);
            */

            var cont = issueBlindedAsset.ToString();
            return issueBlindedAsset;
        }

        private IssueAssociatedBlindedAssetTransaction CreateIssueAssociatedBlindedAsset(byte[] assetId, byte[] blindingPointValue, byte[] blindingPointRoot)
        {
            byte[] nonBlindedAssociatedCommitment = CryptoHelper.GetNonblindedAssetCommitment(assetId);
            byte[] commitmentToValue = CryptoHelper.SumCommitments(blindingPointValue, nonBlindedAssociatedCommitment);
            byte[] commitmentToBinding = CryptoHelper.SumCommitments(blindingPointRoot, nonBlindedAssociatedCommitment);

            var issueAssociatedBlindedAsset = new IssueAssociatedBlindedAssetTransaction
            {
                AssetCommitment = _identityKeyProvider.GetKey(commitmentToValue),
                RootAssetCommitment = _identityKeyProvider.GetKey(commitmentToBinding)
            };

            return issueAssociatedBlindedAsset;
        }

        private IssueBlindedAssetTransaction CreateIssueBlindedAsset2(byte[] assetId, byte[] blindingFactor)
        {
            //TODO: must be replaced with usage of constant secret key of issuing
            byte[] issuanceNonBlindedCommitment = CryptoHelper.GetNonblindedAssetCommitment(assetId);
            byte[] issuanceCommitment = CryptoHelper.BlindAssetCommitment(issuanceNonBlindedCommitment, blindingFactor);
            byte[] keyImage = CryptoHelper.GenerateKeyImage(blindingFactor);
            byte[] pk = CryptoHelper.SubCommitments(issuanceCommitment, issuanceNonBlindedCommitment);
            RingSignature ringSignature = CryptoHelper.GenerateRingSignature(issuanceCommitment, keyImage, new List<IKey> { _identityKeyProvider.GetKey(pk) }, blindingFactor, 0)[0];

            var issueBlindedAsset = new IssueBlindedAssetTransaction
            {
                AssetCommitment = _identityKeyProvider.GetKey(issuanceCommitment),
            };

            return issueBlindedAsset;
        }

        private TransferAssetToStealthTransaction CreateTransferAssetToStealth(byte[] assetId, ConfidentialAccountDTO receiver)
        {
            _clientCryptoService.GetBoundedCommitment(assetId, out byte[] issuedAssetCommitment, out byte[] keyImage, out RingSignature ringSignature);
            byte[] secretKey = CryptoHelper.GetRandomSeed();
            byte[] transactionKey = CryptoHelper.GetPublicKey(secretKey);
            byte[] destinationKey = CryptoHelper.GetDestinationKey(secretKey, receiver.PublicSpendKey, receiver.PublicViewKey);
            byte[] blindingFactor = CryptoHelper.GetOTSK(receiver.PublicViewKey, secretKey);
            byte[] assetCommitment = CryptoHelper.GetAssetCommitment(blindingFactor, assetId);

            SurjectionProof surjectionProof = CryptoHelper.CreateSurjectionProof(assetCommitment, new byte[][] { issuedAssetCommitment }, 0, blindingFactor);

            _logger.LogIfDebug(() => $"{nameof(TransferAssetToStealth)} with secretKey={secretKey.ToHexString()}, transactionKey={transactionKey.ToHexString()}, destinationKey={destinationKey.ToHexString()}, while receiver's Public Spend Key is {receiver.PublicSpendKey.ToHexString()} and Public View Key is {receiver.PublicViewKey.ToHexString()}");

            var transferAssetToStealth = new TransferAssetToStealthTransaction
            {
                TransactionPublicKey = _identityKeyProvider.GetKey(transactionKey),
                DestinationKey = _identityKeyProvider.GetKey(destinationKey),
                TransferredAsset = new EncryptedAsset
                {
                    AssetCommitment = _identityKeyProvider.GetKey(assetCommitment),
                    EcdhTuple = CryptoHelper.CreateEcdhTupleCA(blindingFactor, assetId, secretKey, receiver.PublicViewKey)
                },
                SurjectionProof = surjectionProof
            };

            return transferAssetToStealth;
        }

        private TransferAssetToStealthTransaction CreateTransferAssetToStealth2(byte[] assetId, byte[] issuanceAssetCommitment, ConfidentialAccountDTO receiver)
        {
            byte[] secretKey = CryptoHelper.GetRandomSeed();
            byte[] transactionKey = CryptoHelper.GetPublicKey(secretKey);
            byte[] destinationKey = CryptoHelper.GetDestinationKey(secretKey, receiver.PublicSpendKey, receiver.PublicViewKey);
            byte[] blindingFactor = CryptoHelper.GetRandomSeed();
            byte[] assetCommitment = CryptoHelper.GetAssetCommitment(blindingFactor, assetId);

            SurjectionProof surjectionProof = CryptoHelper.CreateSurjectionProof(assetCommitment, new byte[][] { issuanceAssetCommitment }, 0, blindingFactor);

            var transferAssetToStealth = new TransferAssetToStealthTransaction
            {
                TransactionPublicKey = _identityKeyProvider.GetKey(transactionKey),
                DestinationKey = _identityKeyProvider.GetKey(destinationKey),
                TransferredAsset = new EncryptedAsset
                {
                    AssetCommitment = _identityKeyProvider.GetKey(assetCommitment),
                    EcdhTuple = CryptoHelper.CreateEcdhTupleCA(blindingFactor, assetId, secretKey, receiver.PublicViewKey)
                },
                SurjectionProof = surjectionProof
            };

            return transferAssetToStealth;
        }

        #endregion
    }
}
