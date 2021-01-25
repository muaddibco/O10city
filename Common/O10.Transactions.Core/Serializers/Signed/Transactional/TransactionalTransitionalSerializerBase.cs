using System;
using System.IO;
using O10.Transactions.Core.DataModel.Transactional;
using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Serializers.Signed.Transactional
{
	public abstract class TransactionalTransitionalSerializerBase<T> : TransactionalSerializerBase<T> where T : TransactionalTransitionalPacketBase
	{
		public TransactionalTransitionalSerializerBase(IServiceProvider serviceProvider, PacketType packetType, ushort blockType) 
			: base(serviceProvider, packetType, blockType)
		{
		}

		protected override void WriteBody(BinaryWriter bw)
		{
			base.WriteBody(bw);

			bw.Write(_block.DestinationKey);
			bw.Write(_block.TransactionPublicKey);
		}
	}
}
