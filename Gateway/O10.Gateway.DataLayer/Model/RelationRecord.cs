using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Gateway.DataLayer.Model
{
	[Table("RelationRecords")]
	public class RelationRecord
	{
		[Key]
		public long RelationRecordId { get; set; }

		[Required]
		[Column(TypeName = "varbinary(64)")]
		public string Issuer { get; set; }

		[Required]
		[Column(TypeName = "varbinary(64)")]
		public string RegistrationCommitment { get; set; }

		[Required]
		[Column(TypeName = "varbinary(64)")]
		public string GroupCommitment { get; set; }

		public bool IsRevoked { get; set; }
	}
}
