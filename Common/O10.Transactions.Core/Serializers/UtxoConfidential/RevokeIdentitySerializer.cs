using System;
using System.IO;
using O10.Transactions.Core.Ledgers.Stealth;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;


namespace O10.Transactions.Core.Serializers.Stealth
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.Transient)]
    public class RevokeIdentitySerializer : StealthTransactionSerializerBase<RevokeIdentity>
    {
        public RevokeIdentitySerializer(IServiceProvider serviceProvider) 
			: base(serviceProvider, LedgerType.Stealth, PacketTypes.Stealth_RevokeIdentity)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
			base.WriteBody(bw);

			WriteSurjectionProof(bw, _block.OwnershipProof);
			WriteSurjectionProof(bw, _block.EligibilityProof);
        }
    }
}
