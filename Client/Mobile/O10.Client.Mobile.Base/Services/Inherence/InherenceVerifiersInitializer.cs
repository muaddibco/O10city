using System.Threading;
using O10.Core;
using O10.Core.Architecture;
using O10.Client.Mobile.Base.Interfaces;
using System.Threading.Tasks;

namespace O10.Client.Mobile.Base.Services.Inherence
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Scoped)]
    public class InherenceVerifiersInitializer : InitializerBase
    {
        private readonly IVerifierInteractionsManager _verifierInteractionsManager;

        public InherenceVerifiersInitializer(IVerifierInteractionsManager verifierInteractionsManager)
        {
            _verifierInteractionsManager = verifierInteractionsManager;
        }

        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Normal;

        protected override async Task InitializeInner(CancellationToken cancellationToken)
        {
            _verifierInteractionsManager.Initialize();
        }
    }
}
