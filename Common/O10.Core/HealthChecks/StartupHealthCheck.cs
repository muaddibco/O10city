using Microsoft.Extensions.Diagnostics.HealthChecks;
using O10.Core.Architecture;
using System.Threading;
using System.Threading.Tasks;

namespace O10.Core.HealthChecks
{
    [RegisterExtension(typeof(IHealthCheckService), Lifetime = LifetimeManagement.Singleton)]
    public class StartupHealthCheck : IHealthCheckService
    {
        public const string NAME = "Startup";

        public string Name => NAME;

        public bool IsStartupCompleted { get; set; }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (IsStartupCompleted)
            {
                return Task.FromResult(
                    HealthCheckResult.Healthy("The startup task is finished."));
            }

            return Task.FromResult(
                HealthCheckResult.Unhealthy("The startup task is still running."));
        }
    }
}
