using System;
using System.IO;
using O10.Transactions.Core.Ledgers.Synchronization;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;


namespace O10.Transactions.Core.Serializers.Signed.Synchronization
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.Transient)]
    public class SynchronizationConfirmedBlockSerializer : LinkedSerializerBase<SynchronizationConfirmedBlock>
    {
        public SynchronizationConfirmedBlockSerializer(IServiceProvider serviceProvider) : base(serviceProvider, LedgerType.Synchronization, PacketTypes.Synchronization_ConfirmedBlock)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            base.WriteBody(bw);

            bw.Write(_block.ReportedTime.ToBinary());
            bw.Write(_block.Round);
            byte signersCount = (byte)(_block.PublicKeys?.Length ?? 0);
            bw.Write(signersCount);
            for (int i = 0; i < signersCount; i++)
            {
                bw.Write(_block.PublicKeys[i]);
                bw.Write(_block.Signatures[i]);
            }
        }
    }
}
