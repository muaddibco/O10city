using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Node.Core.DataLayer.DataContexts
{
    [Table("NodeRecords")]
    public class NodeRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long NodeRecordId { get; set; }

        [Column(TypeName = "varbinary(64)")]
        public string PublicKey { get; set; }

        [Column(TypeName = "varbinary(32)")]
		public string IPAddress { get; set; }

        public byte NodeRole { get; set; }
    }
}
