using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.DataModel.Registry
{
    public class RegistryFullBlock : RegistryBlockBase
    {
        public override ushort BlockType => ActionTypes.Registry_FullBlock;

        public override ushort Version => 1;

        public RegistryRegisterBlock[] StateWitnesses { get; set; }
        public RegistryRegisterStealth[] UtxoWitnesses { get; set; }

        public byte[] ShortBlockHash { get; set; }
    }
}
