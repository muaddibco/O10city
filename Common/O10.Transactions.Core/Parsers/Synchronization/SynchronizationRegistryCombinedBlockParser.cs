using System;
using System.Buffers.Binary;
using O10.Transactions.Core.DataModel.Synchronization;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Exceptions;
using O10.Core;
using O10.Core.Architecture;

using O10.Core.Identity;

namespace O10.Transactions.Core.Parsers.Synchronization
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationRegistryCombinedBlockParser : SynchronizationBlockParserBase
    {
        public SynchronizationRegistryCombinedBlockParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) 
            : base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => ActionTypes.Synchronization_RegistryCombinationBlock;

        protected override Memory<byte> ParseSynchronization(ushort version, Memory<byte> spanBody, out SynchronizationBlockBase synchronizationBlockBase)
        {
            SynchronizationRegistryCombinedBlock block = new SynchronizationRegistryCombinedBlock();

            if(version == 1)
            {
                ushort blockHashesCount = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span);
                block.BlockHashes = new byte[blockHashesCount][];
                for (int i = 0; i < blockHashesCount; i++)
                {
                    block.BlockHashes[i] = spanBody.Slice(2 + i * Globals.DEFAULT_HASH_SIZE, Globals.DEFAULT_HASH_SIZE).ToArray();
                }

                synchronizationBlockBase = block;

                return spanBody.Slice(2 + blockHashesCount * Globals.DEFAULT_HASH_SIZE);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
