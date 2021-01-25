namespace O10.Core.Models
{
	public class WitnessPackage
	{
		public ulong CombinedBlockHeight { get; set; }

		public PacketWitness[] StateWitnesses { get; set; }
		public PacketWitness[] StealthWitnesses { get; set; }
	}
}
