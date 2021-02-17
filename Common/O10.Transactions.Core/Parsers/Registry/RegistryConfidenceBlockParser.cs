using System;
using System.Buffers.Binary;
using O10.Transactions.Core.Ledgers.Registry;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Exceptions;
using O10.Core;
using O10.Core.Architecture;

using O10.Core.Identity;
using O10.Core.Models;

namespace O10.Transactions.Core.Parsers.Registry
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class RegistryConfidenceBlockParser : SignedBlockParserBase
    {
        public RegistryConfidenceBlockParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => PacketTypes.Registry_ConfidenceBlock;

        public override LedgerType PacketType => LedgerType.Registry;

        protected override Memory<byte> ParseSigned(ushort version, Memory<byte> spanBody, out SignedPacketBase syncedBlockBase)
        {
            if (version == 1)
            {
                RegistryConfidenceBlock registryConfidenceBlock = new RegistryConfidenceBlock();
                ushort bitMaskLength = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span);
                registryConfidenceBlock.BitMask = spanBody.Slice(2, bitMaskLength).ToArray();
                registryConfidenceBlock.ConfidenceProof = spanBody.Slice(2 + bitMaskLength, Globals.TRANSACTION_KEY_HASH_SIZE).ToArray();
                registryConfidenceBlock.ReferencedBlockHash = spanBody.Slice(2 + bitMaskLength + Globals.TRANSACTION_KEY_HASH_SIZE, Globals.DEFAULT_HASH_SIZE).ToArray();

                syncedBlockBase = registryConfidenceBlock;

                return spanBody.Slice(2 + bitMaskLength + Globals.TRANSACTION_KEY_HASH_SIZE + Globals.DEFAULT_HASH_SIZE);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
