using System;

namespace O10.Client.Web.Portal.Dtos.Scenarios
{
    public class ScenarioDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public long SessionId { get; set; }
        public int CurrentStep { get; set; }
        public DateTime StartTime { get; set; }

        public ScenarioStepDto[] Steps { get; set; }
    }
}
