using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.DataModel.Synchronization
{
    public class SynchronizationRegistryCombinedBlock : SynchronizationBlockBase
    {
        public override ushort BlockType => ActionTypes.Synchronization_RegistryCombinationBlock;

        public override ushort Version => 1;

        public byte[][] BlockHashes { get; set; }
    }
}
