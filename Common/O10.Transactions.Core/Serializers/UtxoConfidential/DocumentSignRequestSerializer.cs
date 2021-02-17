using System;
using System.IO;
using O10.Transactions.Core.Ledgers.Stealth;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;


namespace O10.Transactions.Core.Serializers.Stealth
{
	[RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.Transient)]
    public class DocumentSignRequestSerializer : StealthTransactionSerializerBase<DocumentSignRequest>
    {
        public DocumentSignRequestSerializer(IServiceProvider serviceProvider) 
			: base(serviceProvider, LedgerType.Stealth, PacketTypes.Stealth_DocumentSignRequest)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
			base.WriteBody(bw);

			WriteEcdhTupleProofs(bw, _block.EcdhTuple);
            WriteSurjectionProof(bw, _block.OwnershipProof);
            WriteSurjectionProof(bw, _block.EligibilityProof);
            WriteSurjectionProof(bw, _block.SignerGroupRelationProof);
            //WriteSurjectionProof(bw, _block.SignerGroupNameSurjectionProof);
            WriteCommitment(bw, _block.AllowedGroupCommitment);
            WriteSurjectionProof(bw, _block.AllowedGroupNameSurjectionProof);
        }
    }
}
