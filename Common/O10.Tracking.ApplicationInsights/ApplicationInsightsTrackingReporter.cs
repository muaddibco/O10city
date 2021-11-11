using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Tracking.Core;
using O10.Tracking.ApplicationInsights.Configuration;

namespace O10.Tracking.ApplicationInsights
{
    [RegisterExtension(typeof(ITrackingReporter), Lifetime = LifetimeManagement.Singleton)]
	public class ApplicationInsightsTrackingReporter : TrackingReporterBase
	{
		private readonly ApplicationInsightsConfiguration _configuration;
		private TelemetryClient _telemetryClient;

        public override string Name => "ApplicationInsights";

        public ApplicationInsightsTrackingReporter(IConfigurationService configurationService)
		{
			_configuration = configurationService.Get<ApplicationInsightsConfiguration>();
		}

		protected override void InitializeInner(string category)
		{
			TelemetryConfiguration.Active.InstrumentationKey = _configuration.InstrumentationKey;
			_telemetryClient = new TelemetryClient(TelemetryConfiguration.Active);
            _telemetryClient.InstrumentationKey = _configuration.InstrumentationKey;

            _telemetryClient.TrackTrace("Application Insights tracking initialized");
		}

		public override void TrackMetric(string name, double value, params string[] dimensions)
		{
			_telemetryClient.GetMetric(name).TrackValue(value);
		}

        public override void TrackDependency(string dependencyTypeName, string dependencyName, string data, DateTimeOffset start, TimeSpan duration, bool isSucceeded = true)
        {
            _telemetryClient.TrackDependency(dependencyTypeName, dependencyName, data, start, duration, isSucceeded);
        }

		public override void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
		{
			_telemetryClient.TrackEvent(eventName, properties, metrics);
		}

		public override void TrackTrace(string msg)
		{
			_telemetryClient.TrackTrace(msg);
		}

		public override void TrackException(Exception exception)
		{
			_telemetryClient.TrackException(exception);
		}
	}
}
