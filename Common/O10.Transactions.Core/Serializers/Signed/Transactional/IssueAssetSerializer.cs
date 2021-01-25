using System;
using System.IO;
using System.Text;
using O10.Transactions.Core.DataModel.Transactional;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;


namespace O10.Transactions.Core.Serializers.Signed.Transactional
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.Transient)]
    public class IssueAssetSerializer : TransactionalSerializerBase<IssueAsset>
    {
        public IssueAssetSerializer(IServiceProvider serviceProvider) : base(serviceProvider, PacketType.Transactional, ActionTypes.Transaction_IssueAsset)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            base.WriteBody(bw);

            bw.Write(_block.AssetIssuance.AssetId);
            byte strLen = (byte)_block.AssetIssuance.IssuedAssetInfo.Length;
            bw.Write(strLen);
            bw.Write(Encoding.ASCII.GetBytes(_block.AssetIssuance.IssuedAssetInfo.Substring(0, strLen)));
        }
    }
}
