using System;
using System.Collections.Generic;
using System.Linq;
using O10.Core.Architecture;

using O10.Core.Configuration;
using O10.Core.Tracking;

namespace O10.Core.PerformanceCounters
{

    [RegisterDefaultImplementation(typeof(ITrackingService), Lifetime = LifetimeManagement.Singleton)]
    public class TrackingService : ITrackingService
    {
        private readonly IEnumerable<ITrackingReporter> _trackingReporters;

        public TrackingService(IConfigurationService configurationService, IEnumerable<ITrackingReporter> trackingReporters)
        {
            string[] trackingReporterNames = configurationService?.Get<TrackingConfiguration>()?.TrackingReporterNames ?? Array.Empty<string>();
            _trackingReporters = trackingReporters.Where(t => trackingReporterNames?.Contains(t.Name) ?? false).ToArray();
        }

        public void TrackMetric(string name, double value, params string[] dimensions)
        {
            //TODO: make it in separate background worker function with blocking collection
            foreach (ITrackingReporter reporter in _trackingReporters)
            {
                reporter.TrackMetric(name, value, dimensions);
            }
        }

        public void TrackDependency(string dependencyTypeName, string dependencyName, string data, DateTimeOffset start, TimeSpan duration, bool isSucceeded = true)
        {
            //TODO: make it in separate background worker function with blocking collection
            foreach (ITrackingReporter reporter in _trackingReporters)
            {
                reporter.TrackDependency(dependencyTypeName, dependencyName, data, start, duration, isSucceeded);
            }
        }

		public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
		{
			//TODO: make it in separate background worker function with blocking collection
			foreach (ITrackingReporter reporter in _trackingReporters)
			{
				reporter.TrackEvent(eventName, properties, metrics);
			}
		}

		public void TrackTrace(string msg)
		{
			//TODO: make it in separate background worker function with blocking collection
			foreach (ITrackingReporter reporter in _trackingReporters)
			{
				reporter.TrackTrace(msg);
			}
		}

		public void TrackException(Exception exception)
		{
			//TODO: make it in separate background worker function with blocking collection
			foreach (ITrackingReporter reporter in _trackingReporters)
			{
				reporter.TrackException(exception);
			}
		}
	}
}
