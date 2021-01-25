﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Node.DataLayer.Specific.O10Id.Model
{
	[Table("O10TransactionHashKeys")]
    public class O10TransactionHashKey
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long O10TransactionHashKeyId { get; set; }

        public ulong CombinedBlockHeight { get; set; }

        public ulong SyncBlockHeight { get; set; }

		[Column(TypeName = "varbinary(64)")]
		public string Hash { get; set; }
    }
}
