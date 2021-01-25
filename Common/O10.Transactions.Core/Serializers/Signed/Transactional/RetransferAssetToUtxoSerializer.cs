using System;
using System.IO;
using O10.Transactions.Core.DataModel.Transactional;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;


namespace O10.Transactions.Core.Serializers.Signed.Transactional
{
	[RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.Transient)]
	public class RetransferAssetToStealthSerializer : TransactionalTransitionalSerializerBase<TransferAssetToStealth>
    {
        public RetransferAssetToStealthSerializer(IServiceProvider serviceProvider) : base(serviceProvider, PacketType.Transactional, ActionTypes.Transaction_RetransferAssetToStealth)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            base.WriteBody(bw);

            bw.Write(_block.TransferredAsset.AssetCommitment);
            bw.Write(_block.TransferredAsset.EcdhTuple.Mask);
            bw.Write(_block.TransferredAsset.EcdhTuple.AssetId);
            bw.Write((ushort)_block.SurjectionProof.AssetCommitments.Length);

            for (int i = 0; i < _block.SurjectionProof.AssetCommitments.Length; i++)
            {
                bw.Write(_block.SurjectionProof.AssetCommitments[i]);
            }

            bw.Write(_block.SurjectionProof.Rs.E);

            for (int i = 0; i < _block.SurjectionProof.Rs.S.Length; i++)
            {
                bw.Write(_block.SurjectionProof.Rs.S[i]);
            }
        }
    }
}
