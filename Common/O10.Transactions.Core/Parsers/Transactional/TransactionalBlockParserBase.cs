using System;
using O10.Transactions.Core.Ledgers.O10State;
using O10.Transactions.Core.Enums;
using O10.Core.Identity;
using System.Buffers.Binary;
using O10.Core.Models;

namespace O10.Transactions.Core.Parsers.Transactional
{
    public abstract class TransactionalBlockParserBase : SignedBlockParserBase
    {
        public TransactionalBlockParserBase(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) 
            : base(identityKeyProvidersRegistry)
        {
        }

        public override LedgerType PacketType => LedgerType.O10State;

        protected override Memory<byte> ParseSigned(ushort version, Memory<byte> spanBody, out SignedPacketBase syncedBlockBase)
        {
            int readBytes = 0;
            ulong funds = BinaryPrimitives.ReadUInt64LittleEndian(spanBody.Span.Slice(readBytes));
            readBytes += sizeof(ulong);

            Memory<byte> spanPostBody = ParseTransactional(version, spanBody.Slice(readBytes), out TransactionalPacketBase transactionalBlockBase);
            transactionalBlockBase.UptodateFunds = funds;
            syncedBlockBase = transactionalBlockBase;

            return spanPostBody;
        }

        protected abstract Memory<byte> ParseTransactional(ushort version, Memory<byte> spanBody, out TransactionalPacketBase transactionalBlockBase);
    }
}
