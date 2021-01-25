﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using O10.Client.DataLayer.Enums;

namespace O10.Client.DataLayer.Model
{
	[Table("attributes")]
	public class IdentityAttribute
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long AttributeId { get; set; }

        public virtual Identity Identity { get; set; }

        [Required]
		public string AttributeName { get; set; }

		[Required]
		public string Content { get; set; }

		public ClaimSubject Subject { get; set; }

        public byte[] Commitment { get; set; }

    }
}
