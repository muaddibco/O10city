using O10.Core.Configuration;
using O10.Core.Logging;
using System;
using O10.Transactions.Core.Enums;
using O10.Core.Persistency;

namespace O10.Node.DataLayer.DataAccess
{
    public abstract class NodeDataAccessServiceBase<T> : DataAccessServiceBase<T>, INodeDataAccessService where T : NodeDataContextBase
	{
		private readonly INodeDataContextRepository _dataContextRepository;

		protected NodeDataAccessServiceBase(INodeDataContextRepository dataContextRepository,
                                            IConfigurationService configurationService,
                                            ILoggerService loggerService)
			: base(configurationService, loggerService)
		{
            _dataContextRepository = dataContextRepository ?? throw new ArgumentNullException(nameof(dataContextRepository));
		}

		public abstract LedgerType LedgerType { get; }

		protected override T GetDataContext() => (T)_dataContextRepository.GetInstance(LedgerType, _configuration.ConnectionType);
	}
}
