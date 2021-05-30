using System.Collections.Generic;
using System.Threading.Tasks;
using O10.Transactions.Core.Ledgers.O10State;
using O10.Transactions.Core.Ledgers.Stealth;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Model;
using O10.Client.DataLayer.Services;
using O10.Core.ExtensionMethods;
using O10.Core.Logging;
using O10.Core.Architecture;
using O10.Client.Common.Communication.Notifications;
using O10.Transactions.Core.Ledgers;
using O10.Crypto.Models;
using O10.Transactions.Core.Ledgers.O10State.Transactions;
using O10.Transactions.Core.Ledgers.Stealth.Transactions;
using System.Linq;

namespace O10.Client.Common.Communication
{
    [RegisterExtension(typeof(IWalletSynchronizer), Lifetime = LifetimeManagement.Scoped)]
    public class StealthWalletSynchronizer : WalletSynchronizer
    {
        private readonly IStealthTransactionsService _stealthTransactionsService;
        private readonly IAssetsService _assetsService;

        public override string Name => "Stealth";

        public StealthWalletSynchronizer(IDataAccessService dataAccessService,
                                         IStealthClientCryptoService clientCryptoService,
                                         IStealthTransactionsService stealthTransactionsService,
                                         IAssetsService assetsService,
                                         ILoggerService loggerService)
            : base(dataAccessService, clientCryptoService, loggerService)
        {
            _stealthTransactionsService = stealthTransactionsService;
            _assetsService = assetsService;
        }

        protected override async Task StorePacket(TransactionBase transaction)
        {
            await StoreTransferAssetToStealth(transaction).ConfigureAwait(false);

            StoreUniversalTransport(transaction);

            /*StoreIdentityProofs(transaction);*/

            /*StoreEmployeeRequest(transaction);*/

            /*StoreGroupRelationProofs(transaction);*/

            /*StoreDocumentSignRequest(transaction);*/

            StoreTransitionCompromisedProofs(transaction);

            StoreRevokeIdentity(transaction);

            if (transaction is O10StateTransitionalTransactionBase transitionalTransaction)
            {
                if (_clientCryptoService.CheckTarget(transitionalTransaction.DestinationKey, transitionalTransaction.TransactionPublicKey))
                {
                    await base.StorePacket(transaction).ConfigureAwait(false);
                }
            }
            else
            {
                await base.StorePacket(transaction).ConfigureAwait(false);
            }
        }

        /*private void StoreIdentityProofs(TransactionBase transaction)
		{
			if (transaction is IdentityProofs identityProofs)
			{
				((IStealthClientCryptoService)_clientCryptoService).DecodeEcdhTuple(identityProofs.EncodedPayload, identityProofs.TransactionPublicKey, out byte[] blindingFactor, out byte[] assetId, out byte[] issuer, out byte[] payload);

				string oldKeyImage = identityProofs.KeyImage.Value.ToArray().ToHexString();
				string keyImage = ((IStealthClientCryptoService)_clientCryptoService).GetKeyImage(identityProofs.TransactionPublicKey).ToHexString();
				bool res = _dataAccessService.UpdateUserAttribute(_accountId, oldKeyImage, keyImage, blindingFactor, identityProofs.AssetCommitment, identityProofs.TransactionPublicKey, identityProofs.DestinationKey);

                if(!res)
                {
                    _logger.Error("Failed to save last update");
                }

				NotifyObservers(new UserAttributeStateUpdate
				{
                    Issuer = issuer,
                    AssetId = assetId,
					BlindingFactor = blindingFactor,
					AssetCommitment = identityProofs.AssetCommitment,
					TransactionKey = identityProofs.TransactionPublicKey,
					DestinationKey = identityProofs.DestinationKey
				});
			}
		}*/

        private void StoreUniversalTransport(TransactionBase transactionBase)
        {
            if (transactionBase is UniversalStealthTransaction transaction)
            {
                var transactionSecrets = _stealthTransactionsService.PopLastTransactionSecrets();
                string oldKeyImage = transaction.KeyImage.ToString();
                string keyImage = ((IStealthClientCryptoService)_clientCryptoService).GetKeyImage(transaction.TransactionPublicKey).ToHexString();
                bool res = _dataAccessService.UpdateUserAttribute(_accountId, oldKeyImage, keyImage, transaction.AssetCommitment, transaction.TransactionPublicKey, transaction.DestinationKey);

                if (!res)
                {
                    _logger.Error("Failed to save last update");
                }

                NotifyObservers(new UserAttributeStateUpdate
                {
                    Issuer = transactionSecrets.Issuer.HexStringToByteArray(),
                    AssetId = transactionSecrets.AssetId.HexStringToByteArray(),
                    AssetCommitment = transaction.AssetCommitment,
                    TransactionKey = transaction.TransactionPublicKey,
                    DestinationKey = transaction.DestinationKey
                });
            }
        }

        /*private void StoreEmployeeRequest(PacketBase packetBase)
        {
            if (packetBase is EmployeeRegistrationRequest packet)
            {
                ((IStealthClientCryptoService)_clientCryptoService).DecodeEcdhTuple(packet.EcdhTuple, packet.TransactionPublicKey, out byte[] blindingFactor, out byte[] assetId, out byte[] issuer, out byte[] payload);

				string oldKeyImage = packet.KeyImage.Value.ToArray().ToHexString();
				string keyImage = ((IStealthClientCryptoService)_clientCryptoService).GetKeyImage(packet.TransactionPublicKey).ToHexString();
				_dataAccessService.UpdateUserAttribute(_accountId, oldKeyImage, keyImage, blindingFactor, packet.AssetCommitment, packet.TransactionPublicKey, packet.DestinationKey);

                NotifyObservers(new UserAttributeStateUpdate
                {
                    Issuer = issuer,
                    AssetId = assetId,
                    BlindingFactor = blindingFactor,
                    AssetCommitment = packet.AssetCommitment,
                    TransactionKey = packet.TransactionPublicKey,
                    DestinationKey = packet.DestinationKey
                });
            }
        }*/

        /*private void StoreGroupRelationProofs(PacketBase packetBase)
        {
            if (packetBase is GroupsRelationsProofs packet)
            {
                ((IStealthClientCryptoService)_clientCryptoService).DecodeEcdhTuple(packet.EcdhTuple, packet.TransactionPublicKey, out byte[] blindingFactor, out byte[] assetId, out byte[] issuer, out byte[] payload);

				string oldKeyImage = packet.KeyImage.Value.ToArray().ToHexString();
				string keyImage = ((IStealthClientCryptoService)_clientCryptoService).GetKeyImage(packet.TransactionPublicKey).ToHexString();
				_dataAccessService.UpdateUserAttribute(_accountId, oldKeyImage, keyImage, blindingFactor, packet.AssetCommitment, packet.TransactionPublicKey, packet.DestinationKey);

                NotifyObservers(new UserAttributeStateUpdate
                {
                    Issuer = issuer,
                    AssetId = assetId,
                    BlindingFactor = blindingFactor,
                    AssetCommitment = packet.AssetCommitment,
                    TransactionKey = packet.TransactionPublicKey,
                    DestinationKey = packet.DestinationKey
                });
            }
        }*/

        /*private void StoreDocumentSignRequest(PacketBase packetBase)
        {
            if (packetBase is DocumentSignRequest packet)
            {
                ((IStealthClientCryptoService)_clientCryptoService).DecodeEcdhTuple(packet.EcdhTuple, packet.TransactionPublicKey, out byte[] blindingFactor, out byte[] assetId, out byte[] issuer, out byte[] payload);

				string oldKeyImage = packet.KeyImage.Value.ToArray().ToHexString();
				string keyImage = ((IStealthClientCryptoService)_clientCryptoService).GetKeyImage(packet.TransactionPublicKey).ToHexString();
				_dataAccessService.UpdateUserAttribute(_accountId, oldKeyImage, keyImage, blindingFactor, packet.AssetCommitment, packet.TransactionPublicKey, packet.DestinationKey);

                NotifyObservers(new UserAttributeStateUpdate
                {
                    Issuer = issuer,
                    AssetId = assetId,
                    BlindingFactor = blindingFactor,
                    AssetCommitment = packet.AssetCommitment,
                    TransactionKey = packet.TransactionPublicKey,
                    DestinationKey = packet.DestinationKey
                });
            }
        }*/

        private void StoreTransitionCompromisedProofs(TransactionBase transactionBase)
        {
            if (transactionBase is CompromizationProofsTransaction transaction)
            {
                //((IStealthClientCryptoService)_clientCryptoService).DecodeEcdhTuple(transaction.EcdhTuple, transaction.TransactionPublicKey, out byte[] blindingFactor, out byte[] assetId);

                string oldKeyImage = transaction.KeyImage.Value.ToArray().ToHexString();
                string keyImage = ((IStealthClientCryptoService)_clientCryptoService).GetKeyImage(transaction.TransactionPublicKey).ToHexString();
                if (_dataAccessService.UpdateUserAttribute(_accountId, oldKeyImage, keyImage, transaction.AssetCommitment, transaction.TransactionPublicKey, transaction.DestinationKey))
                {
                    NotifyObservers(new UserAttributeStateUpdate
                    {
                        Issuer = null,
                        AssetId = /*assetId*/null,
                        AssetCommitment = transaction.AssetCommitment,
                        TransactionKey = transaction.TransactionPublicKey,
                        DestinationKey = transaction.DestinationKey
                    });
                }
            }
        }

        private void StoreRevokeIdentity(TransactionBase transactionBase)
        {
            if (transactionBase is RevokeIdentityTransaction packet)
            {
                long disabledId = _dataAccessService.MarkUserRootAttributesOverriden2(_accountId, packet.OwnershipProof.AssetCommitments[0]);

                if (disabledId > 0)
                {
                    EligibilityCommitmentsDisabled eligibilityCommitmentsDisabled = new EligibilityCommitmentsDisabled { DisabledIds = new List<long> { disabledId } };

                    NotifyObservers(eligibilityCommitmentsDisabled);
                }
            }
        }

        //TODO: need to wight to replace storing of UtxoUnspentOutput with sending notification about received packet (like StoreTransitionOnboardingDisclosingProofs)
        private async Task StoreTransferAssetToStealth(TransactionBase transactionBase)
        {
            if (transactionBase is TransferAssetToStealthTransaction transaction)
            {
                ((IStealthClientCryptoService)_clientCryptoService).DecodeEcdhTuple(transaction.TransferredAsset.EcdhTuple, transaction.TransactionPublicKey, out byte[] blindingFactor, out byte[] assetId);

                byte[] issuanceCommitment = transaction.SurjectionProof.AssetCommitments[0];

                _logger.LogIfDebug(() => $"[{_accountId}]: Checking issuance commitment {issuanceCommitment.ToHexString()} for disabling Root Attributes");

                List<long> disabledIds = _dataAccessService.MarkUserRootAttributesOverriden(_accountId, transaction.SurjectionProof.AssetCommitments[0]);

                if ((disabledIds?.Count ?? 0) > 0)
                {
                    _logger.LogIfDebug(() => $"[{_accountId}]: Root Attributes with Ids {string.Join(',', disabledIds)} disabled");

                    EligibilityCommitmentsDisabled eligibilityCommitmentsDisabled = new EligibilityCommitmentsDisabled { DisabledIds = disabledIds };

                    NotifyObservers(eligibilityCommitmentsDisabled);
                }

                bool isMine = _clientCryptoService.CheckTarget(transaction.DestinationKey, transaction.TransactionPublicKey);

                _logger.LogIfDebug(() => $"[{_accountId}]: Target checked for destination key {transaction.DestinationKey} and transaction key {transaction.TransactionPublicKey} and found is mine: {isMine}");

                if (isMine)
                {
                    List<UserRootAttribute> userRootAttributes = _dataAccessService.GetAllNonConfirmedRootAttributes(_accountId);
                    UserRootAttribute userRootAttribute = null;
                    foreach (var item in from item in userRootAttributes
                                         where item.AssetId.Equals32(assetId) && transaction.Source.ToString() == item.Source
                                         select item)
                    {
                        userRootAttribute = item;
                        break;
                    }

                    string keyImage = ((IStealthClientCryptoService)_clientCryptoService).GetKeyImage(transaction.TransactionPublicKey).ToHexString();

                    if (userRootAttribute != null)
                    {
                        userRootAttribute.AssetId = assetId;
                        userRootAttribute.IssuanceTransactionKey = transaction.TransactionPublicKey.ToByteArray();
                        userRootAttribute.IssuanceCommitment = transaction.TransferredAsset.AssetCommitment.ToByteArray();
                        userRootAttribute.AnchoringOriginationCommitment = transaction.SurjectionProof.AssetCommitments[0];
                        userRootAttribute.LastCommitment = transaction.TransferredAsset.AssetCommitment.ToByteArray();
                        userRootAttribute.LastTransactionKey = transaction.TransactionPublicKey.ToByteArray();
                        userRootAttribute.NextKeyImage = keyImage;
                        userRootAttribute.LastDestinationKey = transaction.DestinationKey.ToByteArray();
                        userRootAttribute.Source = transaction.Source.Value.ToHexString();
                        _dataAccessService.UpdateConfirmedRootAttribute(userRootAttribute);
                    }
                    else
                    {
                        userRootAttribute = new UserRootAttribute
                        {
                            AssetId = assetId,
                            SchemeName = await _assetsService.GetAttributeSchemeName(assetId, transaction.Source.ToString()).ConfigureAwait(false),
                            IssuanceTransactionKey = transaction.TransactionPublicKey.ToByteArray(),
                            IssuanceCommitment = transaction.TransferredAsset.AssetCommitment.ToByteArray(),
                            AnchoringOriginationCommitment = transaction.SurjectionProof.AssetCommitments[0],
                            LastCommitment = transaction.TransferredAsset.AssetCommitment.ToByteArray(),
                            LastTransactionKey = transaction.TransactionPublicKey.ToByteArray(),
                            NextKeyImage = keyImage,
                            LastDestinationKey = transaction.DestinationKey.ToByteArray(),
                            Source = transaction.Source.Value.ToHexString()
                        };
                        _dataAccessService.AddUserRootAttribute(_accountId, userRootAttribute);
                    }
                }
            }
        }
    }
}
