using System;
using System.IO;
using O10.Transactions.Core.DataModel.Transactional;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;


namespace O10.Transactions.Core.Serializers.Signed.Transactional
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.Transient)]
    public class DocumentRecordSerializer : TransactionalSerializerBase<DocumentRecord>
    {
        public DocumentRecordSerializer(IServiceProvider serviceProvider) : base(serviceProvider, PacketType.Transactional, ActionTypes.Transaction_DocumentRecord)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            base.WriteBody(bw);

            bw.Write(_block.DocumentHash);
			bw.Write((ushort)_block.AllowedSignerGroupCommitments.Length);

			for (int i = 0; i < _block.AllowedSignerGroupCommitments.Length; i++)
			{
				bw.Write(_block.AllowedSignerGroupCommitments[i]);
			}
		}
	}
}
