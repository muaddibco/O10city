using System;
using System.Buffers.Binary;
using O10.Core;
using O10.Core.Identity;
using O10.Core.Models;

namespace O10.Transactions.Core.Parsers
{
    public abstract class SignedBlockParserBase : BlockParserBase
    {
        protected readonly IIdentityKeyProvider _signerIdentityKeyProvider;

        public SignedBlockParserBase(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
        {
            _signerIdentityKeyProvider = identityKeyProvidersRegistry.GetInstance();
        }

        protected override PacketBase ParseBlockBase(ushort version, Memory<byte> spanBody, out Memory<byte> spanPostBody)
        {
            ulong blockHeight = BinaryPrimitives.ReadUInt64LittleEndian(spanBody.Span);

            spanPostBody = ParseSigned(version, spanBody.Slice(sizeof(ulong)), out SignedPacketBase signedBlockBase);

            signedBlockBase.Height = blockHeight;
            signedBlockBase.Signature = spanPostBody.Slice(0, Globals.SIGNATURE_SIZE);
            signedBlockBase.Source = _signerIdentityKeyProvider.GetKey(spanPostBody.Slice(Globals.SIGNATURE_SIZE, Globals.NODE_PUBLIC_KEY_SIZE));

            spanPostBody = spanPostBody.Slice(Globals.SIGNATURE_SIZE + Globals.NODE_PUBLIC_KEY_SIZE);
            return signedBlockBase;
        }

        protected override Memory<byte> GetBodyBytes(Memory<byte> span)
        {
            return span.Slice(0, span.Length - Globals.SIGNATURE_SIZE - Globals.NODE_PUBLIC_KEY_SIZE);
        }

        protected abstract Memory<byte> ParseSigned(ushort version, Memory<byte> spanBody, out SignedPacketBase signedBlockBase);
    }
}
