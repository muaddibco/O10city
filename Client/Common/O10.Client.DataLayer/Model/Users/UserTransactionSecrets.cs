﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model
{
    [Table("user_transaction_secrets")]
    public class UserTransactionSecrets
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long UserTransactionSecretsId { get; set; }

        public long AccountId { get; set; }

        public string KeyImage { get; set; }

        public string Issuer { get; set; }

        public string AssetId { get; set; }
    }
}
