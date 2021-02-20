using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Node.DataLayer.Specific.O10Id.Model
{
    [Table("O10TransactionSources")]
    public class O10TransactionSource
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long O10TransactionSourceId { get; set; }

        public AccountIdentity Identity { get; set; }

        public virtual ICollection<O10Transaction> Transactions { get; set; }
    }
}
