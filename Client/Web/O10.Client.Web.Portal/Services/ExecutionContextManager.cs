using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using O10.Client.Common.Interfaces;
using O10.Core.Architecture;
using O10.Core.Logging;
using O10.Client.Web.Common.Services;
using O10.Client.Web.Portal.Exceptions;
using O10.Client.Common.Services;

namespace O10.Client.Web.Portal.Services
{
    [RegisterDefaultImplementation(typeof(IExecutionContextManager), Lifetime = LifetimeManagement.Singleton)]
    public class ExecutionContextManager : IExecutionContextManager
    {
        private readonly Dictionary<long, Persistency> _persistencyItems = new Dictionary<long, Persistency>();
        private readonly Dictionary<long, ICollection<IDisposable>> _accountIdCancellationList;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;


        public ExecutionContextManager(IServiceProvider serviceProvider, ILoggerService loggerService)
        {
            _accountIdCancellationList = new Dictionary<long, ICollection<IDisposable>>();
            _serviceProvider = serviceProvider;
            _logger = loggerService.GetLogger(nameof(ExecutionContextManager));
        }

        public Persistency InitializeStateExecutionServices(long accountId, byte[] secretKey, IUpdater updater = null)
        {
            lock (_persistencyItems)
            {
                if (_persistencyItems.ContainsKey(accountId))
                {
                    _logger.Info($"[{accountId}]: Account with id {accountId} already registered at StatePersistency");
                    return _persistencyItems[accountId];
                }

                _logger.Info($"[{accountId}]: {nameof(InitializeStateExecutionServices)} for account with id {accountId}");

                try
                {
                    var state = new Persistency(accountId, _serviceProvider);
                    state.Scope.ServiceProvider.GetService<IUpdaterRegistry>().RegisterInstance(updater ?? CreateStateUpdater(state.Scope.ServiceProvider, accountId, state.CancellationTokenSource.Token));
                    var scopeService = state.Scope.ServiceProvider.GetService<IExecutionScopeServiceRepository>().GetInstance("State");
                    var scopeParams = scopeService.GetScopeInitializationParams<StateScopeInitializationParams>();
                    scopeParams.AccountId = accountId;
                    scopeParams.SecretKey = secretKey;
                    scopeService.Initiliaze(scopeParams);

                    _persistencyItems.Add(accountId, state);
                    return state;
                }
                catch (Exception ex)
                {
                    _logger.Error($"[{accountId}]: Failure during {nameof(InitializeStateExecutionServices)} for account with id {accountId}", ex);
                    throw;
                }
            }
        }

        private static IUpdater CreateStateUpdater(IServiceProvider serviceProvider, long accountId, CancellationToken cancellationToken)
        {
            var updater = ActivatorUtilities.CreateInstance<ServiceProviderUpdater>(serviceProvider);
            updater.Initialize(accountId, cancellationToken);
            return updater;
        }

        public Persistency InitializeUtxoExecutionServices(long accountId, byte[] secretSpendKey, byte[] secretViewKey, byte[] pwdSecretKey, IUpdater updater = null)
        {
            lock (_persistencyItems)
            {
                if (_persistencyItems.ContainsKey(accountId))
                {
                    _logger.Info($"[{accountId}]: account already registered at UtxoPersistency");
                    return _persistencyItems[accountId];
                }

                _logger.Info($"[{accountId}]: {nameof(InitializeUtxoExecutionServices)}");

                try
                {
                    var state = new Persistency(accountId, _serviceProvider);
                    state.Scope.ServiceProvider.GetService<IUpdaterRegistry>().RegisterInstance(updater ?? CreateStealthUpdater(state.Scope.ServiceProvider, accountId, state.CancellationTokenSource.Token));
                    var scopeService = state.Scope.ServiceProvider.GetService<IExecutionScopeServiceRepository>().GetInstance("Stealth");
                    var scopeParams = scopeService.GetScopeInitializationParams<UtxoScopeInitializationParams>();
                    scopeParams.AccountId = accountId;
                    scopeParams.SecretSpendKey = secretSpendKey;
                    scopeParams.SecretViewKey = secretViewKey;
                    scopeParams.PwdSecretKey = pwdSecretKey;
                    scopeService.Initiliaze(scopeParams);

                    _persistencyItems.Add(accountId, state);

                    return state;
                }
                catch (Exception ex)
                {
                    _logger.Error($"[{accountId}]: Failure during {nameof(InitializeUtxoExecutionServices)}", ex);

                    throw;
                }
            }
        }

        private static IUpdater CreateStealthUpdater(IServiceProvider serviceProvider, long accountId, CancellationToken cancellationToken)
        {
            var updater = ActivatorUtilities.CreateInstance<UserIdentitiesUpdater>(serviceProvider);
            updater.Initialize(accountId, cancellationToken);
            return updater;
        }

        public void Clean(long accountId)
        {
            if (_accountIdCancellationList.ContainsKey(accountId))
            {
                _accountIdCancellationList[accountId].ToList().ForEach(t => t.Dispose());

                if (_persistencyItems.ContainsKey(accountId))
                {
                    _persistencyItems.Remove(accountId);
                }
            }
        }

        public Persistency ResolveExecutionServices(long accountId)
        {
            if (!_persistencyItems.ContainsKey(accountId))
            {
                throw new ExecutionContextNotStartedException(accountId);
            }

            return _persistencyItems[accountId];
        }

        public void UnregisterExecutionServices(long accountId)
        {
            _logger.Info($"[{accountId}]: Stopping services for account");

            if (_persistencyItems.ContainsKey(accountId))
            {
                var persistency = _persistencyItems[accountId];
                persistency.CancellationTokenSource.Cancel();
                persistency.Scope.Dispose();

                _persistencyItems.Remove(accountId);
            }

            Clean(accountId);
        }
    }
}
