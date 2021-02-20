using System;
using System.IO;
using O10.Transactions.Core.Ledgers.O10State;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;


namespace O10.Transactions.Core.Serializers.Signed.Transactional
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.Transient)]
    public class IssueBlindedAssetSerializer : TransactionalSerializerBase<IssueBlindedAsset>
	{
		public IssueBlindedAssetSerializer(IServiceProvider serviceProvider) : base(serviceProvider, LedgerType.O10State, PacketTypes.Transaction_IssueBlindedAsset)
		{
		}

		protected override void WriteBody(BinaryWriter bw)
		{
			base.WriteBody(bw);

			bw.Write(_block.AssetCommitment);
            bw.Write(_block.KeyImage);
			bw.Write(_block.UniquencessProof.C);
			bw.Write(_block.UniquencessProof.R);
		}
	}
}
