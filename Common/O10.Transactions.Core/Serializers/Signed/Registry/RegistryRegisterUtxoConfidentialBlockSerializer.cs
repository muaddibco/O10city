using System;
using System.IO;
using O10.Transactions.Core.DataModel.Registry;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Serializers.Stealth;
using O10.Core.Architecture;


namespace O10.Transactions.Core.Serializers.Signed.Registry
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.Transient)]
    public class RegistryRegisterStealthBlockSerializer : StealthSerializerBase<RegistryRegisterStealth>
    {
        public RegistryRegisterStealthBlockSerializer(IServiceProvider serviceProvider) : base(serviceProvider, PacketType.Registry, ActionTypes.Registry_RegisterStealth)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            bw.Write((ushort)_block.ReferencedPacketType);
            bw.Write(_block.ReferencedBlockType);
            bw.Write(_block.ReferencedBodyHash);
        }
    }
}
