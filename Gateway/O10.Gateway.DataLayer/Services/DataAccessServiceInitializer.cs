using System.Threading;
using O10.Core;
using O10.Core.Architecture;

namespace O10.Gateway.DataLayer.Services
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Singleton)]
    public class DataAccessServiceInitializer : IInitializer
    {
        private readonly IDataAccessService _dataAccessService;

		public DataAccessServiceInitializer(IDataAccessService dataAccessService)
        {
            _dataAccessService = dataAccessService;
		}
        public ExtensionOrderPriorities Priority => ExtensionOrderPriorities.AboveNormal9;

        public bool Initialized { get; private set; }

        public void Initialize(CancellationToken cancellationToken)
        {
            if (!Initialized)
            {
                _dataAccessService.Initialize(cancellationToken);
                Initialized = true;
            }
        }
    }
}
