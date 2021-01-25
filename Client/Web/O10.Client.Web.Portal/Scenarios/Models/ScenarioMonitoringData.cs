using System;

namespace O10.Client.Web.Portal.Scenarios.Models
{
    public class ScenarioMonitoringData
    {
        public int ScenarioId { get; set; }

        public long ScenarioSessionId { get; set; }

        public DateTime ActivationTime { get; set; }

        public DateTime LastUseTime { get; set; }
    }
}
