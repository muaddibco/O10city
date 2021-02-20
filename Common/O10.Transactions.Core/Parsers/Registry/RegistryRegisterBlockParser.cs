using System;
using System.Buffers.Binary;
using O10.Transactions.Core.Ledgers.Registry;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;

using O10.Core.Identity;
using O10.Transactions.Core.Exceptions;
using O10.Core.Models;
using O10.Core;

namespace O10.Transactions.Core.Parsers.Registry
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class RegistryRegisterBlockParser : SignedBlockParserBase
    {
        public RegistryRegisterBlockParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) 
            : base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => PacketTypes.Registry_Register;

        public override LedgerType LedgerType => LedgerType.Registry;

        protected override Memory<byte> ParseSigned(ushort version, Memory<byte> spanBody, out SignedPacketBase syncedBlockBase)
        {
            if (version == 1)
            {
				int readBytes = 0;

                LedgerType referencedLedgerType = (LedgerType)BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span);
				readBytes += sizeof(ushort);

                ushort referencedBlockType = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span.Slice(readBytes));
				readBytes += sizeof(ushort);

                byte[] referencedBlockHash = spanBody.Slice(readBytes, Globals.DEFAULT_HASH_SIZE).ToArray();
				readBytes += Globals.DEFAULT_HASH_SIZE;

				byte[] referencedTarget = spanBody.Slice(readBytes, Globals.DEFAULT_HASH_SIZE).ToArray();
				readBytes += Globals.DEFAULT_HASH_SIZE;

				byte[] transactionKey = null;

				if((referencedBlockType & PacketTypes.TransitionalFlag) == PacketTypes.TransitionalFlag)
				{
					transactionKey = spanBody.Slice(readBytes, Globals.DEFAULT_HASH_SIZE).ToArray();
					readBytes += Globals.DEFAULT_HASH_SIZE;
				}

				RegistryRegisterBlock transactionRegisterBlock = new RegistryRegisterBlock
				{
					ReferencedLedgerType = referencedLedgerType,
					ReferencedBlockType = referencedBlockType,
					ReferencedBodyHash = referencedBlockHash,
					ReferencedTarget = referencedTarget,
					ReferencedTransactionKey = transactionKey
                };

                syncedBlockBase = transactionRegisterBlock;

                return spanBody.Slice(readBytes);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
