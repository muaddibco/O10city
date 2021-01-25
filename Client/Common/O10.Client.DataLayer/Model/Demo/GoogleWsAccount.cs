using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model.Demo
{
    [Table("google_ws_accounts")]
	public class GoogleWsAccount
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long GoogleWsAccountId { get; set; }

		public virtual WorkSpace WorkSpace { get; set; }

		public string Email { get; set; }
	}
}
