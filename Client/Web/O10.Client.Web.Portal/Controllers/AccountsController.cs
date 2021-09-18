using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using O10.Client.Web.DataContracts;
using O10.Client.Web.Portal.Helpers;
using O10.Client.Web.Portal.Services;
using O10.Core.ExtensionMethods;
using O10.Client.Web.Common.Services;
using O10.Client.Web.DataContracts.User;
using O10.Client.Common.Entities;
using O10.Client.DataLayer.Services;
using O10.Client.DataLayer.Model.Scenarios;
using System.Collections.Generic;
using O10.Core.Logging;
using Newtonsoft.Json;
using O10.Client.Web.Portal.Exceptions;
using O10.Client.Common.Exceptions;
using O10.Client.DataLayer.Model;
using O10.Core.Translators;
using Microsoft.AspNetCore.Http;
using O10.Client.Common.Integration;
using O10.Client.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using O10.Core.Serialization;
using O10.Core.Configuration;
using O10.Client.Common.Configuration;
using System.Threading.Tasks;

namespace O10.Client.Web.Portal.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountsServiceEx _accountsService;
        private readonly IExecutionContextManager _executionContextManager;
        private readonly IDataAccessService _dataAccessService;
        private readonly ITranslatorsRepository _translatorsRepository;
        private readonly IIntegrationIdPRepository _integrationIdPRepository;
        private readonly AppSettings _appSettings;
        private readonly IRestApiConfiguration _restApiConfiguration;
        private readonly ILogger _logger;

        public AccountsController(IAccountsServiceEx accountsService,
                                  IExecutionContextManager executionContextManager,
                                  IConfigurationService configurationService,
                                  IDataAccessService dataAccessService,
                                  ILoggerService loggerService,
                                  ITranslatorsRepository translatorsRepository,
                                  IIntegrationIdPRepository integrationIdPRepository,
                                  IOptions<AppSettings> appSettings)
        {
            if (loggerService is null)
            {
                throw new ArgumentNullException(nameof(loggerService));
            }

            if (appSettings is null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            _accountsService = accountsService;
            _executionContextManager = executionContextManager;
            _dataAccessService = dataAccessService;
            _translatorsRepository = translatorsRepository;
            _integrationIdPRepository = integrationIdPRepository;
            _logger = loggerService.GetLogger(nameof(AccountsController));
            _appSettings = appSettings.Value;
            _restApiConfiguration = configurationService.Get<IRestApiConfiguration>();
        }

        [HttpGet("Find")]
        public IActionResult FindAccount(string accountAlias)
        {
            var accountDescriptor = _accountsService.FindByAlias(accountAlias);
            var accountDto = _translatorsRepository.GetInstance<AccountDescriptor, AccountDto>().Translate(accountDescriptor);

            return Ok(accountDto);
        }

        [HttpPost("BySecrets")]
        public IActionResult FindAccountBySecrets([FromBody] DisclosedSecretsDto disclosedSecrets)
        {
            var accountDescriptor = _accountsService.GetBySecrets(disclosedSecrets.SecretSpendKey.HexStringToByteArray(), disclosedSecrets.SecretViewKey.HexStringToByteArray(), disclosedSecrets.Password);
            var accountDto = _translatorsRepository.GetInstance<AccountDescriptor, AccountDto>().Translate(accountDescriptor);

            return Ok(accountDto);
        }

        [HttpPost("{accountId}/Authenticate")]
        public IActionResult Authenticate(long accountId, [FromBody] AuthenticateAccountDTO accountDto)
        {
            _logger.LogIfDebug(() => $"[{accountId}]: Started authentication of the account {JsonConvert.SerializeObject(accountDto)}");

            var accountDescriptor = _accountsService.Authenticate(accountId, accountDto.Password);
            
            if (accountDescriptor == null)
            {
                throw new AccountAuthenticationFailedException(accountId);
            }

            if (accountDescriptor.AccountType == AccountTypeDTO.User)
            {
                _executionContextManager.InitializeUtxoExecutionServices(accountDescriptor.AccountId, accountDescriptor.SecretSpendKey, accountDescriptor.SecretViewKey, accountDescriptor.PwdHash);
                var persistency = _executionContextManager.ResolveExecutionServices(accountId);
                var relationsBindingService = persistency.Scope.ServiceProvider.GetService<IBoundedAssetsService>();
                relationsBindingService.Initialize(accountDto.Password, false);
            }
            else
            {
                _executionContextManager.InitializeStateExecutionServices(accountDescriptor.AccountId, accountDescriptor.SecretSpendKey);
            }

            var forLog = new
            {
                accountDescriptor.AccountId,
                accountDescriptor.AccountType,
                accountDescriptor.AccountInfo,
                SecretSpendKey = accountDescriptor.SecretSpendKey.ToHexString(),
                PublicSpendKey = accountDescriptor.PublicSpendKey.ToHexString(),
                SecretViewKey = accountDescriptor.SecretViewKey.ToHexString(),
                PublicViewKey = accountDescriptor.PublicViewKey.ToHexString()
            };

            _logger.LogIfDebug(() => $"[{accountId}]: Authenticated account {JsonConvert.SerializeObject(forLog)}");

            return Ok(_translatorsRepository.GetInstance<AccountDescriptor, AccountDto>().Translate(accountDescriptor));
        }

        [HttpPost("{accountId}/Sync")]
        public async Task<IActionResult> SyncAccount(long accountId)
        {
            _logger.LogIfDebug(() => $"[{accountId}]: Syncing the account with Id {accountId}");

            try
            {
                var persistency = _executionContextManager.ResolveExecutionServices(accountId);
                var witnessPackagesProvidersRepo = persistency.Scope.ServiceProvider.GetService<IWitnessPackagesProviderRepository>();
                IWitnessPackagesProvider packetsProvider = witnessPackagesProvidersRepo.GetInstance(_restApiConfiguration.WitnessProviderName);
                await packetsProvider.Restart();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error($"[{accountId}]: sync account failed", ex);
                throw;
            }
        }

        [HttpPost("{accountId}/Start")]
        public IActionResult Start(long accountId)
        {
            _logger.LogIfDebug(() => $"[{accountId}]: Starting the account with Id {accountId}");

            try
            {
                var account = _dataAccessService.GetAccount(accountId);
                var accountDescriptor = _translatorsRepository.GetInstance<Account, AccountDescriptor>()?.Translate(account);

                if (accountDescriptor == null)
                {
                    throw new AccountNotFoundException(accountId);
                }

                if (accountDescriptor.AccountType == AccountTypeDTO.User)
                {
                    _executionContextManager.InitializeUtxoExecutionServices(accountDescriptor.AccountId, accountDescriptor.SecretSpendKey, accountDescriptor.SecretViewKey, accountDescriptor.PwdHash);
                }
                else
                {
                    _executionContextManager.InitializeStateExecutionServices(accountDescriptor.AccountId, accountDescriptor.SecretSpendKey);
                }

                var forLog = new
                {
                    accountDescriptor.AccountId,
                    accountDescriptor.AccountType,
                    accountDescriptor.AccountInfo,
                    SecretSpendKey = accountDescriptor.SecretSpendKey.ToHexString(),
                    PublicSpendKey = accountDescriptor.PublicSpendKey.ToHexString(),
                    SecretViewKey = accountDescriptor.SecretViewKey.ToHexString(),
                    PublicViewKey = accountDescriptor.PublicViewKey.ToHexString()
                };

                _logger.LogIfDebug(() => $"[{accountId}]: Authenticated account {JsonConvert.SerializeObject(forLog)}");

                return Ok(_translatorsRepository.GetInstance<AccountDescriptor, AccountDto>().Translate(accountDescriptor));
            }
            catch (Exception ex)
            {
                _logger.Error($"[{accountId}]: failure during authentication", ex);
                throw;
            }
        }

        [HttpGet("{accountId}/BindingKey")]
        public IActionResult IsBindingKeySet(long accountId)
        {
            if(!_executionContextManager.IsStarted(accountId))
            {
                return Ok(false);
            }

            return Ok(_executionContextManager.ResolveExecutionServices(accountId)?.Scope.ServiceProvider.GetService<IBoundedAssetsService>().IsBindingKeySet());
        }

        [HttpPost("{accountId}/BindingKey")]
        public IActionResult BindingKey(long accountId, [FromBody] BindingKeyRequestDto bindingKeyRequest)
        {
            var accountDescriptor = _accountsService.Authenticate(accountId, bindingKeyRequest.Password);

            if (accountDescriptor == null && !bindingKeyRequest.Force)
            {
                throw new AccountAuthenticationFailedException(accountId);
            }

            var persistency = _executionContextManager.ResolveExecutionServices(accountId);
            var relationsBindingService = persistency.Scope.ServiceProvider.GetService<IBoundedAssetsService>();
            relationsBindingService.Initialize(bindingKeyRequest.Password);

            return Ok();
        }

        [HttpPost("Register")]
        public IActionResult Register([FromBody] AccountDto accountDto)
        {
            try
            {
                long accountId = _accountsService.Create(accountDto.AccountType, accountDto.AccountInfo, accountDto.Password);
                var accountDescriptor = _accountsService.GetById(accountId);
                return Ok(_translatorsRepository.GetInstance<AccountDescriptor, AccountDto>().Translate(accountDescriptor));
            }
            catch (Exception ex)
            {
                return BadRequest(new { ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetAll(long scenarioId = 0, bool withPrivate = false, int ofTypeOnly = 0)
        {
            ScenarioSession scenarioSession = scenarioId > 0 ? _dataAccessService.GetScenarioSessions(User.Identity.Name).FirstOrDefault(s => s.ScenarioId == scenarioId) : null;
            if (scenarioSession != null)
            {
                IEnumerable<ScenarioAccount> scenarioAccounts = _dataAccessService.GetScenarioAccounts(scenarioSession.ScenarioSessionId);

                var accounts = _accountsService.GetAll()
                    .Where(a => scenarioAccounts.Any(sa => sa.AccountId == a.AccountId))
                    .Select(a => _translatorsRepository.GetInstance<AccountDescriptor, AccountDto>().Translate(a));

                return Ok(accounts);
            }
            else
            {
                var accounts = _accountsService.GetAll()
                    .Where(a => (withPrivate || !a.IsPrivate) && (ofTypeOnly == 0 || (int)a.AccountType == ofTypeOnly))
                    .Select(a => _translatorsRepository.GetInstance<AccountDescriptor, AccountDto>().Translate(a));

                return Ok(accounts);
            }
        }

        [HttpGet("{accountId}")]
        public IActionResult GetById(long accountId)
        {
            var accountDescriptor = _accountsService.GetById(accountId);
            var accountDto = _translatorsRepository.GetInstance<AccountDescriptor, AccountDto>().Translate(accountDescriptor);
            
            if(accountDto == null)
            {
                return NotFound();
            }
            
            return Ok(accountDto);
        }

        [HttpPost("{sourceAccountId}/Duplicate/{targetAccountId}")]
        public IActionResult DuplicateUserAccount(long sourceAccountId, long targetAccountId)
        {
            var account = _accountsService.DuplicateAccount(sourceAccountId, targetAccountId);

            return account.Match<IActionResult>(
                a => Ok(_translatorsRepository.GetInstance<AccountDescriptor, AccountDto>().Translate(a)), 
                () => BadRequest());
        }

        [HttpPost("RemoveAccount")]
        public IActionResult RemoveAccount([FromBody] AccountDto account)
        {
            if (account != null)
            {
                _accountsService.Delete(account.AccountId);
                _executionContextManager.UnregisterExecutionServices(account.AccountId);
                return Ok();
            }

            return BadRequest();
        }

        [HttpPut("{accountId}/Stop")]
        public IActionResult StopAccount(long accountId)
        {
            _executionContextManager.UnregisterExecutionServices(accountId);
            return Ok();
        }

        [HttpPut("{accountId}")]
        public IActionResult OverrideUserAccount(long accountId, [FromBody] DisclosedSecretsDto accountOverride)
        {
            _logger.LogIfDebug(() => $"[{accountId}]: {nameof(OverrideUserAccount)} with {JsonConvert.SerializeObject(accountOverride, new ByteArrayJsonConverter())}");
            _accountsService.Override(AccountTypeDTO.User, accountId, accountOverride.SecretSpendKey.HexStringToByteArray(), accountOverride.SecretViewKey.HexStringToByteArray(), accountOverride.Password, accountOverride.LastCombinedBlockHeight);

            _executionContextManager.UnregisterExecutionServices(accountId);
            return Ok(_translatorsRepository.GetInstance<AccountDescriptor, AccountDto>().Translate(_accountsService.GetById(accountId)));
        }

        [HttpPost("{accountId}/Reset")]
        public IActionResult ResetCompromisedAccount(long accountId, [FromBody] AuthenticateAccountDTO authenticateAccount)
        {
            if (authenticateAccount is null)
            {
                throw new ArgumentNullException(nameof(authenticateAccount));
            }

            AccountDescriptor accountDescriptor = _accountsService.Authenticate(accountId, authenticateAccount.Password);

            if (accountDescriptor != null)
            {
                _accountsService.ResetAccount(accountId, authenticateAccount.Password);
                _executionContextManager.UnregisterExecutionServices(accountId);

                accountDescriptor = _accountsService.Authenticate(accountId, authenticateAccount.Password);

                if (accountDescriptor.AccountType == AccountTypeDTO.User)
                {
                    _executionContextManager.InitializeUtxoExecutionServices(accountId, accountDescriptor.SecretSpendKey, accountDescriptor.SecretViewKey, accountDescriptor.PwdHash);
                }
                else
                {
                    _executionContextManager.InitializeStateExecutionServices(accountId, accountDescriptor.SecretSpendKey);
                }

                return Ok(_translatorsRepository.GetInstance<AccountDescriptor, AccountDto>().Translate(accountDescriptor));
            }
            else
            {
                throw new AccountAuthenticationFailedException(accountId);
            }
        }

        [HttpPost("KeyValues")]
        public IActionResult SetAccountKeyValues(long accountId, [FromBody] Dictionary<string, string> keyValues)
        {
            _dataAccessService.SetAccountKeyValues(accountId, keyValues);

            
            return Ok(_dataAccessService.GetAccountKeyValues(accountId));
        }

        [HttpDelete("KeyValues")]
        public IActionResult DeleteAccountKeyValues(long accountId, [FromBody] List<string> keys)
        {
            _dataAccessService.RemoveAccountKeyValues(accountId, keys);

            return Ok(_dataAccessService.GetAccountKeyValues(accountId));
        }

        [HttpGet("KeyValues")]
        public IActionResult GetAccountKeyValues(long accountId)
        {
            return Ok(_dataAccessService.GetAccountKeyValues(accountId));
        }

        [HttpPut("Integration")]
        public IActionResult SetIntegration(long accountId, string integrationKey)
        {
            _dataAccessService.SetAccountKeyValues(accountId, new Dictionary<string, string> { { _integrationIdPRepository.IntegrationKeyName, integrationKey } });
            return Ok();
        }

        private ObjectResult InternalServerError(Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message });
        }
    }
}
