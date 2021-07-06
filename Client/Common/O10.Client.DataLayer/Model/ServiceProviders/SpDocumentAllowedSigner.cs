using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model
{
    [Table("SpDocumentAllowedSigners")]
    public class SpDocumentAllowedSigner
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SpDocumentAllowedSignerId { get; set; }

        public Account? Account { get; set; }

        public SpDocument? Document { get; set; }

        public string GroupIssuer { get; set; }

        public string GroupName { get; set; }

        public string GroupCommitment { get; set; }

        public string BlindingFactor { get; set; }
    }
}
