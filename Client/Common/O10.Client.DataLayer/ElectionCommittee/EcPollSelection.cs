using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.ElectionCommittee
{
    [Table("EcPollSelections")]
    public class EcPollSelection
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long EcPollSelectionId { get; set; }

        public virtual EcPollRecord EcPollRecord { get; set; }

        public string EcCommitment { get; set; }

        public string EcBlindingFactor { get; set; }

        public string VoterBlindingFactor { get; set; }
    }
}
