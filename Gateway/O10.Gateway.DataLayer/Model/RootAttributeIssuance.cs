using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Gateway.DataLayer.Model
{
    [Table("RootAttributes")]
    public class RootAttribute
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RootAttributeIssuanceId { get; set; }

        public Address Issuer { get; set; }

		[Column(TypeName = "varbinary(64)")]
		public string IssuanceCommitment { get; set; }

		[Column(TypeName = "varbinary(64)")]
		public string RootCommitment { get; set; }

        public bool IsOverriden { get; set; }

        public long IssuanceCombinedBlock { get; set; }
        public long RevocationCombinedBlock { get; set; }
    }
}
