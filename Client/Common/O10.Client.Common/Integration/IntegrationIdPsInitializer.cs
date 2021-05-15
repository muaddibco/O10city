using O10.Core;
using O10.Core.Architecture;

using System.Threading;
using System.Threading.Tasks;

namespace O10.Client.Common.Integration
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Singleton)]
    public class IntegrationIdPsInitializer : InitializerBase
    {
        private readonly IIntegrationIdPRepository _integrationIdPRepository;

        public IntegrationIdPsInitializer(IIntegrationIdPRepository integrationIdPRepository)
        {
            _integrationIdPRepository = integrationIdPRepository;
        }

        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Normal;

        protected override async Task InitializeInner(CancellationToken cancellationToken)
        {
            foreach (var idP in _integrationIdPRepository.GetIntegrationIdPs())
            {
                idP.Initialize();
            }
        }
    }
}
