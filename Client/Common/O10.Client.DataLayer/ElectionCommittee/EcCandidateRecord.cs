using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace O10.Client.DataLayer.ElectionCommittee
{
    [Table("EcCandidateRecords")]
    public class EcCandidateRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long EcCandidateRecordId { get; set; }

        public string Name { get; set; }
        public string AssetId { get; set; }
        public bool IsActive { get; set; }
        public virtual EcPollRecord EcPollRecord { get; set; }
    }
}
