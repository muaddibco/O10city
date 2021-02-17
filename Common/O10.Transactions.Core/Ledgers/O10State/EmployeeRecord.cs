using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers.O10State
{
    public class EmployeeRecord : TransactionalPacketBase
    {
        public override ushort Version => 1;

        public override ushort PacketType => PacketTypes.Transaction_EmployeeRecord;

        public byte[] RegistrationCommitment { get; set; }

        public byte[] GroupCommitment { get; set; }

		//public SurjectionProof GroupSurjectionProof { get; set; }
	}
}
