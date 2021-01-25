using System;
using O10.Transactions.Core.DataModel.Transactional;
using O10.Core;
using O10.Core.Identity;

namespace O10.Transactions.Core.Parsers.Transactional
{
	public abstract class TransactionalTransitionalPacketParserBase : TransactionalBlockParserBase
	{
		public TransactionalTransitionalPacketParserBase(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
		{
		}

		protected override Memory<byte> ParseTransactional(ushort version, Memory<byte> spanBody, out TransactionalPacketBase transactionalBlockBase)
		{
			int readBytes = 0;

			byte[] destinationKey = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
			readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

			byte[] transactionPublicKey = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
			readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

			Memory<byte> spanPostBody = ParseTransactionalTransitional(version, spanBody.Slice(readBytes), out TransactionalTransitionalPacketBase transactionalTransitionalPacketBase);

			transactionalTransitionalPacketBase.DestinationKey = destinationKey;
			transactionalTransitionalPacketBase.TransactionPublicKey = transactionPublicKey;

			transactionalBlockBase = transactionalTransitionalPacketBase;

			return spanPostBody;
		}

		protected abstract Memory<byte> ParseTransactionalTransitional(ushort version, Memory<byte> spanBody, out TransactionalTransitionalPacketBase transactionalTransitionalPacketBase);
	}
}
