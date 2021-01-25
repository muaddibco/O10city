using O10.Transactions.Core.DataModel.Transactional.Internal;
using O10.Transactions.Core.Enums;
using O10.Core.Cryptography;

namespace O10.Transactions.Core.DataModel.Transactional
{
    public class BlindAsset : TransactionalPacketBase
    {
        public override ushort Version => 1;

        public override ushort BlockType => ActionTypes.Transaction_BlindAsset;

        public EncryptedAsset EncryptedAsset { get; set; }

        public SurjectionProof SurjectionProof { get; set; }
    }
}
