using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers.O10State
{
    public class CancelEmployeeRecord : O10StatePacket
    {
        public override ushort Version => 1;

        public override ushort PacketType => TransactionTypes.Transaction_CancelEmployeeRecord;

        public byte[] RegistrationCommitment { get; set; }
    }
}
