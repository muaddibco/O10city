using System;
using System.Buffers.Binary;
using O10.Transactions.Core.DataModel.Registry;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Exceptions;
using O10.Core;
using O10.Core.Architecture;

using O10.Core.Identity;
using O10.Core.Models;

namespace O10.Transactions.Core.Parsers.Registry
{
	[RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class RegistryFullBlockParser : SignedBlockParserBase
    {
        private readonly RegistryRegisterBlockParser _registryRegisterBlockParser;
        private readonly RegistryRegisterStealthBlockParser _registryRegisterStealthBlockParser;

        public RegistryFullBlockParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
        {
            _registryRegisterBlockParser = new RegistryRegisterBlockParser(identityKeyProvidersRegistry);
            _registryRegisterStealthBlockParser = new RegistryRegisterStealthBlockParser(identityKeyProvidersRegistry);
        }

        public override ushort BlockType => ActionTypes.Registry_FullBlock;

        public override PacketType PacketType => PacketType.Registry;

        protected override Memory<byte> ParseSigned(ushort version, Memory<byte> spanBody, out SignedPacketBase syncedBlockBase)
        {
            if (version == 1)
            {
                int readBytes = 0;

                RegistryFullBlock transactionsFullBlock = new RegistryFullBlock();
                ushort stateWitnessesCount = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span.Slice(readBytes));
                readBytes += sizeof(ushort);

                ushort utxoWitnessesCount = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span.Slice(readBytes));
                readBytes += sizeof(ushort);

                transactionsFullBlock.StateWitnesses = new RegistryRegisterBlock[stateWitnessesCount];
                transactionsFullBlock.UtxoWitnesses = new RegistryRegisterStealth[utxoWitnessesCount];

                for (int i = 0; i < stateWitnessesCount; i++)
                {
                    RegistryRegisterBlock block = (RegistryRegisterBlock)_registryRegisterBlockParser.Parse(spanBody.Slice(readBytes));

                    readBytes += block?.RawData.Length ?? 0;

                    transactionsFullBlock.StateWitnesses[i] = block;
                }

                for (int i = 0; i < utxoWitnessesCount; i++)
                {
                    RegistryRegisterStealth block = (RegistryRegisterStealth)_registryRegisterStealthBlockParser.Parse(spanBody.Slice(readBytes));

                    readBytes += block?.RawData.Length ?? 0;

                    transactionsFullBlock.UtxoWitnesses[i] = block;
                }

                transactionsFullBlock.ShortBlockHash = spanBody.Slice(readBytes, Globals.DEFAULT_HASH_SIZE).ToArray();
                readBytes += Globals.DEFAULT_HASH_SIZE;

                syncedBlockBase = transactionsFullBlock;

                return spanBody.Slice(readBytes);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
