using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Node.DataLayer.Specific.O10Id.Model
{
	[Table("O10AccountIdentity")]
    public class AccountIdentity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AccountIdentityId { get; set; }

        public ulong KeyHash { get; set; }

		[Column(TypeName = "varbinary(64)")]
        public string PublicKey { get; set; }
    }
}
