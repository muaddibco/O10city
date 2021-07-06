using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model.Demo
{
    [Table("GoogleWsAccounts")]
    public class GoogleWsAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long GoogleWsAccountId { get; set; }

        public WorkSpace? WorkSpace { get; set; }

        public string Email { get; set; }
    }
}
