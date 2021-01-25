using System.Threading;
using O10.Core;
using O10.Core.Architecture;


namespace O10.Client.Web.Portal.Services
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Singleton)]
    public class ConsentManagementInitializer : InitializerBase
    {
        private readonly IConsentManagementService _consentManagementService;
        private readonly IExecutionContextManager _executionContextManager;

        public ConsentManagementInitializer(IConsentManagementService consentManagementService, IExecutionContextManager executionContextManager)
        {
            _consentManagementService = consentManagementService;
            _executionContextManager = executionContextManager;
        }

        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Lowest1;

        protected override void InitializeInner(CancellationToken cancellationToken)
        {
            _consentManagementService.Initialize(_executionContextManager, cancellationToken);
        }
    }
}
