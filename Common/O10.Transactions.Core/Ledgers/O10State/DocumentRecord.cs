using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers.O10State
{
	public class DocumentRecord : O10StatePacket
    {
        public override ushort Version => 1;

        public override ushort PacketType => TransactionTypes.Transaction_DocumentRecord;

		public byte[] DocumentHash { get; set; }

		public byte[][] AllowedSignerGroupCommitments { get; set; }
	}
}
