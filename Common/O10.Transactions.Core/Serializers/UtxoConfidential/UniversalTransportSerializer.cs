using System;
using System.IO;
using O10.Transactions.Core.DataModel.Stealth;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;


namespace O10.Transactions.Core.Serializers.Stealth
{
	[RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.Transient)]
    public class UniversalTransportSerializer : StealthTransactionSerializerBase<UniversalTransport>
    {
        public UniversalTransportSerializer(IServiceProvider serviceProvider) : base(serviceProvider, PacketType.Stealth, ActionTypes.Stealth_UniversalTransport)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            base.WriteBody(bw);

            WriteSurjectionProof(bw, _block.OwnershipProof);
            WriteCommitment(bw, _block.MessageHash.Span);
        }
    }
}
