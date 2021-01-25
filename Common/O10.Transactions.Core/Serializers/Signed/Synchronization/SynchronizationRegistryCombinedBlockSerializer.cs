using System;
using System.IO;
using O10.Transactions.Core.DataModel.Synchronization;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;


namespace O10.Transactions.Core.Serializers.Signed.Synchronization
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.Transient)]
    public class SynchronizationRegistryCombinedBlockSerializer : LinkedSerializerBase<SynchronizationRegistryCombinedBlock>
    {
        public SynchronizationRegistryCombinedBlockSerializer(IServiceProvider serviceProvider) : base(serviceProvider, PacketType.Synchronization, ActionTypes.Synchronization_RegistryCombinationBlock)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            base.WriteBody(bw);

            bw.Write(_block.ReportedTime.ToBinary());
            bw.Write((ushort)_block.BlockHashes.Length);
            foreach (byte[] blockHash in _block.BlockHashes)
            {
                bw.Write(blockHash);
            }
        }
    }
}
