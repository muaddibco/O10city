using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.DataModel
{
	public class PacketInfo
	{
		public PacketType PacketType { get; set; }

		public ushort BlockType { get; set; }

		public byte[] Content { get; set; }
	}
}
