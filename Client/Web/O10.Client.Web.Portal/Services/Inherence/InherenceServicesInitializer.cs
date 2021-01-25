using System;
using System.Threading;
using O10.Core;
using O10.Core.Architecture;

using O10.Core.Logging;

namespace O10.Client.Web.Portal.Services.Inherence
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Singleton)]
    public class InherenceServicesInitializer : InitializerBase
    {
        private readonly IInherenceServicesManager _inherenceServicesManager;
        private readonly ILogger _logger;

        public InherenceServicesInitializer(IInherenceServicesManager inherenceServicesManager, ILoggerService loggerService)
        {
            _inherenceServicesManager = inherenceServicesManager;
            _logger = loggerService.GetLogger(nameof(InherenceServicesInitializer));
        }

        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Normal;

        protected override void InitializeInner(CancellationToken cancellationToken)
        {
            foreach (var inherenceService in _inherenceServicesManager.GetAll())
            {
                try
                {
                    inherenceService.Initialize(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to initialize {inherenceService.GetType().FullName}", ex);
                }
            }
        }
    }
}
