using System;
using O10.Core;
using O10.Core.Identity;
using O10.Core.Models;
using O10.Transactions.Core.Ledgers.Synchronization;

namespace O10.Transactions.Core.Parsers
{
    public abstract class LinkedBlockParserBase : SignedBlockParserBase
    {
        public LinkedBlockParserBase(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) 
            : base(identityKeyProvidersRegistry)
        {
        }

        protected override Memory<byte> ParseSigned(ushort version, Memory<byte> spanBody, out SignedPacketBase signedPacketBase)
        {
            byte[] prevHash = spanBody.Slice(0, Globals.DEFAULT_HASH_SIZE).ToArray();
            Memory<byte> spanPostBody = ParseLinked(version, spanBody.Slice(Globals.DEFAULT_HASH_SIZE), out LinkedPacketBase linkedPacketBase);
            linkedPacketBase.HashPrev = prevHash;
            signedPacketBase = linkedPacketBase;

            return spanPostBody;
        }

        protected abstract Memory<byte> ParseLinked(ushort version, Memory<byte> spanBody, out LinkedPacketBase linkedPacketBase);
    }
}
