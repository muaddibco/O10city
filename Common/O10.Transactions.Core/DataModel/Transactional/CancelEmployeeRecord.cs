using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.DataModel.Transactional
{
    public class CancelEmployeeRecord : TransactionalPacketBase
    {
        public override ushort Version => 1;

        public override ushort BlockType => ActionTypes.Transaction_CancelEmployeeRecord;

        public byte[] RegistrationCommitment { get; set; }
    }
}
