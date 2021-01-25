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
    public class SynchronizationConfirmedBlockParser : SynchronizationBlockParserBase
    {
        public SynchronizationConfirmedBlockParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) 
            : base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => ActionTypes.Synchronization_ConfirmedBlock;

        protected override Memory<byte> ParseSynchronization(ushort version, Memory<byte> spanBody, out SynchronizationBlockBase synchronizationBlockBase)
        {
            if(version == 1)
            {
                ushort round = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span);
                byte numberOfSigners = spanBody.Span[2];
                byte[][] signers = new byte[numberOfSigners][];
                byte[][] signatures = new byte[numberOfSigners][];

                for (int i = 0; i < numberOfSigners; i++)
                {
                    signers[i] = spanBody.Slice(3 + (Globals.NODE_PUBLIC_KEY_SIZE + Globals.SIGNATURE_SIZE)* i, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                    signatures[i] = spanBody.Slice(3 + (Globals.NODE_PUBLIC_KEY_SIZE + Globals.SIGNATURE_SIZE )* i + Globals.NODE_PUBLIC_KEY_SIZE, Globals.SIGNATURE_SIZE).ToArray();
                }

                SynchronizationBlockBase synchronizationConfirmedBlock = new SynchronizationConfirmedBlock()
                {
                    Round = round,
                    PublicKeys = signers,
                    Signatures = signatures
                };

                synchronizationBlockBase = synchronizationConfirmedBlock;

                return spanBody.Slice(3 + (Globals.NODE_PUBLIC_KEY_SIZE + Globals.SIGNATURE_SIZE) * numberOfSigners);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
