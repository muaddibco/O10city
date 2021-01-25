using System.Collections.Generic;
using System.Threading.Tasks;
using O10.Transactions.Core.DataModel.Transactional;
using O10.Transactions.Core.DataModel.Stealth;
using O10.Client.Common.Communication.SynchronizerNotifications;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Model;
using O10.Client.DataLayer.Services;
using O10.Core.ExtensionMethods;
using O10.Core.Logging;
using O10.Core.Models;
using O10.Core.Architecture;

namespace O10.Client.Common.Communication
{
    [RegisterExtension(typeof(IWalletSynchronizer), Lifetime = LifetimeManagement.Scoped)]
    public class StealthWalletSynchronizer : WalletSynchronizer
    {
		private readonly IAssetsService _assetsService;

        public override string Name => "Stealth";

        public StealthWalletSynchronizer(IDataAccessService dataAccessService, 
                                      IStealthClientCryptoService clientCryptoService, 
                                      IAssetsService assetsService,
                                      ILoggerService loggerService)
			: base(dataAccessService, clientCryptoService, loggerService)
		{
			_assetsService = assetsService;
        }

		protected override async Task StorePacket(PacketBase packetBase)
        {
            await StoretransferAssetToStealth(packetBase).ConfigureAwait(false);

            StoreUniversalTransport(packetBase);

            StoreIdentityProofs(packetBase);

            StoreEmployeeRequest(packetBase);

            StoreGroupRelationProofs(packetBase);

            StoreDocumentSignRequest(packetBase);

            StoreTransitionCompromisedProofs(packetBase);

			StoreRevokeIdentity(packetBase);

            if (packetBase is TransactionalTransitionalPacketBase transitionalPacketBase)
            {
                if (_clientCryptoService.CheckTarget(transitionalPacketBase.DestinationKey, transitionalPacketBase.TransactionPublicKey))
                {
                    await base.StorePacket(packetBase).ConfigureAwait(false);
                }
            }
            else
            {
                await base.StorePacket(packetBase).ConfigureAwait(false);
            }
        }

		private void StoreIdentityProofs(PacketBase packetBase)
		{
			if (packetBase is IdentityProofs identityProofs)
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
		}

        private void StoreUniversalTransport(PacketBase packetBase)
        {
            if (packetBase is UniversalTransport universalTransport)
            {
                string oldKeyImage = universalTransport.KeyImage.ToString();
                var transactionSecrets = _dataAccessService.GetUserTransactionSecrets(_accountId, oldKeyImage);
                _dataAccessService.RemoveUserTransactionSecret(_accountId, oldKeyImage);
                byte[] blindingFactor = transactionSecrets.BlindingFactor.HexStringToByteArray();
                string keyImage = ((IStealthClientCryptoService)_clientCryptoService).GetKeyImage(universalTransport.TransactionPublicKey).ToHexString();
                bool res = _dataAccessService.UpdateUserAttribute(_accountId, oldKeyImage, keyImage, blindingFactor, universalTransport.AssetCommitment, universalTransport.TransactionPublicKey, universalTransport.DestinationKey);

                if (!res)
                {
                    _logger.Error("Failed to save last update");
                }

                NotifyObservers(new UserAttributeStateUpdate
                {
                    Issuer = transactionSecrets.Issuer.HexStringToByteArray(),
                    AssetId = transactionSecrets.AssetId.HexStringToByteArray(),
                    BlindingFactor = blindingFactor,
                    AssetCommitment = universalTransport.AssetCommitment,
                    TransactionKey = universalTransport.TransactionPublicKey,
                    DestinationKey = universalTransport.DestinationKey
                });
            }
        }

        private void StoreEmployeeRequest(PacketBase packetBase)
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
        }

        private void StoreGroupRelationProofs(PacketBase packetBase)
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
        }

        private void StoreDocumentSignRequest(PacketBase packetBase)
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
        }

        private void StoreTransitionCompromisedProofs(PacketBase packetBase)
        {
            if (packetBase is TransitionCompromisedProofs packet)
            {
                ((IStealthClientCryptoService)_clientCryptoService).DecodeEcdhTuple(packet.EcdhTuple, packet.TransactionPublicKey, out byte[] blindingFactor, out byte[] assetId);

				string oldKeyImage = packet.KeyImage.Value.ToArray().ToHexString();
				string keyImage = ((IStealthClientCryptoService)_clientCryptoService).GetKeyImage(packet.TransactionPublicKey).ToHexString();
				if(_dataAccessService.UpdateUserAttribute(_accountId, oldKeyImage, keyImage, blindingFactor, packet.AssetCommitment, packet.TransactionPublicKey, packet.DestinationKey))
                {
                    NotifyObservers(new UserAttributeStateUpdate
                    {
                        Issuer = null,
                        AssetId = assetId,
                        BlindingFactor = blindingFactor,
                        AssetCommitment = packet.AssetCommitment,
                        TransactionKey = packet.TransactionPublicKey,
                        DestinationKey = packet.DestinationKey
                    });
                }
            }
        }

		private void StoreRevokeIdentity(PacketBase packetBase)
		{
			if (packetBase is RevokeIdentity packet)
			{
				long disabledId = _dataAccessService.MarkUserRootAttributesOverriden2(_accountId, packet.EligibilityProof.AssetCommitments[0]);

				if (disabledId > 0)
				{
					EligibilityCommitmentsDisabled eligibilityCommitmentsDisabled = new EligibilityCommitmentsDisabled { DisabledIds = new List<long> { disabledId } };

					NotifyObservers(eligibilityCommitmentsDisabled);
				}
			}
		}

		//TODO: need to wight to replace storing of UtxoUnspentOutput with sending notification about received packet (like StoreTransitionOnboardingDisclosingProofs)
		private async Task StoretransferAssetToStealth(PacketBase packetBase)
		{
            if (packetBase is TransferAssetToStealth packet)
            {
                ((IStealthClientCryptoService)_clientCryptoService).DecodeEcdhTuple(packet.TransferredAsset.EcdhTuple, packet.TransactionPublicKey, out byte[] blindingFactor, out byte[] assetId);

                byte[] issuanceCommitment = packet.SurjectionProof.AssetCommitments[0];

                _logger.LogIfDebug(() => $"[{_accountId}]: Checking issuance commitment {issuanceCommitment.ToHexString()} for disabling Root Attributes");

                List<long> disabledIds = _dataAccessService.MarkUserRootAttributesOverriden(_accountId, packet.SurjectionProof.AssetCommitments[0]);

                if ((disabledIds?.Count ?? 0) > 0)
                {
                    _logger.LogIfDebug(() => $"[{_accountId}]: Root Attributes with Ids {string.Join(',', disabledIds)} disabled");

                    EligibilityCommitmentsDisabled eligibilityCommitmentsDisabled = new EligibilityCommitmentsDisabled { DisabledIds = disabledIds };

                    NotifyObservers(eligibilityCommitmentsDisabled);
                }

                bool isMine = _clientCryptoService.CheckTarget(packet.DestinationKey, packet.TransactionPublicKey);

                _logger.LogIfDebug(() => $"[{_accountId}]: Target checked for destination key {packet.DestinationKey.ToHexString()} and transaction key {packet.TransactionPublicKey.ToHexString()} and found is mine: {isMine}");

                if (isMine)
				{
                    List<UserRootAttribute> userRootAttributes = _dataAccessService.GetAllNonConfirmedRootAttributes(_accountId);
                    UserRootAttribute userRootAttribute = null;
                    foreach (var item in userRootAttributes)
                    {
                        if (item.AssetId.Equals32(assetId) && packet.Signer.ToString() == item.Source)
                        {
                            userRootAttribute = item;
                            break;
                        }
                    }

                    string keyImage = ((IStealthClientCryptoService)_clientCryptoService).GetKeyImage(packet.TransactionPublicKey).ToHexString();

                    if (userRootAttribute != null)
                    {
                        userRootAttribute.AssetId = assetId;
                        userRootAttribute.OriginalBlindingFactor = blindingFactor;
                        userRootAttribute.OriginalCommitment = packet.TransferredAsset.AssetCommitment;
                        userRootAttribute.IssuanceCommitment = packet.SurjectionProof.AssetCommitments[0];
                        userRootAttribute.LastBlindingFactor = blindingFactor;
                        userRootAttribute.LastCommitment = packet.TransferredAsset.AssetCommitment;
                        userRootAttribute.LastTransactionKey = packet.TransactionPublicKey;
                        userRootAttribute.NextKeyImage = keyImage;
                        userRootAttribute.LastDestinationKey = packet.DestinationKey;
                        userRootAttribute.Source = packet.Signer.Value.ToHexString();
                        _dataAccessService.UpdateConfirmedRootAttribute(userRootAttribute);
                    }
                    else
                    {
                        userRootAttribute = new UserRootAttribute
                        {
                            AssetId = assetId,
                            SchemeName = await _assetsService.GetAttributeSchemeName(assetId, packet.Signer.ToString()).ConfigureAwait(false),
                            OriginalBlindingFactor = blindingFactor,
                            OriginalCommitment = packet.TransferredAsset.AssetCommitment,
                            IssuanceCommitment = packet.SurjectionProof.AssetCommitments[0],
                            LastBlindingFactor = blindingFactor,
                            LastCommitment = packet.TransferredAsset.AssetCommitment,
                            LastTransactionKey = packet.TransactionPublicKey,
							NextKeyImage = keyImage,
							LastDestinationKey = packet.DestinationKey,
                            Source = packet.Signer.Value.ToHexString()
                        };
                        _dataAccessService.AddUserRootAttribute(_accountId, userRootAttribute);
                    }
				}
			}
		}
	}
}
