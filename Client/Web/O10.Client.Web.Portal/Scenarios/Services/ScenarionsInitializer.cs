using System.Threading;
using System.Threading.Tasks;
using O10.Core;
using O10.Core.Architecture;


namespace O10.Client.Web.Portal.Scenarios.Services
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Scoped)]
    public class ScenarionsInitializer : InitializerBase
    {
        private readonly IScenarioRunner _scenarioRunner;

        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Normal;

        public ScenarionsInitializer(IScenarioRunner scenarioRunner)
        {
            _scenarioRunner = scenarioRunner;
        }

        protected override async Task InitializeInner(CancellationToken cancellationToken)
        {
            _scenarioRunner.Initialize();
        }
    }
}
