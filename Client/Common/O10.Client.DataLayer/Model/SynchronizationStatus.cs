using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model
{
    [Table("synchronization_statuses")]
    public class SynchronizationStatus
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SynchronizationStatusId { get; set; }

        public Account Account { get; set; }

        public ulong LastUpdatedCombinedBlockHeight { get; set; }
    }
}
