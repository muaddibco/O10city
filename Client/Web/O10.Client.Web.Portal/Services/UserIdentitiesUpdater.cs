using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using O10.Transactions.Core.DataModel.Transactional;
using O10.Client.Common.Interfaces;
using O10.Core.Models;
using O10.Core.ExtensionMethods;
using O10.Client.Web.Common.Hubs;
using O10.Client.DataLayer.Services;
using System.Threading.Tasks.Dataflow;
using System.Globalization;
using O10.Client.DataLayer.Model;
using O10.Client.Common.Entities;
using System.Threading.Tasks;
using O10.Core.Logging;
using System;
using O10.Client.Web.Portal.Dtos.User;
using System.Threading;
using Newtonsoft.Json;
using O10.Core.Serialization;
using O10.Client.Common.Communication.Notifications;

namespace O10.Client.Web.Portal.Services
{
    public class UserIdentitiesUpdater : IUpdater
    {
        private readonly IStealthClientCryptoService _clientCryptoService;
        private readonly IAssetsService _assetsService;
        private readonly IDataAccessService _dataAccessService;
        private readonly IHubContext<IdentitiesHub> _idenitiesHubContext;
        private readonly IRelationsProofsValidationService _relationsProofsValidationService;
        private readonly ISchemeResolverService _schemeResolverService;
        private readonly ILogger _logger;
        
        private CancellationToken _cancellationToken;
        private long _accountId;

        public UserIdentitiesUpdater(IStealthClientCryptoService clientCryptoService,
            IAssetsService assetsService, IDataAccessService dataAccessService,
            IHubContext<IdentitiesHub> idenitiesHubContext, IRelationsProofsValidationService relationsProofsValidationService,
            ISchemeResolverService schemeResolverService, ILoggerService loggerService)
        {
            _clientCryptoService = clientCryptoService;
            _assetsService = assetsService;
            _dataAccessService = dataAccessService;
            _idenitiesHubContext = idenitiesHubContext;
            _relationsProofsValidationService = relationsProofsValidationService;
            _schemeResolverService = schemeResolverService;

            _logger = loggerService.GetLogger(nameof(UserIdentitiesUpdater));
            PipeIn = new ActionBlock<PacketBase>(async p =>
            {
                try
                {
                    if (p is TransferAssetToStealth packet)
                    {
                        _logger.LogIfDebug(() => $"[{_accountId}]: Processing {nameof(TransferAssetToStealth)}");
                        UserRootAttribute userRootAttribute = _dataAccessService.GetRootAttributeByOriginalCommitment(_accountId, packet.TransferredAsset.AssetCommitment);
                        if (userRootAttribute != null)
                        {
                            _clientCryptoService.DecodeEcdhTuple(packet.TransferredAsset.EcdhTuple, packet.TransactionPublicKey, out byte[] blindingFactor, out byte[] assetId);
                            await _assetsService.GetAttributeSchemeName(assetId, packet.Signer.ToString()).ContinueWith(t =>
                            {
                                if (t.IsCompleted && !t.IsFaulted)
                                {
                                    _idenitiesHubContext.Clients.Group(_accountId.ToString(CultureInfo.InvariantCulture))
                                    .SendAsync("PushAttribute",
                                    new UserAttributeDto
                                    {
                                        SchemeName = t.Result,
                                        Source = packet.Signer.ToString(),
                                        Content = userRootAttribute.Content,
                                        Validated = true,
                                        IsOverriden = false
                                    });
                                }
                            }, TaskScheduler.Current).ConfigureAwait(false);

                            await ObtainRelations(packet, assetId).ConfigureAwait(false);

                            await ObtainRegistrations(packet, assetId).ConfigureAwait(false);
                        }
                        else
                        {
                            _logger.Error($"[{_accountId}]: No Root Attribute found by commitment {packet.TransferredAsset.AssetCommitment.ToHexString()}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failure during processing {nameof(PipeIn)}, packet {p?.GetType().Name}", ex);
                }
            });

            PipeInNotifications = new ActionBlock<NotificationBase>(n =>
            {
                try
                {
                    _logger.LogIfDebug(() => $"[{_accountId}]: notification {n.GetType().Name} {JsonConvert.SerializeObject(n, new ByteArrayJsonConverter())}");

                    ProcessEligibilityCommitmentsDisabled(n);

                    NotifyUserAttributeLastUpdate(n);

                    NotifyCompromisedKeyImage(n);

                }
                catch (Exception ex)
                {
                    _logger.Error($"Failure during processing {nameof(PipeInNotifications)}, notification {n?.GetType().Name}", ex);
                }
            });
        }

        private async Task ObtainRegistrations(TransferAssetToStealth packet, byte[] assetId)
        {
            _logger.Debug($"[{_accountId}]: {nameof(ObtainRegistrations)}");
            IEnumerable<RegistrationKeyDescriptionStore> userRegistrations = await _schemeResolverService.GetRegistrationCommitments(packet.Signer.ToString(), assetId.ToHexString()).ConfigureAwait(false);
            foreach (var userRegistration in userRegistrations)
            {
                string groupOwnerName = await _schemeResolverService.ResolveIssuer(userRegistration.Key).ConfigureAwait(false);
                long registrationId = _dataAccessService.AddUserRegistration(_accountId, userRegistration.Key, userRegistration.Description, userRegistration.AssetId, userRegistration.Issuer);

                if (registrationId > 0)
                {
                    UserRegistrationDto userRegistrationDto = new UserRegistrationDto
                    {
                        UserRegistrationId = registrationId.ToString(),
                        Commitment = userRegistration.Key,
                        Issuer = userRegistration.Issuer,
                        AssetId = userRegistration.AssetId
                    };

                    await _idenitiesHubContext.Clients.Group(_accountId.ToString(CultureInfo.InvariantCulture)).SendAsync("PushUserRegistration", userRegistrationDto).ConfigureAwait(false);
                }
            }
        }

        private async Task ObtainRelations(TransferAssetToStealth packet, byte[] assetId)
        {
            _logger.Debug($"[{_accountId}]: {nameof(ObtainRelations)}");
            IEnumerable<RegistrationKeyDescriptionStore> groupRelations = await _schemeResolverService.GetGroupRelations(packet.Signer.ToString(), assetId.ToHexString()).ConfigureAwait(false);
            foreach (var groupRelation in groupRelations)
            {
                string groupOwnerName = await _schemeResolverService.ResolveIssuer(groupRelation.Key).ConfigureAwait(false);
                long groupRelationId = _dataAccessService.AddUserGroupRelation(_accountId, groupOwnerName, groupRelation.Key, groupRelation.Description, groupRelation.AssetId, groupRelation.Issuer);

                if (groupRelationId > 0)
                {
                    GroupRelationDto groupRelationDto = new GroupRelationDto
                    {
                        GroupRelationId = groupRelationId,
                        GroupOwnerName = groupOwnerName,
                        GroupOwnerKey = groupRelation.Key,
                        GroupName = groupRelation.Description,
                        Issuer = groupRelation.Issuer,
                        AssetId = groupRelation.AssetId
                    };

                    await _idenitiesHubContext.Clients.Group(_accountId.ToString(CultureInfo.InvariantCulture)).SendAsync("PushGroupRelation", groupRelationDto).ConfigureAwait(false);
                }
            }
        }

        public ITargetBlock<PacketBase> PipeIn { get; set; }
        public ITargetBlock<NotificationBase> PipeInNotifications { get; }

        private void ProcessEligibilityCommitmentsDisabled(NotificationBase value)
        {
            if (value is EligibilityCommitmentsDisabled eligibilityCommitmentsDisabled)
            {
                _logger.Info("ProcessEligibilityCommitmentsDisabled");
                IEnumerable<UserRootAttribute> userRootAttributes = _dataAccessService.GetUserAttributes(_accountId).Where(u => eligibilityCommitmentsDisabled.DisabledIds.Contains(u.UserAttributeId));

                foreach (UserRootAttribute userAttribute in userRootAttributes)
                {
                    NotifyAttributeUpdate(userAttribute);
                }
            }
        }

        private void NotifyCompromisedKeyImage(NotificationBase value)
        {
            if (value is CompromisedKeyImage compromisedKeyImage)
            {
                _dataAccessService.SetAccountCompromised(_accountId);
                _idenitiesHubContext.Clients
                    .Group(_accountId.ToString(CultureInfo.InvariantCulture))
                    .SendAsync("PushUnauthorizedUse",
                    new UnauthorizedUseDto
                    {
                        KeyImage = compromisedKeyImage.KeyImage.ToHexString(),
                        TransactionKey = compromisedKeyImage.TransactionKey.ToHexString(),
                        DestinationKey = compromisedKeyImage.DestinationKey.ToHexString(),
                        Target = compromisedKeyImage.Target.ToHexString()
                    });
            }
        }

        private void NotifyUserAttributeLastUpdate(NotificationBase value)
        {
            if (value is UserAttributeStateUpdate userAttributeStateUpdate)
            {
                _idenitiesHubContext.Clients.Group(_accountId.ToString(CultureInfo.InvariantCulture)).SendAsync("PushUserAttributeLastUpdate",
                    new UserAttributeLastUpdateDto
                    {
                        Issuer = userAttributeStateUpdate.Issuer.ToHexString(),
                        AssetId = userAttributeStateUpdate.AssetId.ToHexString(),
                        LastBlindingFactor = userAttributeStateUpdate.BlindingFactor.ToHexString(),
                        LastCommitment = userAttributeStateUpdate.AssetCommitment.ToHexString(),
                        LastTransactionKey = userAttributeStateUpdate.TransactionKey.ToHexString(),
                        LastDestinationKey = userAttributeStateUpdate.DestinationKey.ToHexString()
                    });
            }
        }

        private void NotifyAttributeUpdate(UserRootAttribute userAttribute)
        {
            UserAttributeDto userAttributeDto = new UserAttributeDto
            {
                SchemeName = userAttribute.SchemeName,
                Source = userAttribute.Source,
                Content = userAttribute.Content,
                Validated = false,
                IsOverriden = true
            };

            _idenitiesHubContext.Clients.Group(_accountId.ToString(CultureInfo.InvariantCulture)).SendAsync("PushUserAttributeUpdate", userAttributeDto);
        }

        public void Initialize(long accountId, CancellationToken cancellationToken)
        {
            _accountId = accountId;
            _cancellationToken = cancellationToken;

            _cancellationToken.Register(() =>
            {
                PipeIn?.Complete();
                PipeInNotifications?.Complete();
            });
        }
    }
}
