using System.Threading;
using System.Threading.Tasks;
using O10.Core;
using O10.Core.Architecture;


namespace O10.Client.Web.Saml.Common.Services
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Singleton)]
    public class SamlIdpInitializer : InitializerBase
    {
        private readonly ISamlIdentityProvidersManager _samlIdentityProvidersManager;

        public SamlIdpInitializer(ISamlIdentityProvidersManager samlIdentityProvidersManager)
        {
            _samlIdentityProvidersManager = samlIdentityProvidersManager;
        }

        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Normal;

        protected override async Task InitializeInner(CancellationToken cancellationToken)
        {
            _samlIdentityProvidersManager.Initialize(cancellationToken);

            await Task.CompletedTask;
        }
    }
}
