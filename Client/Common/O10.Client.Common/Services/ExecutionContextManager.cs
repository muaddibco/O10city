using O10.Client.Common.Exceptions;
using O10.Client.Common.Interfaces;
using O10.Client.Common.Services.ExecutionScope;
using O10.Client.DataLayer.Enums;
using O10.Core.Architecture;
using O10.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace O10.Client.Common.Services
{
    [RegisterDefaultImplementation(typeof(IExecutionContextManager), Lifetime = LifetimeManagement.Singleton)]
    public class ExecutionContextManager : IExecutionContextManager
    {
        private readonly Dictionary<long, ScopePersistency> _persistencyItems = new();
        private readonly Dictionary<long, ICollection<IDisposable>> _accountIdCancellationList;
        private readonly IServiceProvider _serviceProvider;
        private readonly IScopePersistencyProvider _scopePersistencyProvider;
        private readonly ILogger _logger;

        public ExecutionContextManager(IServiceProvider serviceProvider, IScopePersistencyProvider scopePersistencyProvider, ILoggerService loggerService)
        {
            _accountIdCancellationList = new Dictionary<long, ICollection<IDisposable>>();
            _serviceProvider = serviceProvider;
            _scopePersistencyProvider = scopePersistencyProvider;
            _logger = loggerService.GetLogger(nameof(ExecutionContextManager));
        }

        public ScopePersistency InitializeExecutionServices(AccountType accountType, ScopeInitializationParams initializationParams, IUpdater? updater = null)
        {
            lock (_persistencyItems)
            {
                var accountId = initializationParams.AccountId;

                if (_persistencyItems.ContainsKey(accountId))
                {
                    _logger.Info($"[{accountId}]: Account with id {accountId} already registered at StatePersistency");
                    return _persistencyItems[accountId];
                }

                _logger.Info($"[{accountId}]: >============================================================================");
                _logger.Info($"[{accountId}]: {nameof(InitializeExecutionServices)} for account with id {accountId}");

                var state = _scopePersistencyProvider.GetScopePersistency(accountType, initializationParams, updater);

                _persistencyItems.Add(accountId, state);

                return state;
            }
        }

        private void Clean(long accountId)
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

        public ScopePersistency ResolveExecutionServices(long accountId)
        {
            if (!_persistencyItems.ContainsKey(accountId))
            {
                throw new ExecutionContextNotStartedException(accountId);
            }

            return _persistencyItems[accountId];
        }

        public bool IsStarted(long accountId)
        {
            return _persistencyItems.ContainsKey(accountId);
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
