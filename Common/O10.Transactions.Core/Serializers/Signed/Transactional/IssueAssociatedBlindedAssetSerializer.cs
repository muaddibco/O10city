using System;
using System.IO;
using O10.Transactions.Core.DataModel.Transactional;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;


namespace O10.Transactions.Core.Serializers.Signed.Transactional
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.Transient)]
    public class IssueAssociatedBlindedAssetSerializer : TransactionalSerializerBase<IssueAssociatedBlindedAsset>
	{
		public IssueAssociatedBlindedAssetSerializer(IServiceProvider serviceProvider) : base(serviceProvider, PacketType.Transactional, ActionTypes.Transaction_IssueAssociatedBlindedAsset)
		{
		}

		protected override void WriteBody(BinaryWriter bw)
		{
			base.WriteBody(bw);

			bw.Write(_block.GroupId);
			bw.Write(_block.AssetCommitment);
            bw.Write(_block.RootAssetCommitment);
		}
	}
}
