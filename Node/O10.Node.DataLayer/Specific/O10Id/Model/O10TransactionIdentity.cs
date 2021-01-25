using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Node.DataLayer.Specific.O10Id.Model
{
    [Table("O10TransactionIdentities")]
    public class O10TransactionIdentity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long O10TransactionIdentityId { get; set; }

        public AccountIdentity Identity { get; set; }

        public virtual ICollection<O10Transaction> Transactions { get; set; }
    }
}
