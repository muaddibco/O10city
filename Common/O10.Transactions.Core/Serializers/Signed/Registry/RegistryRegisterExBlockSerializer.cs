using O10.Core.Architecture;
using O10.Core.ExtensionMethods;
using O10.Transactions.Core.DataModel.Registry;
using O10.Transactions.Core.Enums;
using System;
using System.IO;

namespace O10.Transactions.Core.Serializers.Signed.Registry
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.Transient)]
    public class RegistryRegisterExBlockSerializer : SignatureSupportSerializerBase<RegistryRegisterExBlock>
    {
        public RegistryRegisterExBlockSerializer(IServiceProvider serviceProvider) 
            : base(serviceProvider, PacketType.Registry, ActionTypes.Registry_RegisterEx)
        {
        }
    
        protected override void WriteBody(BinaryWriter bw)
        {
            bw.Write((ushort)_block.ReferencedPacketType);

            byte[] serialized = _block.Parameters.Serialize();
            bw.Write(serialized.Length);
            bw.Write(serialized);
        }
    }
}
