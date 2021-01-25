using O10.Core;
using O10.Core.Architecture;

using System.Threading;

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

        protected override void InitializeInner(CancellationToken cancellationToken)
        {
            foreach (var idP in _integrationIdPRepository.GetIntegrationIdPs())
            {
                idP.Initialize();
            }
        }
    }
}
