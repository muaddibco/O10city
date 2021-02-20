using System;
using System.IO;
using O10.Transactions.Core.Ledgers.Registry;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;


namespace O10.Transactions.Core.Serializers.Signed.Registry
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.Transient)]
    public class RegistryRegisterBlockSerializer : SignatureSupportSerializerBase<RegistryRegisterBlock>
    {
        public RegistryRegisterBlockSerializer(IServiceProvider serviceProvider) : base(serviceProvider, LedgerType.Registry, PacketTypes.Registry_Register)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            bw.Write((ushort)_block.ReferencedLedgerType);
            bw.Write(_block.ReferencedBlockType);
            bw.Write(_block.ReferencedBodyHash);
            bw.Write(_block.ReferencedTarget);

			if((_block.ReferencedBlockType & PacketTypes.TransitionalFlag) == PacketTypes.TransitionalFlag)
			{
				bw.Write(_block.ReferencedTransactionKey);
			}
		}
    }
}
