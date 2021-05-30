using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model
{
    [Table("UserRootAttributes")]
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

        /// <summary>
        /// A commitment that is registered at the registry of an Identity Provider that is not changed with a time
        /// </summary>
        [Required]
        public byte[] AnchoringOriginationCommitment { get; set; }

        [Required]
        [Obsolete("Generate issuance blinding factor using Issuance Transaction Key")]
        public byte[] OriginalBlindingFactor { get; set; }

        [Required]
        public byte[] IssuanceTransactionKey { get; set; }

        [Required]
        public byte[] IssuanceCommitment { get; set; }

        [Required]
        [Obsolete("Generate blinding factor using Transaction Key and Secret View Key: bf = Hs(T * svk)")]
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
