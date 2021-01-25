using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Gateway.DataLayer.Model
{
    [Table("AssociatedAttributeIssuances")]
    public class AssociatedAttributeIssuance
    {
        public long AssociatedAttributeIssuanceId { get; set; }

        public Address Issuer { get; set; }

        [Required]
		[Column(TypeName = "varbinary(64)")]
		public string IssuanceCommitment { get; set; }

        [Required]
		[Column(TypeName = "varbinary(64)")]
		public string RootIssuanceCommitment { get; set; }
    }
}
