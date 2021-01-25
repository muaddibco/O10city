using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.DataModel;
using O10.Client.Common.Configuration;
using O10.Client.Common.Interfaces;
using O10.Client.Common.Interfaces.Inputs;
using O10.Client.DataLayer.Model;
using O10.Client.DataLayer.Services;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.ExtensionMethods;
using O10.Core.Logging;
using O10.Client.Mobile.Base.Interfaces;
using O10.Client.Mobile.Base.Models.StateNotifications;

namespace O10.Client.Mobile.Base.Services
{
    [RegisterDefaultImplementation(typeof(ICompromizationService), Lifetime = LifetimeManagement.Singleton)]
    public class CompromizationService : ICompromizationService
    {
        private readonly IExecutionContext _executionContext;
        private readonly IGatewayService _gatewayService;
        private readonly IStateNotificationService _stateNotificationService;
        private readonly IDataAccessService _dataAccessService;
        private readonly IRestApiConfiguration _restApiConfiguration;
        private readonly ILogger _logger;
        private bool _isProtectionEnabled;

        public CompromizationService(IExecutionContext executionContext,
                                     IGatewayService gatewayService,
                                     IStateNotificationService stateNotificationService,
                                     IDataAccessService dataAccessService,
                                     ILoggerService loggerService,
                                     IConfigurationService configurationService)
        {
            _executionContext = executionContext;
            _gatewayService = gatewayService;
            _stateNotificationService = stateNotificationService;
            _dataAccessService = dataAccessService;
            _logger = loggerService.GetLogger(nameof(CompromizationService));
            _restApiConfiguration = configurationService.Get<IRestApiConfiguration>();
            _isProtectionEnabled = _dataAccessService.GetUserSettings(_executionContext.AccountId)?.IsAutoTheftProtection ?? true;
        }

        public bool IsProtectionEnabled
        {
            get => _isProtectionEnabled;
            set
            {
                if (_isProtectionEnabled != value)
                {
                    _isProtectionEnabled = value;

                    UserSettings userSettings = _dataAccessService.GetUserSettings(_executionContext.AccountId) ?? new UserSettings();

                    userSettings.IsAutoTheftProtection = value;

                    _dataAccessService.SetUserSettings(_executionContext.AccountId, userSettings);

                    _stateNotificationService.NotificationsPipe.SendAsync(new AccountModeChangedStateNotification { IsProtectionEnabled = value });
                }
            }
        }

        public async Task ProcessCompromization(byte[] keyImage, byte[] transactionKey, byte[] destinationKey, byte[] target)
        {
            if (!_isProtectionEnabled)
            {
                return;
            }

            UserRootAttribute rootAttribute = GetRootAttributeOnTransactionKeyArriving(transactionKey);

            if (rootAttribute == null)
            {
                _logger.Error($"Failed to obtain root attribute by the transaction key {transactionKey.ToHexString()}");
                return;
            }

            byte[] issuer = rootAttribute.Source.HexStringToByteArray();
            byte[] assetId = rootAttribute.AssetId;
            byte[] originalBlindingFactor = rootAttribute.OriginalBlindingFactor;
            byte[] originalCommitment = rootAttribute.OriginalCommitment;
            byte[] lastTransactionKey = rootAttribute.LastTransactionKey;
            byte[] lastBlindingFactor = rootAttribute.LastBlindingFactor;
            byte[] lastCommitment = rootAttribute.LastCommitment;
            byte[] lastDestinationKey = rootAttribute.LastDestinationKey;

            RequestInput requestInput = new RequestInput
            {
                AssetId = assetId,
                EligibilityBlindingFactor = originalBlindingFactor,
                EligibilityCommitment = originalCommitment,
                Issuer = issuer,
                PrevAssetCommitment = lastCommitment,
                PrevBlindingFactor = lastBlindingFactor,
                PrevDestinationKey = lastDestinationKey,
                PrevTransactionKey = lastTransactionKey,
                PublicSpendKey = target
            };

            OutputModel[] outputModels = await _gatewayService.GetOutputs(_restApiConfiguration.RingSize + 1).ConfigureAwait(false);
            byte[][] issuanceCommitments = await _gatewayService.GetIssuanceCommitments(issuer, _restApiConfiguration.RingSize + 1).ConfigureAwait(false);
            RequestResult requestResult = await _executionContext.TransactionsService.SendCompromisedProofs(requestInput, keyImage, transactionKey, destinationKey, outputModels, issuanceCommitments).ConfigureAwait(false);

            IEnumerable<UserRootAttribute> userRootAttributes = _dataAccessService.GetUserAttributes(_executionContext.AccountId).Where(u => !u.IsOverriden);

            foreach (UserRootAttribute userAttribute in userRootAttributes)
            {
                await SendRevokeIdentity(userAttribute).ConfigureAwait(false);
            }

            _dataAccessService.SetAccountCompromised(_executionContext.AccountId);

            await _stateNotificationService.NotificationsPipe.SendAsync(new AccountCompomisedStateNotification
            {
                KeyImage = keyImage,
                Target = target
            }).ConfigureAwait(false);
        }

        private UserRootAttribute GetRootAttributeOnTransactionKeyArriving(byte[] transactionKey)
        {
            UserRootAttribute rootAttribute;
            int counter = 0;
            do
            {
                IEnumerable<UserRootAttribute> userAttributes = _dataAccessService.GetUserAttributes(_executionContext.AccountId).Where(u => !u.IsOverriden && !u.LastCommitment.Equals32(new byte[32]));
                rootAttribute = userAttributes.FirstOrDefault(a => a.LastTransactionKey.Equals32(transactionKey));

                if (rootAttribute == null)
                {
                    counter++;
                    Thread.Sleep(500);
                }
            } while (rootAttribute == null && counter <= 10);
            return rootAttribute;
        }

        private async Task SendRevokeIdentity(UserRootAttribute rootAttribute)
        {
            byte[] target = rootAttribute.Source.HexStringToByteArray();
            byte[] issuer = rootAttribute.Source.HexStringToByteArray();
            byte[] assetId = rootAttribute.AssetId;
            byte[] originalBlindingFactor = rootAttribute.OriginalBlindingFactor;
            byte[] originalCommitment = rootAttribute.OriginalCommitment;
            byte[] lastTransactionKey = rootAttribute.LastTransactionKey;
            byte[] lastBlindingFactor = rootAttribute.LastBlindingFactor;
            byte[] lastCommitment = rootAttribute.LastCommitment;
            byte[] lastDestinationKey = rootAttribute.LastDestinationKey;

            RequestInput requestInput = new RequestInput
            {
                AssetId = assetId,
                EligibilityBlindingFactor = originalBlindingFactor,
                EligibilityCommitment = originalCommitment,
                Issuer = issuer,
                PrevAssetCommitment = lastCommitment,
                PrevBlindingFactor = lastBlindingFactor,
                PrevDestinationKey = lastDestinationKey,
                PrevTransactionKey = lastTransactionKey,
                PublicSpendKey = target
            };

            OutputModel[] outputModels = await _gatewayService.GetOutputs(_restApiConfiguration.RingSize + 1).ConfigureAwait(false);
            RequestResult requestResult = await _executionContext.TransactionsService.SendRevokeIdentity(requestInput, outputModels, new byte[][] { rootAttribute.IssuanceCommitment }).ConfigureAwait(false);
        }
    }
}
