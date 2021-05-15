using Newtonsoft.Json;
using O10.Core.Identity;
using O10.Core.Serialization;

namespace O10.Core.Models
{
	public class PacketWitness
    {
        public long WitnessId { get; set; }

		[JsonConverter(typeof(KeyJsonConverter))]
		public IKey? DestinationKey { get; set; }

		[JsonConverter(typeof(KeyJsonConverter))]
		public IKey? DestinationKey2 { get; set; }

		[JsonConverter(typeof(KeyJsonConverter))]
		public IKey? TransactionKey { get; set; }

		[JsonConverter(typeof(KeyJsonConverter))]
		public IKey? KeyImage { get; set; }

		public bool IsIdentityIssuing { get; set; }
	}
}
