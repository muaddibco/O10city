using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.DataModel.Transactional
{
    public class EmployeeRecord : TransactionalPacketBase
    {
        public override ushort Version => 1;

        public override ushort BlockType => ActionTypes.Transaction_EmployeeRecord;

        public byte[] RegistrationCommitment { get; set; }

        public byte[] GroupCommitment { get; set; }

		//public SurjectionProof GroupSurjectionProof { get; set; }
	}
}
