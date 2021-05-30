using O10.Core.Architecture;

using System;
using System.Collections.Generic;
using System.Linq;

namespace O10.Client.Common.Integration
{
    [RegisterDefaultImplementation(typeof(IIntegrationIdPRepository), Lifetime = LifetimeManagement.Singleton)]
    public class IntegrationIdPRepository : IIntegrationIdPRepository
    {
        private readonly IEnumerable<IIntegrationIdP> _integrationIdPs;

        public IntegrationIdPRepository(IEnumerable<IIntegrationIdP> integrationIdPs)
        {
            _integrationIdPs = integrationIdPs;
        }

        public string IntegrationKeyName => "IntegrationKey";

        public IIntegrationIdP GetInstance(string key)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var integrationIdP = _integrationIdPs.FirstOrDefault(i => i.Key == key);

            if (integrationIdP == null)
            {
                throw new ArgumentOutOfRangeException(nameof(key));
            }

            return integrationIdP;
        }

        public IEnumerable<IIntegrationIdP> GetIntegrationIdPs()
        {
            return _integrationIdPs;
        }
    }
}
