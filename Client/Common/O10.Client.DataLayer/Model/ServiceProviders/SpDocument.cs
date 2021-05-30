using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model
{
    [Table("sp_documents")]
    public class SpDocument
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SpDocumentId { get; set; }

        public Account Account { get; set; }

        public string DocumentName { get; set; }

        public string Hash { get; set; }

        public ulong LastChangeRecordHeight { get; set; }

        public virtual ICollection<SpDocumentAllowedSigner> AllowedSigners { get; set; }

        public virtual ICollection<SpDocumentSignature> DocumentSignatures { get; set; }
    }
}
