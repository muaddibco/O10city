namespace O10.Core.Models
{
	public class PacketWitness
    {
        public long WitnessId { get; set; }

		public byte[] DestinationKey { get; set; }

		public byte[] DestinationKey2 { get; set; }

		public byte[] TransactionKey { get; set; }

        public byte[] KeyImage { get; set; }

		public bool IsIdentityIssuing { get; set; }
	}
}
