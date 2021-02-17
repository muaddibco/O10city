using System;
using System.Buffers.Binary;
using O10.Transactions.Core.Ledgers.O10State;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Exceptions;
using O10.Core;
using O10.Core.Architecture;

using O10.Core.Identity;

namespace O10.Transactions.Core.Parsers.Transactional
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
	public class RetransferAssetToStealthParser : TransactionalTransitionalPacketParserBase
	{
        public RetransferAssetToStealthParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : 
            base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => PacketTypes.Transaction_RetransferAssetToStealth;

        protected override Memory<byte> ParseTransactionalTransitional(ushort version, Memory<byte> spanBody, out TransactionalTransitionalPacketBase transactionalBlockBase)
        {
            RetransferAssetToStealth block = null;

            if(version == 1)
            {
                int readBytes = 0;

                byte[] assetCommitment = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                byte[] assetId = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                byte[] mask = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                ushort assetCommitmentsCount = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Slice(readBytes).Span);
                readBytes += sizeof(ushort);

                byte[][] assetCommitments = new byte[assetCommitmentsCount][];

                for (int i = 0; i < assetCommitmentsCount; i++)
                {
                    assetCommitments[i] = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                    readBytes += Globals.NODE_PUBLIC_KEY_SIZE;
                }

                byte[] e = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                byte[][] s = new byte[assetCommitmentsCount][];

                for (int i = 0; i < assetCommitmentsCount; i++)
                {
                    s[i] = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                    readBytes += Globals.NODE_PUBLIC_KEY_SIZE;
                }

                transactionalBlockBase = block;
                return spanBody.Slice(readBytes);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
