using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using O10.Core.ExtensionMethods;
using O10.Client.Common.Entities;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Enums;
using O10.Client.DataLayer.Services;
using O10.Core.Architecture;

using O10.Core.Configuration;
using O10.Client.Web.Portal.Scenarios.Configuration;
using O10.Client.Web.Portal.Scenarios.Models;
using O10.Client.DataLayer.Model;
using O10.Client.DataLayer.AttributesScheme;
using O10.Client.DataLayer.Model.Scenarios;
using O10.Client.Web.Portal.Services;
using System.Collections.Concurrent;
using O10.Client.Web.Portal.Scenarios.Exceptions;
using O10.Core.Logging;

namespace O10.Client.Web.Portal.Scenarios.Services
{
    [RegisterDefaultImplementation(typeof(IScenarioRunner), Lifetime = LifetimeManagement.Singleton)]
    public class ScenarioRunner : IScenarioRunner
    {
        private readonly IScenariosConfiguration _scenariosConfiguration;
        private readonly Dictionary<int, ScenarioDefinition> _scenarios = new Dictionary<int, ScenarioDefinition>();
        private readonly IAccountsService _accountsService;
        private readonly IAssetsService _assetsService;
        private readonly IDataAccessService _dataAccessService;
        private readonly IExecutionContextManager _executionContextManager;
        private readonly ConcurrentDictionary<string, ScenarioMonitoringData> _activeScenarios;
        private readonly ILogger _logger;

        public ScenarioRunner(IConfigurationService configurationService, IAccountsService accountsService,
            IAssetsService assetsService, IDataAccessService dataAccessService,
            IExecutionContextManager executionContextManager, ILoggerService loggerService)
        {
            _scenariosConfiguration = configurationService?.Get<IScenariosConfiguration>();
            _accountsService = accountsService;
            _assetsService = assetsService;
            _dataAccessService = dataAccessService;
            _executionContextManager = executionContextManager;
            _activeScenarios = new ConcurrentDictionary<string, ScenarioMonitoringData>();
            _logger = loggerService.GetLogger(nameof(ScenarioRunner));
        }

        public ScenarioSession AbandonScenario(string userSubject, int id)
        {
            _logger.Info($"{nameof(AbandonScenario)}({userSubject}, {id})");
            try
            {
                ScenarioDefinition scenarioDefinition = _scenarios[id];
                ScenarioSession scenarioSession = _dataAccessService.GetScenarioSessions(userSubject).FirstOrDefault(s => s.ScenarioId == id);

                if (scenarioSession == null)
                {
                    return null;
                }

                IEnumerable<Client.DataLayer.Model.Scenarios.ScenarioAccount> scenarioAccounts = _dataAccessService.GetScenarioAccounts(scenarioSession.ScenarioSessionId);

                foreach (var scenarioAccount in scenarioAccounts)
                {
                    _executionContextManager.UnregisterExecutionServices(scenarioAccount.AccountId);
                }

                _activeScenarios.TryRemove(userSubject, out ScenarioMonitoringData scenarioMonitoringData);

                return scenarioSession;

            }
            catch (Exception ex)
            {
                _logger.Error($"Failed {nameof(AbandonScenario)}({userSubject}, {id})", ex);
                throw;
            }
        }

        public string GetScenarioCurrentStepContent(string userSubject, int id)
        {
            _logger.Debug($"{nameof(GetScenarioCurrentStepContent)}({userSubject}, {id})");
            try
            {
                ScenarioDefinition scenarioDefinition = _scenarios[id];
                ScenarioSession scenarioSession = _dataAccessService.GetScenarioSessions(userSubject).FirstOrDefault(s => s.ScenarioId == id);

                string path = Path.Combine(_scenariosConfiguration.ContentBasePath, id.ToString(), $"{scenarioSession.CurrentStep}.md");
                if (File.Exists(path))
                {
                    string content = File.ReadAllText(path);
                    return content;
                }

                return null;

            }
            catch (Exception ex)
            {
                _logger.Error($"Failed {nameof(GetScenarioCurrentStepContent)}({userSubject}, {id})", ex);

                throw;
            }
        }

        public IEnumerable<ScenarioDefinition> GetScenarioDefinitions()
        {
            return _scenarios.Values;
        }

        public void Initialize()
        {
            _logger.Info($"{nameof(Initialize)}()");
            try
            {
                foreach (var path in Directory.GetFiles(_scenariosConfiguration.FolderPath, "Scenario-*.json"))
                {
                    string content = File.ReadAllText(path);
                    ScenarioDefinition scenarioDefinition = JsonConvert.DeserializeObject<ScenarioDefinition>(content);
                    _scenarios.Add(scenarioDefinition.Id, scenarioDefinition);
                }

            }
            catch (Exception ex)
            {
                _logger.Error($"Failed {nameof(Initialize)}()", ex);
                throw;
            }
        }

        public void ProgressScenario(string userSubject, int id, bool forward = true)
        {
            _logger.Debug($"{nameof(ProgressScenario)}({userSubject}, {id}, {forward})");
            try
            {
                ScenarioSession scenarioSession = _dataAccessService.GetScenarioSessions(userSubject).FirstOrDefault(s => s.ScenarioId == id);

                if (scenarioSession == null)
                {
                    throw new ScenarioSessionNotFoundException(userSubject, id);
                }

                if (forward)
                {
                    _dataAccessService.UpdateScenarioSessionStep(scenarioSession.ScenarioSessionId, scenarioSession.CurrentStep + 1);
                }
                else
                {
                    _dataAccessService.UpdateScenarioSessionStep(scenarioSession.ScenarioSessionId, scenarioSession.CurrentStep - 1);
                }

                ScenarioMonitoringData scenarioMonitoringData = _activeScenarios.GetOrAdd(userSubject, u => new ScenarioMonitoringData
                {
                    ScenarioId = id,
                    ScenarioSessionId = scenarioSession.ScenarioSessionId,
                    ActivationTime = scenarioSession.StartTime,
                    LastUseTime = DateTime.UtcNow
                });

                scenarioMonitoringData.LastUseTime = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed {nameof(ProgressScenario)}({userSubject}, {id}, {forward})", ex);
                throw;
            }
        }

        public ScenarioSession ResumeScenario(string userSubject, int id)
        {
            _logger.Info($"{nameof(ResumeScenario)}({userSubject}, {id})");
            try
            {
                ScenarioSession scenarioSession = _dataAccessService.GetScenarioSessions(userSubject).FirstOrDefault(s => s.ScenarioId == id);

                if (scenarioSession == null)
                {
                    throw new ScenarioSessionNotFoundException(userSubject, id);
                }

                bool needInitialize = false;
                if (!_activeScenarios.TryGetValue(userSubject, out ScenarioMonitoringData scenarioMonitoringData))
                {
                    needInitialize = true;
                }
                else if (scenarioMonitoringData.ScenarioId != id)
                {
                    needInitialize = true;
                }

                if (needInitialize)
                {
                    scenarioMonitoringData = new ScenarioMonitoringData
                    {
                        ScenarioId = id,
                        ScenarioSessionId = scenarioSession.ScenarioSessionId,
                        ActivationTime = scenarioSession.StartTime,
                        LastUseTime = DateTime.UtcNow
                    };
                    _activeScenarios.AddOrUpdate(userSubject, scenarioMonitoringData, (k, v) => v);


                    ScenarioDefinition scenarioDefinition = _scenarios[id];
                    IEnumerable<Client.DataLayer.Model.Scenarios.ScenarioAccount> scenarioAccounts = _dataAccessService.GetScenarioAccounts(scenarioSession.ScenarioSessionId);

                    foreach (var scenarioAccount in scenarioAccounts)
                    {
                        AccountDescriptor account = _accountsService.GetById(scenarioAccount.AccountId);
                        if (account.AccountType == AccountType.IdentityProvider || account.AccountType == AccountType.ServiceProvider)
                        {
                            AccountDescriptor accountDescriptor = _accountsService.Authenticate(scenarioAccount.AccountId, "qqq");
                            _executionContextManager.InitializeStateExecutionServices(accountDescriptor.AccountId, accountDescriptor.SecretSpendKey);
                        }
                    }
                }
                else
                {
                    scenarioMonitoringData.LastUseTime = DateTime.UtcNow;
                }

                return scenarioSession;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed {nameof(ResumeScenario)}({userSubject}, {id})", ex);
                throw;
            }
        }

        public ScenarioSession SetupScenario(string userSubject, int id)
        {
            _logger.Info($"{nameof(SetupScenario)}({userSubject}, {id})");
            try
            {
                if (!_scenarios.ContainsKey(id))
                {
                    throw new ArgumentOutOfRangeException(nameof(id));
                }

                if (_activeScenarios.TryRemove(userSubject, out ScenarioMonitoringData scenarioMonitoringData))
                {
                    AbandonScenario(userSubject, scenarioMonitoringData.ScenarioId);
                }

                ScenarioDefinition scenarioDefinition = _scenarios[id];

                long scenarioSessionId = _dataAccessService.AddNewScenarionSession(userSubject, id);
                _dataAccessService.UpdateScenarioSessionStep(scenarioSessionId, 1);
                ScenarioSession scenarioSession = _dataAccessService.GetScenarioSession(scenarioSessionId);

                scenarioMonitoringData = new ScenarioMonitoringData
                {
                    ScenarioId = id,
                    ScenarioSessionId = scenarioSessionId,
                    ActivationTime = scenarioSession.StartTime,
                    LastUseTime = DateTime.UtcNow
                };

                _activeScenarios.AddOrUpdate(userSubject, scenarioMonitoringData, (k, v) => scenarioMonitoringData);

                SetupIdentityProviders(scenarioDefinition, scenarioSessionId);

                SetupServiceProviders(scenarioDefinition, scenarioSessionId);

                SetupUsers(scenarioDefinition, scenarioSessionId);

                return scenarioSession;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed {nameof(ResumeScenario)}({userSubject}, {id})", ex);
                throw;
            }
        }

        private void SetupUsers(ScenarioDefinition scenarioDefinition, long scenarioSessionId)
        {
            foreach (var scenarioAccount in scenarioDefinition.Setup.Accounts.Where(a => a.AccountType == AccountType.User))
            {
                long accountId = _accountsService.Create(AccountType.User, scenarioAccount.AccountInfo, "qqq", true);
                _dataAccessService.AddScenarionSessionAccount(scenarioSessionId, accountId);
            }
        }

        private void SetupServiceProviders(ScenarioDefinition scenarioDefinition, long scenarioSessionId)
        {
            foreach (var scenarioAccount in scenarioDefinition.Setup.Accounts.Where(a => a.AccountType == AccountType.ServiceProvider))
            {
                long accountId = _accountsService.Create(AccountType.ServiceProvider, scenarioAccount.AccountInfo, "qqq", true);
                _dataAccessService.AddScenarionSessionAccount(scenarioSessionId, accountId);
                AccountDescriptor accountDescriptor = _accountsService.Authenticate(accountId, "qqq");
                _executionContextManager.InitializeStateExecutionServices(accountId, accountDescriptor.SecretSpendKey);

                if (scenarioAccount.RelationGroups != null)
                {
                    foreach (var scenarioRelationGroup in scenarioAccount.RelationGroups)
                    {
                        long groupId = _dataAccessService.AddSpEmployeeGroup(accountId, scenarioRelationGroup.GroupName);

                        foreach (var scenarioRelation in scenarioRelationGroup.Relations)
                        {
                            _dataAccessService.AddSpEmployee(accountId, "", scenarioRelation.RootAttribute, groupId);
                        }
                    }
                }
            }
        }

        private void SetupIdentityProviders(ScenarioDefinition scenarioDefinition, long scenarioSessionId)
        {
            foreach (var scenarioAccount in scenarioDefinition.Setup.Accounts.Where(a => a.AccountType == AccountType.IdentityProvider))
            {
                long accountId = _accountsService.Create(AccountType.IdentityProvider, scenarioAccount.AccountInfo, "qqq", true);
                _dataAccessService.AddScenarionSessionAccount(scenarioSessionId, accountId);
                AccountDescriptor accountDescriptor = _accountsService.Authenticate(accountId, "qqq");
                _executionContextManager.InitializeStateExecutionServices(accountId, accountDescriptor.SecretSpendKey);

                foreach (var attributeScheme in scenarioAccount.IdentityScheme)
                {
                    long schemeId = _dataAccessService.AddAttributeToScheme(accountDescriptor.PublicSpendKey.ToHexString(), attributeScheme.AttributeName, attributeScheme.AttributeSchemeName, attributeScheme.Alias, null);

                    if (attributeScheme.CanBeRoot)
                    {
                        _dataAccessService.ToggleOnRootAttributeScheme(schemeId);
                    }
                }

                IdentitiesScheme rootScheme = _dataAccessService.GetRootIdentityScheme(accountDescriptor.PublicSpendKey.ToHexString());
                foreach (var identity in scenarioAccount.Identities)
                {
                    IEnumerable<(string attributeName, string content)> attrs = GetAttribitesAndContent(identity, accountDescriptor);
                    Identity identityDb = _dataAccessService.CreateIdentity(accountDescriptor.AccountId, identity.Alias, attrs.ToArray());
                }
            }
        }

        private IEnumerable<(string attributeName, string content)> GetAttribitesAndContent(ScenarionIdentity identity, AccountDescriptor account)
        {
            IEnumerable<(string attributeName, string content)> attrs;

            IdentitiesScheme rootScheme = _dataAccessService.GetRootIdentityScheme(account.PublicSpendKey.ToHexString());
            if (rootScheme != null)
            {
                string rootAttributeContent = identity.Attributes[rootScheme.AttributeName];
                byte[] rootAssetId = _assetsService.GenerateAssetId(rootScheme.IdentitiesSchemeId, rootAttributeContent);

                if (identity.Attributes.ContainsKey(AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD))
                {
                    identity.Attributes[AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD] = rootAssetId.ToHexString();
                }

                attrs = identity.Attributes.Select(a => (a.Key, a.Value));

                if (!identity.Attributes.ContainsKey(AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD))
                {
                    attrs = attrs.Append((AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD, rootAssetId.ToHexString()));
                }
            }
            else
            {
                attrs = identity.Attributes.Select(a => (a.Key, a.Value));
            }

            return attrs;
        }

        public ScenarioSession GetActiveScenarioSession(string userSubject)
        {
            _logger.Info($"{nameof(GetActiveScenarioSession)}({userSubject})");
            try
            {
                if (_activeScenarios.TryGetValue(userSubject, out ScenarioMonitoringData scenarioMonitoringData))
                {
                    return _dataAccessService.GetScenarioSession(scenarioMonitoringData.ScenarioSessionId);
                }

                ScenarioSession scenarioSession = _dataAccessService.GetScenarioSessions(userSubject).FirstOrDefault();

                if (scenarioSession != null)
                {
                    ResumeScenario(userSubject, scenarioSession.ScenarioId);
                }

                return scenarioSession;
            }
            catch (Exception ex)
            {
                _logger.Error($"{nameof(GetActiveScenarioSession)}({userSubject})", ex);
                throw;
            }
        }

        private string Evaluate(string expression)
        {
            if (!expression.StartsWith("$"))
            {
                return expression;
            }

            ExtractFunctionNameAndArgs(expression, out string functionName, out string[] args);

            switch (functionName.ToLower())
            {
                case "publicspendkey":
                    return FuncPublicSpendKey(args);
                case "publicviewkey":
                    return FuncPublicViewKey(args);
                default:
                    return null;
            }
        }

        private string FuncPublicViewKey(string[] args)
        {
            string accountNameStr = args[0];
            AccountDescriptor account = _accountsService.GetAll().FirstOrDefault(a => a.AccountInfo == accountNameStr);
            return account?.PublicSpendKey.ToHexString();
        }

        private string FuncPublicSpendKey(string[] args)
        {
            string accountNameStr = args[0];
            AccountDescriptor account = _accountsService.GetAll().FirstOrDefault(a => a.AccountInfo == accountNameStr);
            return account?.PublicSpendKey.ToHexString();
        }

        private static void ExtractFunctionNameAndArgs(string expression, out string functionName, out string[] args)
        {
            int functionNameDelimiterPosition = expression.IndexOf("(");
            functionName = expression.Substring(0, functionNameDelimiterPosition).Trim('$');
            string argsString = expression.Substring(functionNameDelimiterPosition + 1, expression.Length - functionNameDelimiterPosition - 1).Trim(')');

            args = argsString.Split(',');
        }
    }
}
