using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Node.Core.DataLayer.DataContexts
{
	[Table("gateways")]
	public class Gateway
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int GatewayId { get; set; }

		public string Alias { get; set; }
		public string BaseUri { get; set; }
	}
}
