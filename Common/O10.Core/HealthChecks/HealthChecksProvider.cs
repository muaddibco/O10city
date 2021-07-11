using O10.Core.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;

namespace O10.Core.HealthChecks
{
    [RegisterDefaultImplementation(typeof(IHealthChecksProvider), Lifetime = LifetimeManagement.Singleton)]
    public class HealthChecksProvider : IHealthChecksProvider
    {
        private readonly IEnumerable<IHealthCheckService> _healthCheckServices;

        public HealthChecksProvider(IEnumerable<IHealthCheckService> healthCheckServices)
        {
            _healthCheckServices = healthCheckServices;
        }

        public IHealthCheckService GetInstance(Type key)
        {
            var healthCheck = _healthCheckServices.FirstOrDefault(s => s.GetType() == key);

            if(healthCheck == null)
            {
                throw new ArgumentOutOfRangeException(nameof(key));
            }

            return healthCheck;
        }
    }
}
