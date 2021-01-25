using System;
using System.IO;
using O10.Transactions.Core.DataModel.Registry;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;


namespace O10.Transactions.Core.Serializers.Signed.Registry
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.Transient)]
    public class RegistryConfidenceBlockSerializer : SignatureSupportSerializerBase<RegistryConfidenceBlock>
    {
        public RegistryConfidenceBlockSerializer(IServiceProvider serviceProvider) : base(serviceProvider, PacketType.Registry, ActionTypes.Registry_ConfidenceBlock)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            bw.Write((ushort)_block.BitMask.Length);
            bw.Write(_block.BitMask);
            bw.Write(_block.ConfidenceProof);
            bw.Write(_block.ReferencedBlockHash);
        }
    }
}
