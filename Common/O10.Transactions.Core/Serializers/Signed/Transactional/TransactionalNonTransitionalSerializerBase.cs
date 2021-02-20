using System;
using System.IO;
using O10.Transactions.Core.Ledgers.O10State;
using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Serializers.Signed.Transactional
{
	public abstract class TransactionalNonTransitionalSerializerBase<T> : TransactionalSerializerBase<T> where T : TransactionalNonTransitionalPacketBase
	{
		public TransactionalNonTransitionalSerializerBase(IServiceProvider serviceProvider, LedgerType ledgerType, ushort blockType) 
			: base(serviceProvider, packetType, blockType)
		{
		}
		protected override void WriteBody(BinaryWriter bw)
		{
			base.WriteBody(bw);

			bw.Write(_block.Target);
		}
	}
}
