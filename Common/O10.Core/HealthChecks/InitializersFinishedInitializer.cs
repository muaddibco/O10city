using O10.Core.Architecture;
using System.Threading;
using System.Threading.Tasks;

namespace O10.Core.HealthChecks
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Scoped)]
    public class InitializersFinishedInitializer : InitializerBase
    {
        private readonly IHealthChecksProvider _healthChecksProvider;

        public InitializersFinishedInitializer(IHealthChecksProvider healthChecksProvider)
        {
            _healthChecksProvider = healthChecksProvider;
        }

        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Lowest9;

        protected override Task InitializeInner(CancellationToken cancellationToken)
        {
            var hc = (StartupHealthCheck)_healthChecksProvider.GetInstance(typeof(StartupHealthCheck));
            hc.IsStartupCompleted = true;

            return Task.CompletedTask;
        }
    }
}
