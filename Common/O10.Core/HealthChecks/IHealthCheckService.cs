using Microsoft.Extensions.Diagnostics.HealthChecks;
using O10.Core.Architecture;

namespace O10.Core.HealthChecks
{
    [ExtensionPoint]
    public interface IHealthCheckService : IHealthCheck
    {
        public string Name { get; }
    }
}
