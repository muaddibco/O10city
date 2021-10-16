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

        public string PublicKey { get; set; }

		public string IPAddress { get; set; }

        public byte NodeRole { get; set; }
    }
}
