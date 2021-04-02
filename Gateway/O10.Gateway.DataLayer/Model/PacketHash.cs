using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Gateway.DataLayer.Model
{
    [Table("PacketHashes")]
    public class PacketHash
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long PacketHashId { get; set; }

        public long CombinedRegistryBlockHeight { get; set; }

        [Column(TypeName = "varbinary(64)")]
		public string Hash { get; set; }
    }
}
