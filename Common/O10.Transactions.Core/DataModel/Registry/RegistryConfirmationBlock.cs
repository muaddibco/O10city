using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.DataModel.Registry
{

    public class RegistryConfirmationBlock : RegistryBlockBase
    {
        public override ushort BlockType => ActionTypes.Registry_ConfirmationBlock;

        public override ushort Version => 1;

        public byte[] ReferencedBlockHash { get; set; }

        //public class ConfidenceDescriptor
        //{
        //    public ushort Confidence { get; set; }

        //    public byte[] Signature{ get; set; }

        //    public IKey Signer { get; set; }
        //}
    }
}
