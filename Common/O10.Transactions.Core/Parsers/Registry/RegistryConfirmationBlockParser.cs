using System;
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
    public class RegistryConfirmationBlockParser : SignedBlockParserBase
    {
        public RegistryConfirmationBlockParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) 
            : base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => PacketTypes.Registry_ConfirmationBlock;

        public override LedgerType LedgerType => LedgerType.Registry;

        protected override Memory<byte> ParseSigned(ushort version, Memory<byte> spanBody, out SignedPacketBase syncedBlockBase)
        {
            if(version == 1)
            {
                RegistryConfirmationBlock block = new RegistryConfirmationBlock
                {
                    ReferencedBlockHash = spanBody.Slice(0, Globals.DEFAULT_HASH_SIZE).ToArray()
                };

                syncedBlockBase = block;

                return spanBody.Slice(Globals.DEFAULT_HASH_SIZE);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
