using System.Collections.Generic;

namespace O10.Client.Web.Portal.Scenarios.Models
{
    public class ScenarioDefinition
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ScenarioSetup Setup { get; set; }

        public IEnumerable<ScenarioStep> Steps { get; set; }
    }
}
