using System;
using System.IO;
using O10.Transactions.Core.DataModel.Transactional;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;


namespace O10.Transactions.Core.Serializers.Signed.Transactional
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.Transient)]
    public class DocumentSignRecordSerializer : TransactionalSerializerBase<DocumentSignRecord>
    {
        public DocumentSignRecordSerializer(IServiceProvider serviceProvider) : base(serviceProvider, PacketType.Transactional, ActionTypes.Transaction_DocumentSignRecord)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            base.WriteBody(bw);

            bw.Write(_block.DocumentHash);
            bw.Write(_block.RecordHeight);
			bw.Write(_block.KeyImage);
			bw.Write(_block.SignerCommitment);
			WriteSurjectionProof(bw, _block.EligibilityProof);
            bw.Write(_block.Issuer);
            WriteSurjectionProof(bw, _block.SignerGroupRelationProof);
			bw.Write(_block.GroupIssuer);
			bw.Write(_block.SignerGroupCommitment);
			WriteSurjectionProof(bw, _block.SignerGroupProof);
			WriteSurjectionProof(bw, _block.SignerAllowedGroupsProof);
		}
	}
}
