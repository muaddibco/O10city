using System.Threading;
using System.Threading.Tasks;
using O10.Core;
using O10.Core.Architecture;


namespace O10.Client.Web.Portal.Services
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Scoped)]
    public class ConsentManagementInitializer : InitializerBase
    {
        private readonly IConsentManagementService _consentManagementService;
        private readonly IWebExecutionContextManager _executionContextManager;

        public ConsentManagementInitializer(IConsentManagementService consentManagementService, IWebExecutionContextManager executionContextManager)
        {
            _consentManagementService = consentManagementService;
            _executionContextManager = executionContextManager;
        }

        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Lowest1;

        protected override async Task InitializeInner(CancellationToken cancellationToken)
        {
            _consentManagementService.Initialize(_executionContextManager, cancellationToken);
        }
    }
}
