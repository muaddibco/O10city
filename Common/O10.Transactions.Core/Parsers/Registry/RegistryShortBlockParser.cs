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
    public class RegistryShortBlockParser : SignedBlockParserBase
    {
        private readonly IIdentityKeyProvider _keyProvider;
        public RegistryShortBlockParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
        {
            _keyProvider = identityKeyProvidersRegistry.GetInstance();
        }

        public override ushort BlockType => PacketTypes.Registry_ShortBlock;

        public override LedgerType LedgerType => LedgerType.Registry;

        protected override Memory<byte> ParseSigned(ushort version, Memory<byte> spanBody, out SignedPacketBase syncedBlockBase)
        {
            if (version == 1)
            {
                int readBytes = 0;
                RegistryShortBlock transactionsShortBlock = new RegistryShortBlock();

                ushort witnessStateKeysCount = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span.Slice(readBytes));
                readBytes += sizeof(ushort);

                ushort witnessUtxoKeysCount = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span.Slice(readBytes));
                readBytes += sizeof(ushort);

                transactionsShortBlock.WitnessStateKeys = new WitnessStateKey[witnessStateKeysCount];
                transactionsShortBlock.WitnessUtxoKeys = new WitnessUtxoKey[witnessUtxoKeysCount];

                for (int i = 0; i < witnessStateKeysCount; i++)
                {
                    byte[] witnessPublicKey = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                    readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                    ulong witnessHeight = BinaryPrimitives.ReadUInt64LittleEndian(spanBody.Slice(readBytes).Span);
                    readBytes += sizeof(ulong);

                    IKey key = _keyProvider.GetKey(witnessPublicKey);

                    transactionsShortBlock.WitnessStateKeys[i] = new WitnessStateKey { PublicKey = key, Height = witnessHeight};
                }

                for (int i = 0; i < witnessUtxoKeysCount; i++)
                {
                    byte[] witnessKeyImage = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                    readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                    IKey key = _keyProvider.GetKey(witnessKeyImage);

                    transactionsShortBlock.WitnessUtxoKeys[i] = new WitnessUtxoKey { KeyImage = key };
                }

                syncedBlockBase = transactionsShortBlock;

                return spanBody.Slice(readBytes);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
