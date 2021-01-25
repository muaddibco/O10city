using O10.Core;
using O10.Core.Architecture;
using System.Threading;

namespace O10.Client.Web.Portal.ElectionCommittee
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Singleton)]
    public class ElectionCommitteeInitializer : InitializerBase
    {
        private readonly IElectionCommitteeService _electionCommitteeService;

        public ElectionCommitteeInitializer(IElectionCommitteeService electionCommitteeService)
        {
            _electionCommitteeService = electionCommitteeService;
        }

        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Normal;

        protected override void InitializeInner(CancellationToken cancellationToken)
        {
            _electionCommitteeService.Initialize();
        }
    }
}
