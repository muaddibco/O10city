using System;
using System.IO;
using O10.Transactions.Core.Ledgers.Registry;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;


namespace O10.Transactions.Core.Serializers.Signed.Registry
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.Transient)]
    public class RegistryShortBlockSerializer : SignatureSupportSerializerBase<RegistryShortBlock>
    {
        public RegistryShortBlockSerializer(IServiceProvider serviceProvider) : base(serviceProvider, LedgerType.Registry, PacketTypes.Registry_ShortBlock)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            bw.Write((ushort)_block.WitnessStateKeys.Length);
            bw.Write((ushort)_block.WitnessUtxoKeys.Length);

            foreach (var item in _block.WitnessStateKeys)
            {
                bw.Write(item.PublicKey.Value.ToArray());
                bw.Write(item.Height);
            }

            foreach (var item in _block.WitnessUtxoKeys)
            {
                bw.Write(item.KeyImage.Value.ToArray());
            }
        }
    }
}
