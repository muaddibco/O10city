using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using O10.IdentityProvider.DataLayer.Model.Enums;

namespace O10.IdentityProvider.DataLayer.Model
{
	[Table("user_records")]
	public class UserRecord
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long UserRecordId { get; set; }

		public string AssetId { get; set; }

		public string IssuanceCommitment { get; set; }

		public string IssuanceBiometricCommitment { get; set; }

		public string ProtectionCommitment { get; set; }

		public long IssuanceBlindingRecordId { get; set; }

		public UserRecordStatus Status { get; set; }

		public DateTime CreationTime { get; set; }

		public DateTime LastUpdateTime { get; set; }
	}
}
