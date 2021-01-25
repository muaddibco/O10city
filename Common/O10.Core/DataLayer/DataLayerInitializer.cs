using System;
using System.Collections.Generic;
using System.Threading;
using O10.Core.Architecture;

using O10.Core.Logging;

namespace O10.Core.DataLayer
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Singleton)]
    public class DataLayerInitializer : InitializerBase
    {
        private readonly IEnumerable<IDataAccessService> _dataAccessServices;
		private readonly ILogger _logger;

        public DataLayerInitializer(IEnumerable<IDataAccessService> dataAccessServices, ILoggerService loggerService)
        {
            if (loggerService is null)
            {
                throw new ArgumentNullException(nameof(loggerService));
            }

            _dataAccessServices = dataAccessServices;
			_logger = loggerService.GetLogger(nameof(DataLayerInitializer));
        }

        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Highest7;

        protected override void InitializeInner(CancellationToken cancellationToken)
        {
            _logger.Info($"{nameof(DataLayerInitializer)} started");

            try
            {
                foreach (var dataAccessService in _dataAccessServices)
                {
                    dataAccessService.Initialize();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"{nameof(DataLayerInitializer)} failed", ex);
                throw;
            }
        }
    }
}
