using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using O10.Client.Common.Interfaces;
using O10.Core.Architecture;
using O10.Client.Stealth;
using O10.Client.State;
using O10.Client.Common.Services.ExecutionScope;
using O10.Client.DataLayer.Enums;

namespace O10.Client.Web.Portal.Services
{
    [RegisterDefaultImplementation(typeof(IWebExecutionContextManager), Lifetime = LifetimeManagement.Singleton)]
    public class ExecutionContextManager : IWebExecutionContextManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IExecutionContextManager _executionContextManager;


        public ExecutionContextManager(IServiceProvider serviceProvider, IExecutionContextManager executionContextManager)
        {
            _serviceProvider = serviceProvider;
            _executionContextManager = executionContextManager;
        }

        private ScopePersistency InitializeStateExecutionServices(AccountType accountType, long accountId, byte[] secretKey, IUpdater updater)
        {
            return _executionContextManager.InitializeExecutionServices(accountType, new StateScopeInitializationParams { AccountId = accountId, SecretKey = secretKey }, updater);
        }

        private static IUpdater CreateStateUpdater(IServiceProvider serviceProvider, long accountId, CancellationToken cancellationToken)
        {
            var updater = ActivatorUtilities.CreateInstance<ServiceProviderUpdater>(serviceProvider);
            updater.Initialize(accountId, cancellationToken);
            return updater;
        }

        public ScopePersistency InitializeServiceProviderExecutionServices(long accountId, byte[] secretKey, IUpdater? updater = null)
        {
            return InitializeStateExecutionServices(AccountType.ServiceProvider, accountId, secretKey, updater ?? CreateStateUpdater(_serviceProvider, accountId, CancellationToken.None));
        }

        public ScopePersistency InitializeIdentityProviderExecutionServices(long accountId, byte[] secretKey, IUpdater? updater = null)
        {
            return InitializeStateExecutionServices(AccountType.IdentityProvider, accountId, secretKey, updater);
        }


        public ScopePersistency InitializeUserExecutionServices(long accountId, byte[] secretSpendKey, byte[] secretViewKey, byte[] pwdSecretKey, IUpdater? updater = null)
        {
            return _executionContextManager.InitializeExecutionServices(
                AccountType.User,
                new StealthScopeInitializationParams
                {
                    AccountId = accountId,
                    SecretSpendKey = secretSpendKey,
                    SecretViewKey = secretViewKey,
                    PwdSecretKey = pwdSecretKey
                },
                updater?? CreateStealthUpdater(_serviceProvider, accountId, CancellationToken.None));
        }

        private static IUpdater CreateStealthUpdater(IServiceProvider serviceProvider, long accountId, CancellationToken cancellationToken)
        {
            var updater = ActivatorUtilities.CreateInstance<UserIdentitiesUpdater>(serviceProvider);
            updater.Initialize(accountId, cancellationToken);
            return updater;
        }

        public ScopePersistency ResolveExecutionServices(long accountId)
        {
            return _executionContextManager.ResolveExecutionServices(accountId);
        }

        public bool IsStarted(long accountId)
        {
            return _executionContextManager.IsStarted(accountId);
        }

        public void UnregisterExecutionServices(long accountId)
        {
            _executionContextManager.UnregisterExecutionServices(accountId);
        }
    }
}
