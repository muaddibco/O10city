using System.Collections;
using System.Collections.Generic;

namespace O10.Core.Models
{
	public class WitnessPackage
	{
		public long CombinedBlockHeight { get; set; }

		public List<PacketWitness>? Witnesses { get; set; }
	}
}
