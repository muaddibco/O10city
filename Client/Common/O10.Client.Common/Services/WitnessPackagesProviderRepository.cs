using System;
using System.Collections.Generic;
using System.Linq;
using O10.Client.Common.Interfaces;
using O10.Core.Architecture;


namespace O10.Client.Common.Services
{
    [RegisterDefaultImplementation(typeof(IWitnessPackagesProviderRepository), Lifetime = LifetimeManagement.Scoped)]
    public class WitnessPackagesProviderRepository : IWitnessPackagesProviderRepository
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEnumerable<IWitnessPackagesProvider> _witnessPackagesProviders;

        public WitnessPackagesProviderRepository(IServiceProvider serviceProvider, IEnumerable<IWitnessPackagesProvider> witnessPackagesProviders)
        {
            _serviceProvider = serviceProvider;
            _witnessPackagesProviders = witnessPackagesProviders;
        }

        public IWitnessPackagesProvider GetInstance(string key)
        {
            return _witnessPackagesProviders.FirstOrDefault(s => s.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
