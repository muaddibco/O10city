using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model
{
    [Table("sp_document_signatures")]
    public class SpDocumentSignature
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SpDocumentSignatureId { get; set; }

        public SpDocument Document { get; set; }

        public ulong SignatureRecordHeight { get; set; }

        public ulong DocumentRecordHeight { get; set; }

        public byte[] DocumentSignRecord { get; set; }
    }
}
