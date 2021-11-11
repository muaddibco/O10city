using System;
using System.Collections.Generic;

namespace O10.Tracking.Core
{
	public abstract class TrackingReporterBase : ITrackingReporter
	{
		private readonly object _sync = new object();
		private bool _isInitialized = false;

        public abstract string Name { get; }

        protected abstract void InitializeInner(string category);

		internal bool Initialize(string category)
		{
			if(_isInitialized)
			{
				return false;
			}

			lock(_sync)
			{
				if (_isInitialized)
				{
					return false;
				}

				_isInitialized = true;
			}

			InitializeInner(category);

			return true;
		}

		public abstract void TrackMetric(string name, double value, params string[] dimensions);

        public abstract void TrackDependency(string dependencyTypeName, string dependencyName, string data, DateTimeOffset start, TimeSpan duration, bool isSucceeded = true);
		public abstract void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null);
		public abstract void TrackTrace(string msg);
		public abstract void TrackException(Exception exception);
	}
}
