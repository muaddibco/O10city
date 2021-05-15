using System.Threading;
using System.Threading.Tasks;
using O10.Core;
using O10.Core.Architecture;


namespace O10.IdentityProvider.DataLayer.Services
{
	[RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Singleton)]
	public class DataAccessServiceInitializer : InitializerBase
	{
		private readonly IDataAccessService _dataAccessService;

		public DataAccessServiceInitializer(IDataAccessService dataAccessService)
		{
			_dataAccessService = dataAccessService;
		}

		public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.AboveNormal;

		protected override async Task InitializeInner(CancellationToken cancellationToken)
		{
			_dataAccessService.Initialize();
		}
	}
}
