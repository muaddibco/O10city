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
	public class BlindAssetParser : TransactionalBlockParserBase
    {
        public BlindAssetParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : 
            base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => PacketTypes.Transaction_BlindAsset;

        protected override Memory<byte> ParseTransactional(ushort version, Memory<byte> spanBody, out TransactionalPacketBase transactionalBlockBase)
        {
            BlindAsset block = null;

            if (version == 1)
            {
                int readBytes = 0;

                byte[] assetCommitment = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                byte[] mask = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                byte[] encryptedAssetId = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
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

                block = new BlindAsset
                {
                    EncryptedAsset = new Ledgers.O10State.Internal.EncryptedAsset
                    {
                        AssetCommitment = assetCommitment,
                        EcdhTuple = new O10.Core.Cryptography.EcdhTupleCA
                        {
                            Mask = mask,
                            AssetId = encryptedAssetId
                        }
                    },
                    SurjectionProof = new O10.Core.Cryptography.SurjectionProof
                    {
                        AssetCommitments = assetCommitments,
                        Rs = new O10.Core.Cryptography.BorromeanRingSignature
                        {
                            E = e,
                            S = s
                        }
                    }
                };

                transactionalBlockBase = block;
                return spanBody.Slice(readBytes);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
