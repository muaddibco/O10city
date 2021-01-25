using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Node.DataLayer.Specific.Synchronization.Model
{
	[Table("RegistryCombinedBlocks")]
    public class RegistryCombinedBlock
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long RegistryCombinedBlockId { get; set; }

        public ulong SyncBlockHeight { get; set; }

        public byte[] Content { get; set; }

		public string FullBlockHashes { get; set; }
	}
}
