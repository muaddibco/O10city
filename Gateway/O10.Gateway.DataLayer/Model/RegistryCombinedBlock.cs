using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Gateway.DataLayer.Model
{
    [Table("RegistryCombinedBlocks")]
    public class RegistryCombinedBlock
    {
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long RegistryCombinedBlockId { get; set; }

        public string Content { get; set; }
    }
}
