﻿using System;
using System.Collections.Generic;
using O10.Core.Architecture;

namespace O10.Core.Tracking
{
    [ServiceContract]
    public interface ITrackingService
    {
        void TrackMetric(string name, double value, params string[] dimensions);
        void TrackDependency(string dependencyTypeName, string dependencyName, string data, DateTimeOffset start, TimeSpan duration, bool isSucceeded = true);
		void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null);
		void TrackTrace(string msg);
		void TrackException(Exception exception);
	}
}
