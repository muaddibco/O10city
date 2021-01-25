using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.ElectionCommittee
{
    [Table("EcPollRecords")]
    public class EcPollRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long EcPollRecordId { get; set; }
        public string Name { get; set; }
        public int State { get; set; }
        public long AccountId { get; set; }
        public virtual ICollection<EcCandidateRecord> Candidates { get; set; }
        public virtual ICollection<EcPollSelection> PollSelections { get; set; }
    }
}
