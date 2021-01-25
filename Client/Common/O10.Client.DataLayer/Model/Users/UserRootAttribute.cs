using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model
{
    [Table("user_root_attributes")]
	public class UserRootAttribute
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long UserAttributeId { get; set; }

		public long AccountId { get; set; }

		public string SchemeName { get; set; }

		public string Content { get; set; }

		[Required]
		public byte[] AssetId { get; set; }

		[Required]
		public string Source { get; set; }

		[Required]
		public byte[] IssuanceCommitment { get; set; }

		[Required]
		public byte[] OriginalBlindingFactor { get; set; }

		[Required]
		public byte[] OriginalCommitment { get; set; }

		[Required]
		public byte[] LastBlindingFactor { get; set; }

		[Required]
		public byte[] LastCommitment { get; set; }

		[Required]
		public byte[] LastTransactionKey { get; set; }

		[Required]
		public byte[] LastDestinationKey { get; set; }

		[Required]
		public string NextKeyImage { get; set; }

		public bool IsOverriden { get; set; }

        public DateTime? CreationTime { get; set; }
        
        public DateTime? ConfirmationTime { get; set; }

        public DateTime? LastUpdateTime { get; set; }
    }
}
