using O10.Client.Common.Interfaces;
using O10.Client.Common.Services;
using O10.Client.Common.Services.ExecutionScope;
using System;
using Microsoft.Extensions.DependencyInjection;
using O10.Core.Logging;
using O10.Core.Architecture;
using O10.Client.DataLayer.Enums;

namespace O10.Client.IdentityProvider
{
    [RegisterDefaultImplementation(typeof(IScopePersistencyProvider), Lifetime = LifetimeManagement.Singleton)]
    public class ScopePersistencyProvider : IScopePersistencyProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public ScopePersistencyProvider(IServiceProvider serviceProvider, ILoggerService loggerService)
        {
            _serviceProvider = serviceProvider;
            _logger = loggerService.GetLogger(nameof(ScopePersistencyProvider));
        }

        public ScopePersistency GetScopePersistency(AccountType accountType, ScopeInitializationParams initializationParams, Func<IServiceProvider, IUpdater?>? getUpdater = null)
        {
            try
            {
                var state = new ScopePersistency(initializationParams.AccountId, _serviceProvider);
                
                if (getUpdater != null)
                {
                    var updater = getUpdater.Invoke(state.Scope.ServiceProvider);
                    if (updater != null)
                    {
                        state.Scope.ServiceProvider.GetService<IUpdaterRegistry>()?.RegisterInstance(updater);
                    }
                }

                var scopeService = state.Scope.ServiceProvider.GetService<IExecutionScopeServiceRepository>()?.GetInstance(accountType);
                scopeService.Initiliaze(initializationParams);

                return state;
            }
            catch (Exception ex)
            {
                _logger.Error($"[{initializationParams.AccountId}]: Failure during {nameof(GetScopePersistency)} for account with id {initializationParams.AccountId}", ex);
                throw;
            }
            finally
            {
                _logger.Info($"[{initializationParams.AccountId}]: <============================================================================");
            }
        }
    }
}
