using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers.O10State
{
    public class EmployeeRecord : O10StatePacket
    {
        public override ushort Version => 1;

        public override ushort PacketType => TransactionTypes.Transaction_EmployeeRecord;

        public byte[] RegistrationCommitment { get; set; }

        public byte[] GroupCommitment { get; set; }

		//public SurjectionProof GroupSurjectionProof { get; set; }
	}
}
