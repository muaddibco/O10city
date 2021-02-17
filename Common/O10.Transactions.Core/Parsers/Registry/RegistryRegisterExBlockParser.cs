using O10.Core.Architecture;
using O10.Core.ExtensionMethods;
using O10.Core.Identity;
using O10.Core.Models;
using O10.Transactions.Core.Ledgers.Registry;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Exceptions;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace O10.Transactions.Core.Parsers.Registry
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class RegistryRegisterExBlockParser : SignedBlockParserBase
    {
        public RegistryRegisterExBlockParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) 
            : base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => PacketTypes.Registry_RegisterEx;

        public override LedgerType PacketType => LedgerType.Registry;

        protected override Memory<byte> ParseSigned(ushort version, Memory<byte> spanBody, out SignedPacketBase syncedBlockBase)
        {
            if (version == 1)
            {
                int readBytes = 0;

                LedgerType referencedPacketType = (LedgerType)BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span);
                readBytes += sizeof(ushort);

                int paramsLength = BinaryPrimitives.ReadInt32LittleEndian(spanBody.Span[readBytes..]);
                readBytes += sizeof(int);

                byte[] paramBytes = spanBody.Slice(readBytes, paramsLength).ToArray();
                readBytes += paramsLength;


                RegistryRegisterExBlock transactionRegisterBlock = new RegistryRegisterExBlock
                {
                    ReferencedPacketType = referencedPacketType,
                    Parameters = (Dictionary<string, string>)paramBytes.DeSerialize()
                };

                syncedBlockBase = transactionRegisterBlock;

                return spanBody[readBytes..];
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
