using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model
{
    [Table("processed_witnesses")]
    public class ProcessedWitness
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ProcessedWitnessId { get; set; }

        public long AccountId { get; set; }
        public long WitnessId { get; set; }

        public DateTime Time { get; set; }
    }
}
