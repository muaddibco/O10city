using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using O10.Core.Architecture;

using O10.Core.Exceptions;
using O10.Core.Cryptography;

namespace O10.Crypto.Services
{
    [RegisterDefaultImplementation(typeof(ISigningServicesRepository), Lifetime = LifetimeManagement.Singleton)]
    public class SigningServicesRepository : ISigningServicesRepository
    {
        private readonly Dictionary<string, Type> _signingServicesTypes;
        private readonly IServiceProvider _serviceProvider;

        public SigningServicesRepository(IEnumerable<ISigningService> signingServices, IServiceProvider serviceProvider)
        {
            _signingServicesTypes = signingServices?.ToDictionary(s => s.Name, s => s.GetType());
            _serviceProvider = serviceProvider;
        }

        public ISigningService GetInstance(string key)
        {
            if (!_signingServicesTypes.ContainsKey(key))
            {
                throw new SigningServiceNotSupportedException(key);
            }

            return (ISigningService)ActivatorUtilities.CreateInstance(_serviceProvider, _signingServicesTypes[key]);
        }
    }
}
