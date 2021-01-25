using log4net;
using System;
using System.Collections.Generic;
using System.Reflection;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.Exceptions;
using System.Linq;

namespace O10.Core.Identity
{

    [RegisterDefaultImplementation(typeof(IIdentityKeyProvidersRegistry), Lifetime = LifetimeManagement.Singleton)]
    public class IdentityKeyProvidersRegistry : IIdentityKeyProvidersRegistry
    {
        public static string TRANSACTIONS_IDENTITY_KEY_PROVIDER_NAME = "TransactionRegistry";

        private readonly Dictionary<string, IIdentityKeyProvider> _identityKeyProviders;
        private readonly IIdentityKeyProvider _currentIdentityKeyProvider;

        private readonly ILog _log = LogManager.GetLogger(Assembly.GetCallingAssembly(), typeof(IdentityKeyProvidersRegistry));

        public IdentityKeyProvidersRegistry(IConfigurationService configurationService, IEnumerable<IIdentityKeyProvider> identityKeyProviders)
        {
            _log.Info($"{GetType().FullName} ctor");
            _identityKeyProviders = new Dictionary<string, IIdentityKeyProvider>();

            if(identityKeyProviders == null)
            {
                return;
            }

            foreach (IIdentityKeyProvider item in identityKeyProviders)
            {
                if(!_identityKeyProviders.ContainsKey(item.Name))
                {
                    _identityKeyProviders.Add(item.Name, item);
                }
            }

            IIdentityConfiguration identityConfiguration = configurationService?.Get<IIdentityConfiguration>();
            string currentIdentityKeyProviderName = identityConfiguration?.Provider;

            if(string.IsNullOrEmpty(currentIdentityKeyProviderName))
            {
                throw new IdentityConfigurationSectionCorruptedException();
            }

            if(!_identityKeyProviders.ContainsKey(currentIdentityKeyProviderName))
            {
                throw new IdentityProviderNotSupportedException(currentIdentityKeyProviderName);
            }

            _currentIdentityKeyProvider = _identityKeyProviders[currentIdentityKeyProviderName];
        }

        public IIdentityKeyProvider GetInstance()
        {
            return _currentIdentityKeyProvider;
        }

        public IIdentityKeyProvider GetInstance(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (!_identityKeyProviders.ContainsKey(key))
            {
                throw new IdentityProviderNotSupportedException(key);
            }

            return _identityKeyProviders[key];
        }

        public T1 GetInstance<T1>() where T1 : IIdentityKeyProvider => 
            (T1)_identityKeyProviders.Values.FirstOrDefault(s => s is T1);

        public IIdentityKeyProvider GetTransactionsIdenityKeyProvider() => 
            GetInstance(TRANSACTIONS_IDENTITY_KEY_PROVIDER_NAME);
    }
}
