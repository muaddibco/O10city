using System;
using System.Buffers.Binary;
using O10.Transactions.Core.DataModel.Registry;
using O10.Transactions.Core.DataModel.Stealth;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Exceptions;
using O10.Transactions.Core.Parsers.Stealth;
using O10.Core;
using O10.Core.Architecture;

using O10.Core.Identity;

namespace O10.Transactions.Core.Parsers.Registry
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class RegistryRegisterStealthBlockParser : StealthParserBase
    {
        public RegistryRegisterStealthBlockParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
        {
        }

        public override PacketType PacketType => PacketType.Registry;

        public override ushort BlockType => ActionTypes.Registry_RegisterStealth;

        protected override Memory<byte> ParseStealth(ushort version, Memory<byte> spanBody, out StealthBase StealthBase)
        {
            if(version == 1)
            {
                int readBytes = 0;

                PacketType referencedPacketType = (PacketType)BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span);
                readBytes += 2;

                ushort referencedBlockType = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span.Slice(readBytes));
                readBytes += 2;

				byte[] referencedBlockHash = spanBody.Slice(readBytes, Globals.DEFAULT_HASH_SIZE).ToArray();
                readBytes += Globals.DEFAULT_HASH_SIZE;

                RegistryRegisterStealth registryRegisterStealthBlock = new RegistryRegisterStealth
                {
                    ReferencedPacketType = referencedPacketType,
                    ReferencedBlockType = referencedBlockType,
                    ReferencedBodyHash = referencedBlockHash,
                };

                StealthBase = registryRegisterStealthBlock;

                return spanBody.Slice(readBytes);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
