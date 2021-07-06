using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model.Demo
{
    [Table("FacebookWsAccounts")]
    public class FacebookWsAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long FacebookWsAccountId { get; set; }

        public WorkSpace? WorkSpace { get; set; }
        public string Email { get; set; }
    }
}
