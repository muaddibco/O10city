using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model.Users
{
    [Table("UserIdentityIssuers")]
    public class UserIdentityIssuer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long UserIdentityIssuerId { get; set; }

        public string Key { get; set; }

        public string Alias { get; set; }

        public string Description { get; set; }

        public DateTime UpdateTime { get; set; }
    }
}
