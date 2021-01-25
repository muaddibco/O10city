using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Gateway.DataLayer.Model
{
    [Table("RegistryFullBlocks")]
    public class RegistryFullBlockData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RegistryFullBlockDataId { get; set; }

        public long CombinedBlockHeight { get; set; }

        public byte[] Content { get; set; }
    }
}
