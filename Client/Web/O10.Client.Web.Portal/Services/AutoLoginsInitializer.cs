﻿using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using O10.Client.DataLayer.Services;
using O10.Core;
using O10.Core.Architecture;
using O10.Core.Logging;
using O10.Core.Serialization;

namespace O10.Client.Web.Portal.Services
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Scoped)]
    public class AutoLoginsInitializer : InitializerBase
    {
        private readonly IDataAccessService _dataAccessService;
        private readonly IExecutionContextManager _executionContextManager;
        private readonly ILogger _logger;

        public AutoLoginsInitializer(IDataAccessService dataAccessService, IExecutionContextManager executionContextManager, ILoggerService loggerService)
        {
            _dataAccessService = dataAccessService;
            _executionContextManager = executionContextManager;
            _logger = loggerService.GetLogger(nameof(AutoLoginsInitializer));
        }

        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Lowest;

        protected override async Task InitializeInner(CancellationToken cancellationToken)
        {
            foreach (var autoLogin in _dataAccessService.GetAutoLogins().Where(a => a.Account != null))
            {
                _logger.LogIfDebug(() => $"[{autoLogin.Account.AccountId}]: Autologin of {JsonConvert.SerializeObject(autoLogin, new ByteArrayJsonConverter())}");
                int attempts = 5;
                bool succeeded = false;

                do
                {
                    try
                    {
                        _executionContextManager.InitializeStateExecutionServices(autoLogin.Account.AccountId, autoLogin.SecretKey);

                        _logger.Info($"[{autoLogin.Account.AccountId}]: Account {autoLogin.Account.AccountInfo} with id {autoLogin.Account.AccountId} successfully auto logged in");
                        succeeded = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"[{autoLogin.Account.AccountId}]: Failure during {nameof(AutoLoginsInitializer)} for {JsonConvert.SerializeObject(autoLogin, new ByteArrayJsonConverter())}", ex);
                        await Task.Delay(1000);
                    }
                } while (!succeeded && --attempts > 0);
            }
        }
    }
}
