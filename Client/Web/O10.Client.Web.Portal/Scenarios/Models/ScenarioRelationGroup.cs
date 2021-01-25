using System.Collections.Generic;

namespace O10.Client.Web.Portal.Scenarios.Models
{
    public class ScenarioRelationGroup
    {
        public string GroupName { get; set; }

        public IEnumerable<ScenarioRelation> Relations { get; set; }
    }
}
