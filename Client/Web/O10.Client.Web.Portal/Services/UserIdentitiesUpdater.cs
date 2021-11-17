using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using O10.Client.Common.Interfaces;
using O10.Core.ExtensionMethods;
using O10.Client.Web.Common.Hubs;
using O10.Client.DataLayer.Services;
using System.Threading.Tasks.Dataflow;
using System.Globalization;
using O10.Client.DataLayer.Model;
using System.Threading.Tasks;
using O10.Core.Logging;
using System;
using O10.Client.Web.DataContracts.User;
using System.Threading;
using Newtonsoft.Json;
using O10.Core.Serialization;
using O10.Client.Common.Communication.Notifications;
using O10.Core.Notifications;
using O10.Crypto.Models;
using O10.Transactions.Core.Ledgers.O10State.Transactions;
using O10.Client.Web.DataContracts;
using O10.Client.Web.Common.Extensions;
using O10.Client.Stealth;
using O10.Client.Common.Dtos;

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
            PipeIn = new ActionBlock<TransactionBase>(async p =>
            {
                try
                {
                    if (p is TransferAssetToStealthTransaction transaction)
                    {
                        _logger.LogIfDebug(() => $"Processing {nameof(TransferAssetToStealthTransaction)}");
                        UserRootAttribute userRootAttribute = _dataAccessService.GetRootAttributeByOriginalCommitment(_accountId, transaction.TransferredAsset.AssetCommitment);
                        if (userRootAttribute != null)
                        {
                            _clientCryptoService.DecodeEcdhTuple(transaction.TransferredAsset.EcdhTuple, transaction.TransactionPublicKey, out byte[] blindingFactor, out byte[] assetId);
                            await _assetsService.GetAttributeSchemeName(assetId, transaction.Source.ToString()).ContinueWith(t =>
                            {
                                if (t.IsCompleted && !t.IsFaulted)
                                {
                                    _idenitiesHubContext.Clients.Group(_accountId.ToString(CultureInfo.InvariantCulture))
                                    .SendAsync("PushAttribute",
                                    new UserAttributeDto
                                    {
                                        SchemeName = t.Result,
                                        IssuerAddress = transaction.Source.ToString(),
                                        Content = userRootAttribute.Content,
                                        State = AttributeState.NotConfirmed
                                    });
                                }
                            }, TaskScheduler.Current).ConfigureAwait(false);

                            await RecoverRelations(transaction, assetId).ConfigureAwait(false);

                            await RecoverRegistrations(transaction, assetId).ConfigureAwait(false);
                        }
                        else
                        {
                            _logger.Error($"No Root Attribute found by commitment {transaction.TransferredAsset.AssetCommitment}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failure during processing {nameof(PipeIn)}, packet {p?.GetType().Name}", ex);
                }
            });

            PipeInNotifications = new ActionBlock<NotificationBase>(async n =>
            {
                try
                {
                    _logger.LogIfDebug(() => $"notification {n.GetType().Name} {JsonConvert.SerializeObject(n, new ByteArrayJsonConverter(), new MemoryByteJsonConverter(), new KeyJsonConverter())}");

                    await ProcessEligibilityCommitmentsDisabled(n);

                    await NotifyUserAttributeLastUpdate(n);

                    await NotifyCompromisedKeyImage(n);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failure during processing {nameof(PipeInNotifications)}, notification {n?.GetType().Name}", ex);
                }
            });
        }

        private async Task RecoverRegistrations(TransferAssetToStealthTransaction transaction, byte[] assetId)
        {
            _logger.Debug($"{nameof(RecoverRegistrations)}");
            IEnumerable<RegistrationKeyDescriptionStoreDTO> userRegistrations = await _schemeResolverService.GetRegistrationCommitments(transaction.Source.ToString(), assetId.ToHexString()).ConfigureAwait(false);
            if(userRegistrations == null)
            {
                return;
            }

            foreach (var userRegistration in userRegistrations)
            {
                string groupOwnerName = await _schemeResolverService.ResolveIssuer(userRegistration.Key).ConfigureAwait(false);
                long registrationId = _dataAccessService.AddUserRegistration(_accountId, userRegistration.Key, userRegistration.Description, userRegistration.AssetId, userRegistration.Issuer);

                if (registrationId > 0)
                {
                    UserRegistrationDto userRegistrationDto = new()
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

        private async Task RecoverRelations(TransferAssetToStealthTransaction transaction, byte[] assetId)
        {
            _logger.Debug($"{nameof(RecoverRelations)}");
            IEnumerable<RegistrationKeyDescriptionStoreDTO> groupRelations = await _schemeResolverService.GetGroupRelations(transaction.Source.ToString(), assetId.ToHexString()).ConfigureAwait(false);
            if(groupRelations == null)
            {
                return;
            }

            foreach (var groupRelation in groupRelations)
            {
                string groupOwnerName = await _schemeResolverService.ResolveIssuer(groupRelation.Key).ConfigureAwait(false);
                long groupRelationId = _dataAccessService.AddUserGroupRelation(_accountId, groupOwnerName, groupRelation.Key, groupRelation.Description, groupRelation.AssetId, groupRelation.Issuer);

                if (groupRelationId > 0)
                {
                    GroupRelationDto groupRelationDto = new()
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

        public ITargetBlock<TransactionBase> PipeIn { get; set; }
        public ITargetBlock<NotificationBase> PipeInNotifications { get; }

        private async Task ProcessEligibilityCommitmentsDisabled(NotificationBase value)
        {
            if (value is EligibilityCommitmentsDisabled eligibilityCommitmentsDisabled)
            {
                _logger.Info("ProcessEligibilityCommitmentsDisabled");
                IEnumerable<UserRootAttribute> userRootAttributes = _dataAccessService.GetUserAttributes(_accountId).Where(u => eligibilityCommitmentsDisabled.DisabledIds.Contains(u.UserAttributeId));

                foreach (UserRootAttribute userAttribute in userRootAttributes)
                {
                    await NotifyAttributeUpdate(userAttribute);
                }
            }
        }

        private async Task NotifyCompromisedKeyImage(NotificationBase value)
        {
            if (value is CompromisedKeyImage compromisedKeyImage)
            {
                _dataAccessService.SetAccountCompromised(_accountId);
                await _idenitiesHubContext.SendMessageAsync(_logger, _accountId.ToString(), "PushUnauthorizedUse", new UnauthorizedUseDto
                {
                    KeyImage = compromisedKeyImage.KeyImage,
                    TransactionKey = compromisedKeyImage.TransactionKey,
                    DestinationKey = compromisedKeyImage.DestinationKey,
                    Target = compromisedKeyImage.Target
                });
            }
        }

        private async Task NotifyUserAttributeLastUpdate(NotificationBase value)
        {
            if (value is UserAttributeStateUpdate userAttributeStateUpdate)
            {
                await _idenitiesHubContext.Clients.Group(_accountId.ToString()).SendAsync("PushUserAttributeLastUpdate",
                    new UserAttributeLastUpdateDto
                    {
                        Issuer = userAttributeStateUpdate.Issuer.ToHexString(),
                        AssetId = userAttributeStateUpdate.AssetId.ToHexString(),
                        LastCommitment = userAttributeStateUpdate.AssetCommitment,
                        LastTransactionKey = userAttributeStateUpdate.TransactionKey,
                        LastDestinationKey = userAttributeStateUpdate.DestinationKey
                    });
            }
        }

        private async Task NotifyAttributeUpdate(UserRootAttribute userAttribute)
        {
            UserAttributeDto userAttributeDto = new()
            {
                SchemeName = userAttribute.SchemeName,
                IssuerAddress = userAttribute.Source,
                Content = userAttribute.Content,
                State = AttributeState.Disabled
            };

            await _idenitiesHubContext.Clients.Group(_accountId.ToString()).SendAsync("PushUserAttributeUpdate", userAttributeDto);
        }

        public void Initialize(long accountId, CancellationToken cancellationToken)
        {
            _accountId = accountId;
            _logger.SetContext(_accountId.ToString());
            _cancellationToken = cancellationToken;

            _cancellationToken.Register(() =>
            {
                PipeIn?.Complete();
                PipeInNotifications?.Complete();
            });
        }
    }
}
