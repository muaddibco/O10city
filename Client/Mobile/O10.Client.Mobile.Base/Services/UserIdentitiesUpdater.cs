using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.DataModel.Transactional;
using O10.Transactions.Core.DataModel.Stealth;
using O10.Client.Common.Entities;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Model;
using O10.Client.DataLayer.Services;
using O10.Core.ExtensionMethods;
using O10.Core.Logging;
using O10.Core.Models;
using O10.Core.Tracking;
using O10.Client.Mobile.Base.Interfaces;
using O10.Client.Mobile.Base.Models;
using O10.Client.Mobile.Base.Models.StateNotifications;
using Xamarin.Forms.Internals;
using O10.Core.Architecture;
using System.Threading;
using O10.Client.Common.Communication.Notifications;
using O10.Core.Notifications;

namespace O10.Client.Mobile.Base.Services
{
    [RegisterExtension(typeof(IUpdater), Lifetime = LifetimeManagement.Scoped)]
    public class UserIdentitiesUpdater : IUpdater
    {
        private long _accountId;
        private IStealthClientCryptoService _clientCryptoService;
        private readonly IAssetsService _assetsService;
        private readonly IDataAccessService _dataAccessService;
        private readonly IExecutionContext _executionContext;
        private readonly ISchemeResolverService _schemeResolverService;
        private readonly IRelationsProofsValidationService _relationsProofsValidationService;
        private readonly IStateNotificationService _stateNotificationService;
        private readonly ICompromizationService _compromizationService;
        private readonly ITrackingService _trackingService;
        private readonly ILogger _logger;

        public UserIdentitiesUpdater(IAssetsService assetsService,
                                     IDataAccessService dataAccessService,
                                     IExecutionContext executionContext,
                                     ISchemeResolverService schemeResolverService,
                                     IRelationsProofsValidationService relationsProofsValidationService,
                                     IStealthClientCryptoService clientCryptoService,
                                     IStateNotificationService stateNotificationService,
                                     ICompromizationService compromizationService,
                                     ITrackingService trackingService,
                                     ILoggerService loggerService)
        {
            _assetsService = assetsService;
            _dataAccessService = dataAccessService;
            _executionContext = executionContext;
            _schemeResolverService = schemeResolverService;
            _relationsProofsValidationService = relationsProofsValidationService;
            _stateNotificationService = stateNotificationService;
            _clientCryptoService = clientCryptoService;
            _compromizationService = compromizationService;
            _trackingService = trackingService;
            _logger = loggerService.GetLogger(nameof(UserIdentitiesUpdater));

            PipeIn = new ActionBlock<PacketBase>(async p =>
            {
                try
                {
                    if (p is TransferAssetToStealth packet)
                    {
                        UserRootAttribute userRootAttribute = _dataAccessService.GetRootAttributeByOriginalCommitment(_accountId, packet.TransferredAsset.AssetCommitment);
                        if (userRootAttribute != null)
                        {
                            _clientCryptoService.DecodeEcdhTuple(packet.TransferredAsset.EcdhTuple, packet.TransactionPublicKey, out byte[] blindingFactor, out byte[] assetId);

                            string issuer = packet.Signer.ToString();

                            await RecoverAssociatedAttributes(issuer, assetId).ConfigureAwait(false);

                            await SendNotification(packet, userRootAttribute, blindingFactor, assetId).ConfigureAwait(false);

                            await RecoverGroupRelations(issuer, assetId).ConfigureAwait(false);

                            await RecoverRegistrationCommitments(issuer, assetId).ConfigureAwait(false);
                        }
                    }
                    else if (p is GroupsRelationsProofs relationsProofs && _clientCryptoService.CheckTarget(relationsProofs.DestinationKey2, relationsProofs.TransactionPublicKey))
                    {
                        //RelationProofsValidationResults validationResults = _relationsProofsValidationService.VerifyRelationProofs(relationsProofs, _clientCryptoService);

                        //_idenitiesHubContext.Clients.Group(_accountId.ToString(CultureInfo.InvariantCulture)).SendAsync("PushRelationValidation", validationResults);
                    }
                }
                catch
                {
                }
            });

            PipeInNotifications = new ActionBlock<NotificationBase>(async n =>
            {
                try
                {
                    ProcessEligibilityCommitmentsDisabled(n);

                    NotifyUserAttributeLastUpdate(n);

                    await NotifyCompromisedKeyImage(n).ConfigureAwait(false);

                    NotifyKeyImageCorrupted(n);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to process notification {n.GetType().FullName}", ex);
                }
            });
        }

        private void NotifyKeyImageCorrupted(NotificationBase n)
        {
            if (n is KeyImageCorruptedNotification keyImageCorrupted)
            {
                _stateNotificationService.NotificationsPipe.SendAsync(new KeyImageCorruptedStateNotification(keyImageCorrupted.KeyImage));
            }
        }

        private async Task SendNotification(TransferAssetToStealth packet, UserRootAttribute userRootAttribute, byte[] blindingFactor, byte[] assetId)
        {
            UserAttributeModel userAttributeModel = new UserAttributeModel
            {
                UserAttributeId = userRootAttribute.UserAttributeId,
                SchemeName = userRootAttribute.SchemeName,
                Source = packet.Signer.Value.ToArray().ToHexString(),
                AssetId = assetId.ToHexString(),
                OriginalBlindingFactor = blindingFactor.ToHexString(),
                OriginalCommitment = packet.TransferredAsset.AssetCommitment.ToHexString(),
                LastBlindingFactor = blindingFactor.ToHexString(),
                LastCommitment = packet.TransferredAsset.AssetCommitment.ToHexString(),
                LastTransactionKey = packet.TransactionPublicKey.ToHexString(),
                LastDestinationKey = packet.DestinationKey.ToHexString(),
                Content = userRootAttribute.Content,
                Validated = true,
                IsOverriden = false
            };

            await _stateNotificationService.NotificationsPipe.SendAsync(new RootAttributeAddedStateNotification(userAttributeModel)).ConfigureAwait(false);
        }

        private async Task RecoverAssociatedAttributes(string issuer, byte[] assetId)
        {
            IEnumerable<AssociatedAttributeBackupDTO> associatedAttributeBackups = await _schemeResolverService.GetAssociatedAttributeBackups(issuer, assetId.ToHexString()).ConfigureAwait(false);

            if (associatedAttributeBackups != null)
            {
                associatedAttributeBackups
                    .GroupBy(a => a.AssociatedIssuer)
                    .ForEach(g => _dataAccessService.UpdateUserAssociatedAttributes(_executionContext.AccountId, g.Key, g.Select(s => new Tuple<string, string>(s.SchemeName, s.Content)), assetId));
            }
        }

        private async Task RecoverRegistrationCommitments(string issuer, byte[] assetId)
        {
            IEnumerable<RegistrationKeyDescriptionStore> userRegistrations = await _schemeResolverService.GetRegistrationCommitments(issuer, assetId.ToHexString()).ConfigureAwait(false);
            foreach (var userRegistration in userRegistrations)
            {
                //string groupOwnerName = await _schemeResolverService.ResolveIssuer(userRegistration.Key).ConfigureAwait(false);
                long registrationId = _dataAccessService.AddUserRegistration(_accountId, userRegistration.Key, userRegistration.Description, userRegistration.AssetId, userRegistration.Issuer);

                if (registrationId > 0)
                {
                    UserRegistrationModel userRegistrationModel = new UserRegistrationModel
                    {
                        UserRegistrationId = registrationId.ToString(),
                        Commitment = userRegistration.Key,
                        Issuer = userRegistration.Issuer,
                        AssetId = userRegistration.AssetId
                    };

                    await _stateNotificationService.NotificationsPipe.SendAsync(new UserRegistrationAddedStateNotification(userRegistrationModel)).ConfigureAwait(false);
                }
            }
        }

        private async Task RecoverGroupRelations(string issuer, byte[] assetId)
        {
            IEnumerable<RegistrationKeyDescriptionStore> groupRelations = await _schemeResolverService.GetGroupRelations(issuer, assetId.ToHexString()).ConfigureAwait(false);
            foreach (var groupRelation in groupRelations)
            {
                string groupOwnerName = await _schemeResolverService.ResolveIssuer(groupRelation.Key).ConfigureAwait(false);
                long groupRelationId = _dataAccessService.AddUserGroupRelation(_accountId, groupOwnerName, groupRelation.Key, groupRelation.Description, groupRelation.AssetId, groupRelation.Issuer);

                if (groupRelationId > 0)
                {
                    GroupRelationModel groupRelationModel = new GroupRelationModel
                    {
                        GroupRelationId = groupRelationId,
                        GroupOwnerName = groupOwnerName,
                        GroupOwnerKey = groupRelation.Key,
                        GroupName = groupRelation.Description,
                        Issuer = groupRelation.Issuer,
                        AssetId = groupRelation.AssetId
                    };

                    await _stateNotificationService.NotificationsPipe.SendAsync(new GroupRelationAddedStateNotification(groupRelationModel)).ConfigureAwait(false);
                }
            }
        }

        public ITargetBlock<PacketBase> PipeIn { get; set; }
        public ITargetBlock<NotificationBase> PipeInNotifications { get; }

        public void Initialize(long accountId, CancellationToken cancellationToken)
        {
            _executionContext.GatewayService.PipeOutNotifications.LinkTo(PipeInNotifications);
            _accountId = accountId;

            cancellationToken.Register(() =>
            {
                PipeIn?.Complete();
                PipeInNotifications?.Complete();
            });
        }

        private void ProcessEligibilityCommitmentsDisabled(NotificationBase value)
        {
            if (value is EligibilityCommitmentsDisabled eligibilityCommitmentsDisabled)
            {
                _trackingService.TrackEvent(nameof(UserIdentitiesUpdater));
                foreach (UserRootAttribute userAttribute in _dataAccessService.GetUserAttributes(_accountId).Where(u => eligibilityCommitmentsDisabled.DisabledIds.Contains(u.UserAttributeId)))
                {
                    _stateNotificationService.NotificationsPipe.SendAsync(new RootAttributeDisabledStateNotification(userAttribute.UserAttributeId));
                }
            }
        }

        private async Task NotifyCompromisedKeyImage(NotificationBase value)
        {
            if (value is CompromisedKeyImage compromisedKeyImage)
            {
                await _compromizationService.ProcessCompromization(compromisedKeyImage.KeyImage, compromisedKeyImage.TransactionKey, compromisedKeyImage.DestinationKey, compromisedKeyImage.Target).ConfigureAwait(false);
            }
        }

        private void NotifyUserAttributeLastUpdate(NotificationBase value)
        {
            if (value is UserAttributeStateUpdate userAttributeStateUpdate)
            {
                UserAttributeLastUpdateModel lastUpdateModel = new UserAttributeLastUpdateModel
                {
                    AssetId = userAttributeStateUpdate.AssetId.ToHexString(),
                    LastBlindingFactor = userAttributeStateUpdate.BlindingFactor.ToHexString(),
                    LastCommitment = userAttributeStateUpdate.AssetCommitment.ToHexString(),
                    LastTransactionKey = userAttributeStateUpdate.TransactionKey.ToHexString(),
                    LastDestinationKey = userAttributeStateUpdate.DestinationKey.ToHexString()
                };

                _stateNotificationService.NotificationsPipe.SendAsync(new RootAttributeUpdateStateNotification(lastUpdateModel));
            }
        }
    }
}
