using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.DataModel.Stealth;
using O10.Client.Common.Entities;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Enums;
using O10.Client.DataLayer.Model.Inherence;
using O10.Client.DataLayer.Services;
using O10.Client.Web.Common;
using O10.Client.Web.Common.Configuration;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.Models;
using O10.Core.ExtensionMethods;
using O10.Core.Cryptography;
using Microsoft.AspNetCore.SignalR;
using O10.Core.Logging;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using O10.Core.Identity;
using O10.Client.Common.Dtos.UniversalProofs;
using Microsoft.Extensions.DependencyInjection;
using O10.Client.Common.Exceptions;
using O10.Core.Serialization;
using O10.Core.Notifications;

namespace O10.Client.Web.Portal.Services.Inherence
{
    [RegisterExtension(typeof(IInherenceService), Lifetime = LifetimeManagement.Singleton)]
    public class O10InherenceService : IInherenceService
    {
        public const string NAME = "O10Inherence";

        private readonly ConcurrentDictionary<string, TaskCompletionSource<InherenceData>> _sessions;
        private readonly IDataAccessService _dataAccessService;
        private readonly IAccountsService _accountsService;
        private readonly IProofsValidationService _spValidationsService;
        private readonly IHubContext<O10InherenceHub> _o10InherenceHubContext;
        private readonly IAzureConfiguration _azureConfiguration;
        private readonly IExecutionContextManager _executionContextManager;
        private readonly IUniversalProofsPool _universalProofsPool;
        private readonly IIdentityKeyProvider _identityKeyProvider;
        private readonly ILogger _logger;
        private IStateClientCryptoService _clientCryptoService;

        public O10InherenceService(IDataAccessService dataAccessService,
                              IAccountsService accountsService,
                              IConfigurationService configurationService,
                              IProofsValidationService spValidationsService,
                              IExecutionContextManager executionContextManager,
                              IUniversalProofsPool universalProofsPool,
                              IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
                              IHubContext<O10InherenceHub> o10InherenceHubContext,
                              ILoggerService loggerService)
        {
            _sessions = new ConcurrentDictionary<string, TaskCompletionSource<InherenceData>>();
            _dataAccessService = dataAccessService;
            _accountsService = accountsService;
            _spValidationsService = spValidationsService;
            _o10InherenceHubContext = o10InherenceHubContext;
            _azureConfiguration = configurationService.Get<IAzureConfiguration>();
            _logger = loggerService.GetLogger(nameof(O10InherenceService));
            _executionContextManager = executionContextManager;
            _universalProofsPool = universalProofsPool;
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
            PipeIn = new ActionBlock<PacketBase>(
                async p =>
                {
                    try
                    {
                        if (p is IdentityProofs identityProofs)
                        {
                            ProcessIdentityProofs(identityProofs);
                        }
                        else if (p is UniversalTransport universalTransport)
                        {
                            await ProcessUniversalTransport(universalTransport).ConfigureAwait(false);
                        }
                    }
                    catch
                    {
                        _logger.Error("Unexpected exception at PipeIn");
                    }
                });

            PipeInNotifications = new ActionBlock<NotificationBase>(n =>
            {
            });
        }

        public string Name => NAME;

        public string Alias => "O10 Inherence";

        public string Description => "Face verification with Microsoft Cognitive Services";
        public string Target { get; private set; }

        public ITargetBlock<PacketBase> PipeIn { get; set; }

        public ITargetBlock<NotificationBase> PipeInNotifications { get; }

        public long AccountId { get; private set; }

        public void Initialize(CancellationToken cancellationToken)
        {
            _logger.Info($"Initializing {nameof(O10InherenceService)}");

            InherenceSetting inherenceSetting = _dataAccessService.GetInherenceSetting(Name);

            if (inherenceSetting == null)
            {
                inherenceSetting = CreateO10Inherence();
            }

            AccountId = inherenceSetting.AccountId;
            _logger.LogIfDebug(() => $"[{AccountId}]: {nameof(Initialize)} proceeding");

            AccountDescriptor accountDescriptor = _accountsService.Authenticate(inherenceSetting.AccountId, GetDefaultO10InherencePassword());
            if (accountDescriptor == null)
            {
                _dataAccessService.RemoveInherenceSetting(Name);
                inherenceSetting = CreateO10Inherence();
                accountDescriptor = _accountsService.Authenticate(inherenceSetting.AccountId, GetDefaultO10InherencePassword());
                if (accountDescriptor == null)
                {
                    throw new Exception($"{nameof(O10InherenceService)} initialization failed");
                }
            }

            _logger.Info($"[{AccountId}]: Invoking InitializeStateExecutionServices");
            var persistency = _executionContextManager.InitializeStateExecutionServices(
                accountDescriptor.AccountId, accountDescriptor.SecretSpendKey, this);

            _clientCryptoService = persistency.Scope.ServiceProvider.GetService<IStateClientCryptoService>();

            Target = accountDescriptor.PublicSpendKey.ToHexString();

            cancellationToken.Register(() =>
            {
                _executionContextManager.UnregisterExecutionServices(AccountId);
            });
        }

        private InherenceSetting CreateO10Inherence()
        {
            _logger.Info("CreateO10Inherence");
            AccountId = _accountsService.Create(AccountType.ServiceProvider, nameof(O10InherenceService), GetDefaultO10InherencePassword(), true);
            InherenceSetting inherenceSetting = _dataAccessService.AddInherenceSetting(Name, AccountId);
            _logger.LogIfDebug(() => $"[{AccountId}]: {nameof(CreateO10Inherence)} account created");

            return inherenceSetting;
        }

        private string GetDefaultO10InherencePassword()
        {
            string secretName = "ConsentManagementPassword";

            return AzureHelper.GetSecretValue(secretName, _azureConfiguration.AzureADCertThumbprint, _azureConfiguration.AzureADApplicationId, _azureConfiguration.KeyVaultName);
        }

        private async Task ProcessUniversalTransport(UniversalTransport universalTransport)
        {
            _logger.LogIfDebug(() => $"[{AccountId}]: {nameof(ProcessUniversalTransport)} with {nameof(universalTransport.KeyImage)}={universalTransport.KeyImage}");

            TaskCompletionSource<UniversalProofs> universalProofsTask = _universalProofsPool.Extract(universalTransport.KeyImage);

            try
            {
                UniversalProofs universalProofs = await universalProofsTask.Task.ConfigureAwait(false);

                _logger.LogIfDebug(() => $"[{AccountId}]: {nameof(ProcessUniversalTransport)}, {nameof(UniversalProofs)} obtained with {nameof(universalProofs.KeyImage)}={universalProofs.KeyImage} and {nameof(universalProofs.SessionKey)}={universalProofs.SessionKey}");

                var mainIssuer = universalProofs.RootIssuers.Find(i => i.Issuer.Equals(universalProofs.MainIssuer));
                IKey commitmentKey = mainIssuer.IssuersAttributes.FirstOrDefault(a => a.Issuer.Equals(mainIssuer.Issuer))?.RootAttribute.Commitment;
                SurjectionProof eligibilityProof = mainIssuer.IssuersAttributes.FirstOrDefault(a => a.Issuer.Equals(mainIssuer.Issuer))?.RootAttribute.BindingProof;

                bool isEligibilityCorrect = await CheckEligibilityProofs(commitmentKey.Value, eligibilityProof, mainIssuer.Issuer.Value).ConfigureAwait(false);

                if (!isEligibilityCorrect && !string.IsNullOrEmpty(universalProofs.SessionKey))
                {
                    SetException(universalProofs.SessionKey, new ArgumentException("Eligibility proofs were wrong"));
                    return;
                }

                SurjectionProof registrationProof = mainIssuer.IssuersAttributes.FirstOrDefault(a => a.Issuer.Equals(mainIssuer.Issuer))?.RootAttribute.CommitmentProof.SurjectionProof;
                _spValidationsService.HandleRegistration(AccountId, commitmentKey.Value, registrationProof);

                SetCompletion(
                    new InherenceData
                    {
                        Issuer = mainIssuer.Issuer.ArraySegment.Array,
                        AssetRootCommitment = commitmentKey.ArraySegment.Array,
                        RootRegistrationProof = registrationProof,
                        AssociatedRootCommitment = mainIssuer.IssuersAttributes?.FirstOrDefault(a => !a.Issuer.Equals(mainIssuer.Issuer))?.RootAttribute.Commitment.ArraySegment.Array,
                        AssociatedRegistrationProof = mainIssuer.IssuersAttributes?.FirstOrDefault(a => !a.Issuer.Equals(mainIssuer.Issuer))?.RootAttribute.CommitmentProof.SurjectionProof
                    }, universalProofs.SessionKey);
            }
            catch (TimeoutException)
            {
                _logger.Error($"[{AccountId}]: Timeout during obtaining {nameof(UniversalProofs)} for key image {universalTransport.KeyImage}");
            }
            catch (Exception ex)
            {
                if (ex is AggregateException aex)
                {
                    _logger.Error($"[{AccountId}]: {nameof(ProcessUniversalTransport)}, unexpected aggregated exception", aex.InnerException);
                }
                else
                {
                    _logger.Error($"[{AccountId}]: {nameof(ProcessUniversalTransport)}, unexpected exception", ex);
                }

                throw;
            }
        }

        private async void ProcessIdentityProofs(IdentityProofs packet)
        {
            byte[] keyImage = packet.KeyImage.Value.ToArray();

            _clientCryptoService.DecodeEcdhTuple(packet.EncodedPayload, packet.TransactionPublicKey, out byte[] blindingFactor, out byte[] assetId, out byte[] issuer, out byte[] payload);
            string sessionKey = payload.ToHexString();

            bool isEligibilityCorrect = await CheckEligibilityProofs(packet.AssetCommitment, packet.EligibilityProof, issuer).ConfigureAwait(false);

            if (!isEligibilityCorrect)
            {
                await _o10InherenceHubContext.Clients.Group(sessionKey).SendAsync("AuthenticationFailed", new { Code = 2, Message = "Eligibility proofs were wrong" }).ConfigureAwait(false);
                SetCompletion(null, sessionKey);
                return;
            }

            (long registrationId, bool isNew) = _spValidationsService.HandleRegistration(AccountId, packet.AssetCommitment, packet.AuthenticationProof);

            if (isNew)
            {
                // TODO: if account is new so SignalR will call to function of taking image
                await _o10InherenceHubContext.Clients.Group(sessionKey).SendAsync("Register").ConfigureAwait(false);
            }
            else
            {
                await _o10InherenceHubContext.Clients.Group(sessionKey).SendAsync("Verify").ConfigureAwait(false);
            }

            SetCompletion(new InherenceData { AssetRootCommitment = packet.AssetCommitment, RootRegistrationProof = packet.AuthenticationProof }, sessionKey);
        }

        private void SetCompletion(InherenceData packet, string sessionKey)
        {
            _logger.LogIfDebug(() => $"[{AccountId}]: {nameof(SetCompletion)}, {nameof(sessionKey)}={sessionKey}, {nameof(packet)}={{0}}", packet);
            TaskCompletionSource<InherenceData> taskCompletion = _sessions.GetOrAdd(sessionKey, key => new TaskCompletionSource<InherenceData>());
            taskCompletion.SetResult(packet);
        }

        private void SetException(string sessionKey, Exception ex)
        {
            _logger.Error($"[{AccountId}]: {nameof(SetException)}, {nameof(sessionKey)}={sessionKey}", ex);
            TaskCompletionSource<InherenceData> taskCompletion = _sessions.GetOrAdd(sessionKey, key => new TaskCompletionSource<InherenceData>());
            taskCompletion.SetException(ex);
        }

        private async Task<bool> CheckEligibilityProofs(Memory<byte> assetCommitment, SurjectionProof eligibilityProofs, Memory<byte> issuer)
        {
            _logger.LogIfDebug(() => $"[{AccountId}]: {nameof(CheckEligibilityProofs)} with assetCommitment={assetCommitment.ToHexString()}, issuer={issuer.ToHexString()}, eligibilityProofs={JsonConvert.SerializeObject(eligibilityProofs, new ByteArrayJsonConverter())}");

            try
            {
                await _spValidationsService.CheckEligibilityProofs(assetCommitment, eligibilityProofs, issuer).ConfigureAwait(false);
            }
            catch (CommitmentNotEligibleException)
            {
                _logger.Error($"[{AccountId}]: {nameof(CheckEligibilityProofs)} found commitment not eligible");
                return false;
            }

            _logger.Debug($"[{AccountId}]: {nameof(CheckEligibilityProofs)} correct");
            return true;
        }

        public TaskCompletionSource<InherenceData> GetIdentityProofsAwaiter(string sessionKey)
        {
            _logger.Debug($"[{AccountId}]: {nameof(GetIdentityProofsAwaiter)}, {nameof(sessionKey)}={sessionKey}");
            TaskCompletionSource<InherenceData> taskCompletion = _sessions.GetOrAdd(sessionKey, key => new TaskCompletionSource<InherenceData>());
            return taskCompletion;
        }

        public void RemoveIdentityProofsAwaiter(string sessionKey)
        {
            _logger.Debug($"[{AccountId}]: {nameof(RemoveIdentityProofsAwaiter)}, {nameof(sessionKey)}={sessionKey}");
            _sessions.TryRemove(sessionKey, out _);
        }

        public void Initialize(long accountId, CancellationToken cancellationToken)
        {
            AccountId = accountId;
            cancellationToken.Register(() =>
            {
                PipeIn?.Complete();
                PipeInNotifications?.Complete();
            });
        }
    }
}
