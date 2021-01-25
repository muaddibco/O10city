using Nethereum.BlockchainProcessing.BlockStorage.Entities.Mapping;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using O10.Client.Common.Integration;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Services;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.Logging;
using O10.Integrations.Rsk.Web.Configuration;
using O10Idp.Contracts.O10Identity;
using O10Idp.Contracts.O10Identity.ContractDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace O10.Integrations.Rsk.Web
{
    [RegisterExtension(typeof(IIntegrationIdP), Lifetime = LifetimeManagement.Singleton)]
    public class IntegrationIdP : IIntegrationIdP
    {
        public const string RSK_ADDR = "RskAddr";
        private readonly IIntegrationConfiguration _integrationConfiguration;
        private readonly IAccountsService _accountsService;
        private readonly IDataAccessService _dataAccessService;
        private readonly ILogger _logger;
        static SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public string Key => "RSK";

        public IntegrationIdP(
            IAccountsService accountsService,
            IDataAccessService dataAccessService,
            IConfigurationService configurationService,
            ILoggerService loggerService)
        {
            _integrationConfiguration = configurationService.Get<IIntegrationConfiguration>();
            _accountsService = accountsService;
            _dataAccessService = dataAccessService;
            _logger = loggerService.GetLogger(GetType().FullName);
        }

        public void Initialize()
        {
        }

        public async Task<ActionStatus> Register(long accountId)
        {
            ActionStatus actionStatus = new ActionStatus
            {
                IntegrationType = Key,
                IntegrationAction = "Register",
                ActionSucceeded = true
            };

            await _semaphoreSlim.WaitAsync();
            var privateKey = _dataAccessService.GetAccountKeyValue(accountId, "RskSecretKey");
            if(string.IsNullOrEmpty(privateKey))
            {
                var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
                privateKey = ecKey.GetPrivateKeyAsBytes().ToHex();
                _dataAccessService.SetAccountKeyValues(accountId, new Dictionary<string, string> { { "RskSecretKey", privateKey } });
            }

            var account = new Account(privateKey);
            actionStatus.IntegrationAddress = account.Address;

            var web3 = new Web3(account, _integrationConfiguration.RpcUri);
            var balance = await web3.Eth.GetBalance.SendRequestAsync(account.Address).ConfigureAwait(false);
            
            var o10IdentityService = new O10IdentityService(web3, _integrationConfiguration.ContractAddress);

            try
            {
                var issuers = await o10IdentityService.GetAllIssuersQueryAsync().ConfigureAwait(false);
                if (!issuers.ReturnValue1.Any(issuers => issuers.Address.Equals(account.Address, StringComparison.InvariantCultureIgnoreCase)))
                {
                    var registerFunctionTransactionHandler = web3.Eth.GetContractTransactionHandler<RegisterFunction>();
                    RegisterFunction registerFunction = new RegisterFunction
                    {
                        AliasName = _accountsService.GetById(accountId).AccountInfo
                    };
                    var esimatedGas = await registerFunctionTransactionHandler.EstimateGasAsync(_integrationConfiguration.ContractAddress, registerFunction).ConfigureAwait(false);
                    registerFunction.Gas = new BigInteger(esimatedGas.ToLong() * 1.1);
                    if (balance.Value >= registerFunction.Gas)
                    {
                        var rcpt = await o10IdentityService.RegisterRequestAndWaitForReceiptAsync(registerFunction).ConfigureAwait(false);
                        if (rcpt.Failed())
                        {
                            actionStatus.ActionSucceeded = false;
                            actionStatus.ErrorMsg = $"Transaction with hash {rcpt.TransactionHash} failed";
                        }
                    }
                    else
                    {
                        actionStatus.ActionSucceeded = false;
                        actionStatus.ErrorMsg = "Not enough funds";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failure during communication with RSK network", ex);
                actionStatus.ActionSucceeded = false;
                actionStatus.ErrorMsg = ex.Message;
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return actionStatus;
        }

        public async Task<ActionStatus> StoreScheme(long accountId, Client.Common.Entities.AttributeDefinition[] attributeDefinitions)
        {
            ActionStatus actionStatus = new ActionStatus
            {
                IntegrationType = Key,
                IntegrationAction = nameof(StoreScheme),
                ActionSucceeded = true
            };

            await _semaphoreSlim.WaitAsync();

            try
            {
                var privateKey = _dataAccessService.GetAccountKeyValue(accountId, "RskSecretKey");
                if (string.IsNullOrEmpty(privateKey))
                {
                    actionStatus.ActionSucceeded = false;
                    actionStatus.ErrorMsg = $"Account {accountId} has no integration with {Key}";
                }
                else
                {
                    var account = new Account(privateKey);

                    actionStatus.IntegrationAddress = account.Address;

                    var web3 = new Web3(account, _integrationConfiguration.RpcUri);
                    var o10IdentityService = new O10IdentityService(web3, _integrationConfiguration.ContractAddress);

                    var issuers = await o10IdentityService.GetAllIssuersQueryAsync().ConfigureAwait(false);
                    if (!issuers.ReturnValue1.Any(issuers => issuers.Address.Equals(account.Address, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        actionStatus.ActionSucceeded = false;
                        actionStatus.ErrorMsg = $"Account with Id {accountId} not registered as an Identity Provider";
                    }
                    else
                    {
                        GetSchemeOutputDTO scheme = null;
                        try
                        {
                            scheme = await o10IdentityService.GetSchemeQueryAsync(account.Address).ConfigureAwait(false);
                        }
                        catch (Exception)
                        {
                            scheme = null;
                        }

                        List<AttributeDefinition> definitions = attributeDefinitions.Select(a => new AttributeDefinition { AttributeName = a.AttributeName, AttributeScheme = a.SchemeName, Alias = a.Alias, IsRoot = a.IsRoot }).ToList();
                        if (scheme?.ReturnValue2.All(a => definitions.Any(d => a.AttributeName == d.AttributeName && a.AttributeScheme == d.AttributeScheme && a.Alias == d.Alias && a.IsRoot == d.IsRoot)) ?? false)
                        {
                            actionStatus.ActionSucceeded = true;
                        }
                        else
                        {
                            var balance = await web3.Eth.GetBalance.SendRequestAsync(account.Address).ConfigureAwait(false);
                            var functionTransactionHandler = web3.Eth.GetContractTransactionHandler<SetSchemeFunction>();
                            SetSchemeFunction func = new SetSchemeFunction
                            {
                                Definitions = definitions
                            };
                            var esimatedGas = await functionTransactionHandler.EstimateGasAsync(_integrationConfiguration.ContractAddress, func).ConfigureAwait(false);
                            func.Gas = new BigInteger(esimatedGas.ToLong() * 10);
                            if (balance < func.Gas)
                            {
                                actionStatus.ActionSucceeded = false;
                                actionStatus.ErrorMsg = "Not enough funds";
                            }
                            else
                            {
                                var receipt = await o10IdentityService.SetSchemeRequestAndWaitForReceiptAsync(func).ConfigureAwait(false);
                                if (receipt.Failed())
                                {
                                    actionStatus.ActionSucceeded = false;
                                    actionStatus.ErrorMsg = $"Transaction with hash {receipt.TransactionHash} failed";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"{nameof(StoreScheme)} failed for the acccount {accountId}", ex);
                actionStatus.ActionSucceeded = false;
                actionStatus.ErrorMsg = ex.Message;
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return actionStatus;
        }

        public async Task<ActionStatus> IssueAttributes(long accountId, IssuanceDetails issuanceDetails)
        {
            ActionStatus actionStatus = new ActionStatus
            {
                IntegrationType = Key,
                IntegrationAction = nameof(IssueAttributes),
                ActionSucceeded = true
            };

            await _semaphoreSlim.WaitAsync();

            try
            {
                var privateKey = _dataAccessService.GetAccountKeyValue(accountId, "RskSecretKey");
                if (string.IsNullOrEmpty(privateKey))
                {
                    actionStatus.ActionSucceeded = false;
                    actionStatus.ErrorMsg = $"Account {accountId} has no integration with {Key}";
                }
                else
                {
                    var account = new Account(privateKey);

                    actionStatus.IntegrationAddress = account.Address;

                    var web3 = new Web3(account, _integrationConfiguration.RpcUri);
                    var o10IdentityService = new O10IdentityService(web3, _integrationConfiguration.ContractAddress);

                    var issuers = await o10IdentityService.GetAllIssuersQueryAsync().ConfigureAwait(false);
                    if (!issuers.ReturnValue1.Any(issuers => issuers.Address.Equals(account.Address, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        actionStatus.ActionSucceeded = false;
                        actionStatus.ErrorMsg = $"Account with Id {accountId} not registered as an Identity Provider";
                    }
                    else
                    {
                        List<AttributeRecord> attributeRecords = new List<AttributeRecord> 
                        {
                            new AttributeRecord
                            {
                                AttributeName = issuanceDetails.RootAttribute.AttributeName,
                                AssetCommitment = issuanceDetails.RootAttribute.AssetCommitment,
                                BindingCommitment = issuanceDetails.RootAttribute.OriginatingCommitment
                            }
                        };

                        if (issuanceDetails.AssociatedAttributes != null)
                        {
                            foreach (var attr in issuanceDetails.AssociatedAttributes)
                            {
                                attributeRecords.Add(new AttributeRecord
                                {
                                    AttributeName = attr.AttributeName,
                                    AssetCommitment = attr.AssetCommitment,
                                    BindingCommitment = attr.BindingToRootCommitment,
                                    AttributeId = new BigInteger(0),
                                    Version = new BigInteger(0)
                                });
                            }
                        }

                        var balance = await web3.Eth.GetBalance.SendRequestAsync(account.Address).ConfigureAwait(false);
                        var functionIssueAttributesHandler = web3.Eth.GetContractTransactionHandler<IssueAttributesFunction>();
                        IssueAttributesFunction func = new IssueAttributesFunction
                        {
                            AttributeRecords = attributeRecords
                        };
                        var esimatedGas = await functionIssueAttributesHandler.EstimateGasAsync(_integrationConfiguration.ContractAddress, func).ConfigureAwait(false);
                        func.Gas = new BigInteger(esimatedGas.ToLong() * 100);
                        if (balance < func.Gas)
                        {
                            actionStatus.ActionSucceeded = false;
                            actionStatus.ErrorMsg = "Not enough funds";
                        }
                        else
                        {
                            var receipt = await o10IdentityService.IssueAttributesRequestAndWaitForReceiptAsync(func.AttributeRecords).ConfigureAwait(false);
                            if (receipt.Failed())
                            {
                                actionStatus.ActionSucceeded = false;
                                actionStatus.ErrorMsg = $"Transaction with hash {receipt.TransactionHash} failed";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"{nameof(IssueAttributes)} failed for the acccount {accountId}", ex);
                actionStatus.ActionSucceeded = false;
                actionStatus.ErrorMsg = ex.Message;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        
            return actionStatus;
        }

        public string GetAddress(long accountId)
        {
            var privateKey = _dataAccessService.GetAccountKeyValue(accountId, "RskSecretKey");
            if(string.IsNullOrEmpty(privateKey))
            {
                throw new ArgumentOutOfRangeException(nameof(accountId));
            }

            var account = new Account(privateKey);
            return account.Address;
        }

        public async Task<BigInteger> GetBalance(long accountId)
        {
            var privateKey = _dataAccessService.GetAccountKeyValue(accountId, "RskSecretKey");
            if (string.IsNullOrEmpty(privateKey))
            {
                throw new ArgumentOutOfRangeException(nameof(accountId));
            }

            await _semaphoreSlim.WaitAsync();

            try
            {
                var account = new Account(privateKey);
                var web3 = new Web3(account, _integrationConfiguration.RpcUri);
                var balance = await web3.Eth.GetBalance.SendRequestAsync(account.Address).ConfigureAwait(false);
                return balance;

            }
            finally
            {
                _semaphoreSlim.Release();
            }   
        }
    }
}
