using System;
using System.IO;
using O10.Transactions.Core.DataModel.Registry;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;


namespace O10.Transactions.Core.Serializers.Signed.Registry
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.Transient)]
    public class RegistryConfirmationBlockSerializer : SignatureSupportSerializerBase<RegistryConfirmationBlock>
    {
        public RegistryConfirmationBlockSerializer(IServiceProvider serviceProvider) : base(serviceProvider, PacketType.Registry, ActionTypes.Registry_ConfirmationBlock)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            bw.Write(_block.ReferencedBlockHash);
        }
    }
}
