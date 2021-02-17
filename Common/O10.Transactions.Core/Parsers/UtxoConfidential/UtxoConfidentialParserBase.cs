using System;
using System.Buffers.Binary;
using O10.Transactions.Core.Ledgers.Stealth;
using O10.Transactions.Core.Enums;
using O10.Core;
using O10.Core.Cryptography;
using O10.Core.Identity;
using O10.Core.Models;

namespace O10.Transactions.Core.Parsers.Stealth
{
    public abstract class StealthParserBase : BlockParserBase
    {
        public StealthParserBase(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
        {
        }

        public override LedgerType PacketType => LedgerType.Stealth;

        protected override PacketBase ParseBlockBase(ushort version, Memory<byte> spanBody, out Memory<byte> spanPostBody)
        {
            int readBytes = 0;

			ReadCommitment(ref spanBody, ref readBytes, out byte[] destinationKey);
			ReadCommitment(ref spanBody, ref readBytes, out byte[] destinationKey2);
			ReadCommitment(ref spanBody, ref readBytes, out byte[] transactionPublicKey);

			spanPostBody = ParseStealth(version, spanBody.Slice(readBytes), out StealthBase StealthBase);

            ushort readBytesPostBody = 0;
            Memory<byte> keyImage = spanPostBody.Slice(readBytesPostBody, Globals.NODE_PUBLIC_KEY_SIZE);
            readBytesPostBody += Globals.NODE_PUBLIC_KEY_SIZE;

            ushort ringSignaturesCount = BinaryPrimitives.ReadUInt16LittleEndian(spanPostBody.Span.Slice(readBytesPostBody));
            readBytesPostBody += sizeof(ushort);

            StealthBase.KeyImage = _entityIdentityKeyProvider.GetKey(keyImage);
            StealthBase.DestinationKey = destinationKey;
			StealthBase.DestinationKey2 = destinationKey2;
            StealthBase.TransactionPublicKey = transactionPublicKey;
            StealthBase.PublicKeys = new IKey[ringSignaturesCount];
            StealthBase.Signatures = new RingSignature[ringSignaturesCount];

            for (int i = 0; i < ringSignaturesCount; i++)
            {
                byte[] publicKey = spanPostBody.Slice(readBytesPostBody, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                IKey key = _entityIdentityKeyProvider.GetKey(spanPostBody.Slice(readBytesPostBody, Globals.NODE_PUBLIC_KEY_SIZE));
                StealthBase.PublicKeys[i] = key;
                readBytesPostBody += Globals.NODE_PUBLIC_KEY_SIZE;
            }

            for (int i = 0; i < ringSignaturesCount; i++)
            {
                byte[] c = spanPostBody.Slice(readBytesPostBody, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytesPostBody += Globals.NODE_PUBLIC_KEY_SIZE;

                byte[] r = spanPostBody.Slice(readBytesPostBody, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytesPostBody += Globals.NODE_PUBLIC_KEY_SIZE;

                RingSignature ringSignature = new RingSignature { C = c, R = r };
                StealthBase.Signatures[i] = ringSignature;
            }

            spanPostBody = spanPostBody.Slice(readBytesPostBody);

            return StealthBase;
        }

        protected abstract Memory<byte> ParseStealth(ushort version, Memory<byte> spanBody, out StealthBase StealthBase);
    }
}
