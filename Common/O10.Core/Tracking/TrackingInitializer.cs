using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using O10.Core.Architecture;

using O10.Core.Configuration;

namespace O10.Core.Tracking
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Singleton)]
    public class TrackingInitializer : InitializerBase
    {
        private readonly IEnumerable<ITrackingReporter> _trackingReporters;

        public TrackingInitializer(IConfigurationService configurationService, IEnumerable<ITrackingReporter> trackingReporters)
        {
            string[] trackingReporterNames = configurationService?.Get<TrackingConfiguration>()?.TrackingReporterNames ?? Array.Empty<string>();
            _trackingReporters = trackingReporters.Where(t => trackingReporterNames?.Contains(t.Name) ?? false).ToArray();
        }

        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Highest;

        protected override void InitializeInner(CancellationToken cancellationToken)
        {
            foreach (ITrackingReporter reporter in _trackingReporters)
            {
                (reporter as TrackingReporterBase)?.Initialize("");
            }
        }
    }
}
