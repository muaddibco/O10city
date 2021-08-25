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

		/// <summary>
		/// A commitment to the Root Identity Asset ID of the user that this relation established for plus non-blinded commitment to the Asset ID of the group that this relations established with
		/// </summary>
		[Required]
		[Column(TypeName = "varbinary(64)")]
		public string RegistrationCommitment { get; set; }

		/*[Required]
		[Column(TypeName = "varbinary(64)")]
		public string GroupCommitment { get; set; }*/

		public bool IsRevoked { get; set; }
	}
}
