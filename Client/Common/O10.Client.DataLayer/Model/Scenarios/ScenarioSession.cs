using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model.Scenarios
{
    [Table("scenario_sessions")]
    public class ScenarioSession
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ScenarioSessionId { get; set; }

        public string UserSubject { get; set; }

        public int ScenarioId { get; set; }

        public DateTime StartTime { get; set; }

        public int CurrentStep { get; set; }
    }
}
