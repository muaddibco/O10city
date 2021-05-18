using System.Threading;
using System.Threading.Tasks;
using O10.Core;
using O10.Core.Architecture;


namespace O10.Client.DataLayer.Services
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Singleton)]
    public class DataAccessServiceInitializer : IInitializer
    {
        private readonly IDataAccessService _dataAccessService;

		public DataAccessServiceInitializer(IDataAccessService dataAccessService)
        {
            _dataAccessService = dataAccessService;
		}
        public ExtensionOrderPriorities Priority => ExtensionOrderPriorities.AboveNormal;

        public bool Initialized { get; private set; }

        public async Task Initialize(CancellationToken cancellationToken)
        {
            if (!Initialized)
            {
                Initialized = _dataAccessService.Initialize();
            }
        }
    }
}
