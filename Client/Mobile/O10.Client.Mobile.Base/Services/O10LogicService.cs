using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using O10.Client.Common.Configuration;
using O10.Client.Common.Dtos.UniversalProofs;
using O10.Client.Common.Entities;
using O10.Client.Common.Interfaces;
using O10.Client.Common.Interfaces.Inputs;
using O10.Client.DataLayer.Services;
using O10.Core;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.ExtensionMethods;
using O10.Core.Logging;
using O10.Client.Mobile.Base.Exceptions;
using O10.Client.Mobile.Base.Interfaces;
using O10.Transactions.Core.DTOs;

namespace O10.Client.Mobile.Base.Services
{
    [RegisterDefaultImplementation(typeof(IO10LogicService), Lifetime = LifetimeManagement.Singleton)]
    public class O10LogicService : IO10LogicService
    {
        private readonly IExecutionContext _executionContext;
        private readonly IAccountsService _accountsService;
        private readonly IDataAccessService _dataAccessService;
        private readonly ISchemeResolverService _schemeResolverService;
        private readonly IGatewayService _gatewayService;
        private readonly IRestApiConfiguration _restApiConfiguration;
        private readonly ILogger _logger;

        public O10LogicService(IExecutionContext executionContext,
                                   IAccountsService accountsService,
                                   IDataAccessService dataAccessService,
                                   ISchemeResolverService schemeResolverService,
                                   IGatewayService gatewayService,
                                   ILoggerService loggerService,
                                   IConfigurationService configurationService)
        {
            _executionContext = executionContext;
            _accountsService = accountsService;
            _dataAccessService = dataAccessService;
            _schemeResolverService = schemeResolverService;
            _gatewayService = gatewayService;
            _logger = loggerService.GetLogger(nameof(O10LogicService));
            _restApiConfiguration = configurationService.Get<IRestApiConfiguration>();
        }

        public async Task SendIdentityProofs(RequestInput requestInput)
        {
            if (requestInput is null)
            {
                throw new ArgumentNullException(nameof(requestInput));
            }

            OutputSources[] outputModels = await _gatewayService.GetOutputs(_restApiConfiguration.RingSize + 1).ConfigureAwait(false);

            RequestResult requestResult = await _executionContext.TransactionsService.SendIdentityProofs(requestInput, null, outputModels, requestInput.Issuer).ConfigureAwait(false);
        }

        public async Task<bool> StoreRegistration(byte[] target, string spInfo, Memory<byte> issuer, params Memory<byte>[] assetIds)
        {
            string issuerStr = issuer.ToHexString();
            string assetIdStr = string.Join(',', assetIds.Select(a => a.ToString()));

            _logger.LogIfDebug(() => $"Storing user registration at {spInfo}, assetId: {assetIdStr}, issuer: {issuerStr}");

            (_, byte[] registrationCommitment) = await _executionContext.RelationsBindingService.GetBoundedCommitment(target, assetIds).ConfigureAwait(false);
            long registrationId = _dataAccessService.AddUserRegistration(_executionContext.AccountId, registrationCommitment.ToHexString(), spInfo, assetIdStr, issuerStr);
            if (registrationId > 0)
            {
                _logger.LogIfDebug(() => $"New user registration {registrationCommitment.ToHexString()} added for {spInfo}, assetId: {assetIdStr}, issuer: {issuerStr}");
                try
                {
                    bool res = await _schemeResolverService.StoreRegistrationCommitment(issuerStr, assetIdStr, registrationCommitment.ToHexString(), spInfo).ConfigureAwait(false);
                    if (!res)
                    {
                        _logger.Error($"Failed to store user registration remotely, registration: {registrationCommitment.ToHexString()}, spInfo: {spInfo}, assetId: {assetIdStr}, issuer: {issuerStr}");
                        _dataAccessService.RemoveUserRegistration(registrationId);
                    }
                    else
                    {
                        _logger.LogIfDebug(() => $"New user registration at {spInfo} stored successfully");
                    }

                    return res;
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to store Inherence Registration Commitment", ex);
                }
            }
            else
            {
                _logger.LogIfDebug(() => $"User registration {registrationCommitment.ToHexString()} at {spInfo} already exists");
            }

            return true;
        }

        public async Task StoreAssociatedAttributes(string rootIssuer, byte[] rootAssetId, string associatedIssuer, IEnumerable<AttributeValue> attributeValues)
        {
            _logger.LogIfDebug(() => $"Storing associated attributes with {nameof(rootIssuer)}={rootIssuer}, {nameof(rootAssetId)}={rootAssetId.ToHexString()}, {nameof(associatedIssuer)}={associatedIssuer} and values: {string.Join(',', attributeValues.Select(a => $"[{a.Definition.SchemeName}:{a.Value}]"))}");

            try
            {
                await _schemeResolverService.BackupAssociatedAttributes(rootIssuer, rootAssetId.ToHexString(), attributeValues.Select(a => new AssociatedAttributeBackupDTO
                {
                    AssociatedIssuer = associatedIssuer,
                    SchemeName = a.Definition.SchemeName,
                    Content = a.Value
                }).ToArray()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex is FlurlHttpException httpException)
                {
                    string response = await httpException.Call.Response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    _logger.Error($"Failed to store associated attributes due to error {httpException.Call.HttpStatus}, response: {response}");
                }
                else if (ex is AggregateException aex)
                {
                    if (ex.InnerException is FlurlHttpException httpException1)
                    {
                        string response = await httpException1.Call.Response.Content.ReadAsStringAsync().ConfigureAwait(false);

                        _logger.Error($"Failed to store associated attributes due to error {httpException1.Call.HttpStatus}, response: {response}");
                    }
                    else
                    {
                        _logger.Error($"Failed to store associated attributes due to aggregated exception {aex.InnerException.Message}");
                    }
                }
                else
                {
                    _logger.Error($"Failed to store associated attributes due to exception {ex.InnerException.Message}");
                }
            }

            _dataAccessService.UpdateUserAssociatedAttributes(_executionContext.AccountId, associatedIssuer, attributeValues.Select(a => new Tuple<string, string>(a.Definition.SchemeName, a.Value)), rootAssetId);
        }

        public async Task SendUniversalTransport(RequestInput requestInput, UniversalProofs universalProofs, string serviceProviderInfo, bool storeRegistration = false)
        {
            if (requestInput is null)
            {
                throw new ArgumentNullException(nameof(requestInput));
            }

            if (universalProofs is null)
            {
                throw new ArgumentNullException(nameof(universalProofs));
            }

            if (string.IsNullOrEmpty(serviceProviderInfo))
            {
                throw new ArgumentException($"'{nameof(serviceProviderInfo)}' cannot be null or empty.", nameof(serviceProviderInfo));
            }

            OutputSources[] outputModels = await _gatewayService.GetOutputs(_restApiConfiguration.RingSize + 1).ConfigureAwait(false);
            await _executionContext.TransactionsService.SendUniversalTransport(requestInput, outputModels, universalProofs)
                .ContinueWith(t =>
                {
                    _dataAccessService.AddUserTransactionSecret(_executionContext.AccountId,
                                                                universalProofs.KeyImage.ToString(),
                                                                universalProofs.Issuer.ToString(),
                                                                requestInput.AssetId.ToHexString(),
                                                                t.Result.NewBlindingFactor.ToHexString());
                }, TaskScheduler.Current)
                .ConfigureAwait(false);

            bool postSucceeded = false;
            await _restApiConfiguration
                .UniversalProofsPoolUri.PostJsonAsync(universalProofs)
                .ContinueWith(t =>
                {
                    if (!t.IsCompletedSuccessfully)
                    {
                        string response = AsyncUtil.RunSync(async () => await ((FlurlHttpException)t.Exception.InnerException).Call.Response.Content.ReadAsStringAsync().ConfigureAwait(false));
                        _logger.Error($"Failure during posting Universal Proofs", t.Exception.InnerException);
                        throw new UniversalProofsSendingFailedException(t.Exception.InnerException.Message, t.Exception.InnerException);

                    }
                    else
                    {
                        postSucceeded = true;
                    }
                }, TaskScheduler.Current)
                .ConfigureAwait(false);

            if (postSucceeded && storeRegistration)
            {
                await StoreRegistration(requestInput.PublicSpendKey, serviceProviderInfo, requestInput.Issuer, requestInput.AssetId).ConfigureAwait(false);
            }
        }
    }
}
