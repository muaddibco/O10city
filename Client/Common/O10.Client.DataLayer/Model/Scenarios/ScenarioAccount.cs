using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model.Scenarios
{
    [Table("scenario_accounts")]
    public class ScenarioAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ScenarioAccountId { get; set; }

        public virtual ScenarioSession ScenarioSession { get; set; }

        public long AccountId { get; set; }
    }
}
