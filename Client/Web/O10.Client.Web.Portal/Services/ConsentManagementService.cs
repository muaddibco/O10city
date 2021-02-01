using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.DataModel.Stealth;
using O10.Client.Common.Entities;
using O10.Client.Common.Interfaces;
using O10.Client.Common.Interfaces.Inputs;
using O10.Client.DataLayer.Enums;
using O10.Client.DataLayer.Model.ConsentManagement;
using O10.Client.DataLayer.Services;
using O10.Client.Web.Common;
using O10.Client.Web.Common.Configuration;
using O10.Core;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.ExtensionMethods;
using O10.Core.HashCalculations;
using O10.Core.Logging;
using O10.Core.Models;
using O10.Crypto.ConfidentialAssets;
using O10.Client.Web.Portal.Dtos.ServiceProvider;
using O10.Client.Web.Portal.Hubs;
using Microsoft.Extensions.DependencyInjection;
using O10.Core.Serialization;
using O10.Core.Notifications;

namespace O10.Client.Web.Portal.Services
{
    [RegisterDefaultImplementation(typeof(IConsentManagementService), Lifetime = LifetimeManagement.Singleton)]
    public class ConsentManagementService : IConsentManagementService
    {
        private readonly IDataAccessService _dataAccessService;
        private readonly IAccountsService _accountsService;
        private readonly IRelationsProofsValidationService _relationsProofsValidationService;
        private readonly IHashCalculation _hashCalculation;
        private readonly IHubContext<ConsentManagementHub> _hubContext;
        private readonly IAzureConfiguration _azureConfiguration;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, RelationProofsSession> _relationProofSessions = new ConcurrentDictionary<string, RelationProofsSession>();
        private readonly ConcurrentDictionary<string, ProofsSession> _proofsSessions = new ConcurrentDictionary<string, ProofsSession>();
        private readonly ConcurrentDictionary<string, TransactionConsentRequest> _consentRequests = new ConcurrentDictionary<string, TransactionConsentRequest>();
        private readonly ConcurrentDictionary<string, List<string>> _consentsPerRegistration = new ConcurrentDictionary<string, List<string>>();
        private readonly ConcurrentDictionary<string, string> _consentRequestsByConfirmKey = new ConcurrentDictionary<string, string>();
        private readonly ConcurrentDictionary<string, string> _consentRequestsByDeclineKey = new ConcurrentDictionary<string, string>();
        private readonly ConcurrentDictionary<string, string> _consentRequestsByKeyImage = new ConcurrentDictionary<string, string>();

        private IStealthClientCryptoService _clientCryptoService;
        private IExecutionContextManager _executionContextManager;
        private long _accountId;

        public ConsentManagementService(IDataAccessService dataAccessService, IAccountsService accountsService,
            IRelationsProofsValidationService relationsProofsValidationService, IConfigurationService configurationService,
            IHashCalculationsRepository hashCalculationsRepository, ILoggerService loggerService, IHubContext<ConsentManagementHub> hubContext)
        {
            _logger = loggerService.GetLogger(nameof(ConsentManagementService));
            _dataAccessService = dataAccessService;
            _accountsService = accountsService;
            _relationsProofsValidationService = relationsProofsValidationService;
            _hubContext = hubContext;
            _azureConfiguration = configurationService.Get<IAzureConfiguration>();
            _hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);

            PipeIn = new ActionBlock<PacketBase>(async p =>
            {
                try
                {
                    if (p is GroupsRelationsProofs relationsProofs)
                    {
                        _logger.LogIfDebug(() => $"[{_accountId}]: checking relation proofs {JsonConvert.SerializeObject(relationsProofs, new ByteArrayJsonConverter())}");

                        var persistency = _executionContextManager.ResolveExecutionServices(_accountId);
                        var clientCryptoService = persistency.Scope.ServiceProvider.GetService<IStealthClientCryptoService>();
                        clientCryptoService.DecodeEcdhTuple(relationsProofs.EcdhTuple, relationsProofs.TransactionPublicKey, out byte[] blindingFactor, out byte[] imageHash, out byte[] issuer, out byte[] sessionKey);
                        string keyImage = relationsProofs.KeyImage.ToString();

                        _proofsSessions.AddOrUpdate(keyImage, new ProofsSession { SessionKey = sessionKey.ToHexString(), CreationTime = DateTime.UtcNow }, (k, v) => v);

                        RelationProofsSession relationProofsSession = PopRelationProofSession(sessionKey.ToHexString());

                        _logger.LogIfDebug(() => $"{nameof(relationProofsSession)}={JsonConvert.SerializeObject(relationProofsSession, new ByteArrayJsonConverter())}");

                        RelationProofsValidationResults validationResults
                            = await _relationsProofsValidationService
                            .VerifyRelationProofs(relationsProofs, _clientCryptoService, relationProofsSession)
                            .ConfigureAwait(false);

                        await _hubContext.Clients.Group(sessionKey.ToHexString()).SendAsync("ValidationResults", validationResults).ConfigureAwait(false);
                    }
                    else if (p is TransitionCompromisedProofs compromisedProofs)
                    {
                        if (_proofsSessions.TryGetValue(compromisedProofs.CompromisedKeyImage.ToHexString(), out ProofsSession proofsSession))
                        {
                            await _hubContext.Clients.Group(proofsSession.SessionKey).SendAsync("ProofsCompromised", proofsSession).ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"[{_accountId}]: failure during processing {p.GetType().Name}", ex);
                }
            });

            PipeInNotifications = new ActionBlock<NotificationBase>(n =>
            {
            });
        }

        public string PublicSpendKey { get; private set; }
        public string PublicViewKey { get; private set; }

        public ITargetBlock<PacketBase> PipeIn { get; set; }

        public ITargetBlock<NotificationBase> PipeInNotifications { get; }

        public void Initialize(IExecutionContextManager executionContextManager, CancellationToken cancellationToken)
        {
            _executionContextManager = executionContextManager;
            ConsentManagementSettings settings = _dataAccessService.GetConsentManagementSettings();
            if (settings == null)
            {
                settings = CreateNewConsentManagementServiceAccount();
            }

            AccountDescriptor accountDescriptor = _accountsService.Authenticate(settings.AccountId, GetDefaultConsentManagementPassword());
            if (accountDescriptor == null)
            {
                settings = CreateNewConsentManagementServiceAccount();
                accountDescriptor = _accountsService.Authenticate(settings.AccountId, GetDefaultConsentManagementPassword());
                if (accountDescriptor == null)
                {
                    throw new Exception("ConsentManagementService initialization failed");
                }
            }

            _accountId = accountDescriptor.AccountId;

            var persistency = _executionContextManager.InitializeUtxoExecutionServices(
                accountDescriptor.AccountId,
                accountDescriptor.SecretSpendKey,
                accountDescriptor.SecretViewKey,
                accountDescriptor.PwdHash,
                this);


            _clientCryptoService = persistency.Scope.ServiceProvider.GetService<IStealthClientCryptoService>();
            
            PublicSpendKey = accountDescriptor.PublicSpendKey.ToHexString();
            PublicViewKey = accountDescriptor.PublicViewKey.ToHexString();

            cancellationToken.Register(() =>
            {
                _executionContextManager.UnregisterExecutionServices(_accountId);
            });
        }

        public RelationProofsSession PopRelationProofSession(string sessionKey)
        {
            if (_relationProofSessions.TryRemove(sessionKey, out RelationProofsSession relationProofSession))
            {
                return relationProofSession;
            }

            return null;
        }

        public bool PushRelationProofsData(string sessionKey, RelationProofsData relationProofData)
        {
            bool res = _relationProofSessions.TryGetValue(sessionKey, out RelationProofsSession relationProofSession);

            if (res)
            {
                relationProofSession.ProofsData = relationProofData;
            }

            return res;
        }

        public string InitiateRelationProofsSession(ProofsRequest proofsRequest)
        {
            string sessionKey = ConfidentialAssetsHelper.GetRandomSeed().ToHexString();
            _relationProofSessions.AddOrUpdate(sessionKey, new RelationProofsSession { ProofsRequest = proofsRequest }, (k, v) => v);
            return sessionKey;
        }

        private ConsentManagementSettings CreateNewConsentManagementServiceAccount()
        {
            ConsentManagementSettings settings;
            long accountId = _accountsService.Create(AccountType.User, nameof(ConsentManagementService), GetDefaultConsentManagementPassword(), true);
            settings = new ConsentManagementSettings
            {
                AccountId = accountId
            };
            _dataAccessService.SetConsentManagementSettings(settings);
            return settings;
        }

        private string GetDefaultConsentManagementPassword()
        {
            string secretName = "ConsentManagementPassword";

            return AzureHelper.GetSecretValue(secretName, _azureConfiguration.AzureADCertThumbprint, _azureConfiguration.AzureADApplicationId, _azureConfiguration.KeyVaultName);
        }

        public IEnumerable<SpUserTransactionDto> GetUserTransactions(long spAccountId)
        {
            IEnumerable<SpUserTransactionDto> transactionDtos = _dataAccessService.GetSpUserTransactions(spAccountId)
                .Select(s =>
                    new SpUserTransactionDto
                    {
                        SpUserTransactionId = s.SpUserTransactionId.ToString(),
                        RegistrationId = s.ServiceProviderRegistrationId.ToString(),
                        TransactionId = s.TransactionId,
                        Description = s.TransactionDescription,
                        IsProcessed = s.IsProcessed,
                        IsConfirmed = s.IsConfirmed,
                        IsCompromised = s.IsCompromised
                    });

            return transactionDtos;
        }

        public void RegisterTransactionForConsent(TransactionConsentRequest consentRequest)
        {
            _consentRequests.AddOrUpdate(consentRequest.TransactionId, consentRequest, (k, v) => consentRequest);
            byte[] confirmHash = _hashCalculation.CalculateHash(Encoding.UTF8.GetBytes(consentRequest.TransactionId));
            byte[] declineHash = _hashCalculation.CalculateHash(confirmHash);

            _consentRequestsByConfirmKey.AddOrUpdate(confirmHash.ToHexString(), consentRequest.TransactionId, (k, v) => confirmHash.ToHexString());
            _consentRequestsByDeclineKey.AddOrUpdate(declineHash.ToHexString(), consentRequest.TransactionId, (k, v) => declineHash.ToHexString());

            _consentsPerRegistration.AddOrUpdate(consentRequest.RegistrationCommitment, new List<string> { consentRequest.TransactionId }, (k, v) => { v.Add(consentRequest.TransactionId); return v; });
        }

        public IEnumerable<TransactionConsentRequest> GetTransactionConsentRequests(string registrationCommitment)
        {
            List<TransactionConsentRequest> consentRequests = new List<TransactionConsentRequest>();

            if (_consentsPerRegistration.TryGetValue(registrationCommitment, out List<string> transactionIds))
            {

                foreach (var transactionId in transactionIds)
                {
                    if (_consentRequests.TryGetValue(transactionId, out TransactionConsentRequest consentRequest))
                    {
                        consentRequests.Add(consentRequest);
                    }
                }
            }

            return consentRequests;
        }

        public bool TryGetTransactionRequestByKey(string key, string keyImage, out string transactionId, out bool confirmed)
        {
            if (_consentRequestsByConfirmKey.TryGetValue(key, out transactionId))
            {
                confirmed = true;
                _consentRequestsByKeyImage.AddOrUpdate(keyImage, transactionId, (k, v) => v);
                return true;
            }

            if (_consentRequestsByDeclineKey.TryGetValue(key, out transactionId))
            {
                confirmed = false;
                _consentRequestsByKeyImage.AddOrUpdate(keyImage, transactionId, (k, v) => v);
                return true;
            }

            transactionId = null;
            confirmed = false;
            return false;
        }

        public string GetTransactionRequestByKeyImage(string keyImage)
        {
            if (_consentRequestsByKeyImage.TryGetValue(keyImage, out string transactionId))
            {
                return transactionId;
            }

            return null;
        }

        public void Initialize(long accountId, CancellationToken cancellationToken)
        {
            _accountId = accountId;

            cancellationToken.Register(() =>
            {
                PipeIn?.Complete();
                PipeInNotifications?.Complete();
            });
        }
    }
}
