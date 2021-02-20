using System;
using System.Buffers.Binary;
using O10.Transactions.Core.Ledgers.O10State.Internal;
using O10.Transactions.Core.Enums;
using O10.Core;
using O10.Core.Cryptography;
using O10.Core.Identity;
using O10.Core.Models;

namespace O10.Transactions.Core.Parsers
{
    public abstract class BlockParserBase : IBlockParser
    {
        protected readonly IIdentityKeyProvider _entityIdentityKeyProvider;

        public BlockParserBase(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry)
        {
            _entityIdentityKeyProvider = identityKeyProvidersRegistry.GetInstance();
        }

        public abstract ushort BlockType { get; }

        public abstract LedgerType LedgerType { get; }

        public virtual PacketBase Parse(Memory<byte> source)
        {
            Memory<byte> spanBody = SliceInitialBytes(source, out Memory<byte> spanHeader);

            PacketBase blockBase = ParseBody(spanBody, out Memory<byte> nonHeaderBytes);
			blockBase.BodyBytes = GetBodyBytes(source.Slice(0, spanHeader.Length + nonHeaderBytes.Length));

			blockBase.RawData = source.Slice(0, spanHeader.Length + nonHeaderBytes.Length);

            FillBlockBaseHeader(blockBase, spanHeader);

            return blockBase;
        }

        private PacketBase ParseBody(Memory<byte> spanBody, out Memory<byte> nonHeaderBytes)
        {
            ushort version = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span);
            ushort messageType = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span.Slice(2));
            PacketBase blockBase = ParseBlockBase(version, spanBody.Slice(4), out Memory<byte> spanPostBody);

            nonHeaderBytes = spanBody.Slice(0, spanBody.Length - spanPostBody.Length);

            return blockBase;
        }

        protected virtual Memory<byte> GetBodyBytes(Memory<byte> spanBody)
        {
            return spanBody;
        }

        protected virtual Memory<byte> SliceInitialBytes(Memory<byte> span, out Memory<byte> spanHeader)
        {
            spanHeader = span.Slice(0, 2 + Globals.SYNC_BLOCK_HEIGHT_LENGTH + Globals.NONCE_LENGTH + Globals.POW_HASH_SIZE);

            return span.Slice(2 + Globals.SYNC_BLOCK_HEIGHT_LENGTH + Globals.NONCE_LENGTH + Globals.POW_HASH_SIZE);
        }

        protected abstract PacketBase ParseBlockBase(ushort version, Memory<byte> spanBody, out Memory<byte> spanPostBody);

        protected virtual Memory<byte> FillBlockBaseHeader(PacketBase blockBase, Memory<byte> spanHeader)
        {
            LedgerType ledgerType = (LedgerType)BinaryPrimitives.ReadUInt16LittleEndian(spanHeader.Span);
            int readBytes = sizeof(ushort);

            blockBase.SyncHeight = BinaryPrimitives.ReadUInt64LittleEndian(spanHeader.Slice(readBytes).Span);
            readBytes += sizeof(ulong);

            blockBase.Nonce = BinaryPrimitives.ReadUInt32LittleEndian(spanHeader.Slice(readBytes).Span);
            readBytes += sizeof(uint);

            blockBase.PowHash = spanHeader.Slice(readBytes, Globals.POW_HASH_SIZE).ToArray();
            readBytes += Globals.POW_HASH_SIZE;

            return spanHeader.Slice(readBytes);
        }

        public static void GetPacketAndBlockTypes(Memory<byte> source, out LedgerType ledgerType, out ushort blockType)
        {
            int blockTypePos = Globals.PACKET_TYPE_LENGTH + Globals.SYNC_BLOCK_HEIGHT_LENGTH + Globals.NONCE_LENGTH + Globals.POW_HASH_SIZE + Globals.VERSION_LENGTH;
            ledgerType = (LedgerType)BinaryPrimitives.ReadUInt16LittleEndian(source.Span);
            blockType = BinaryPrimitives.ReadUInt16LittleEndian(source.Span.Slice(blockTypePos));
        }

        protected static void ReadCommitment(ref Memory<byte> spanBody, ref int readBytes, out byte[] assetCommitment)
        {
            assetCommitment = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
            readBytes += Globals.NODE_PUBLIC_KEY_SIZE;
        }

        protected static void ReadSurjectionProof(ref Memory<byte> spanBody, ref int readBytes, out SurjectionProof surjectionProof)
        {
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

            surjectionProof = new SurjectionProof
            {
                AssetCommitments = assetCommitments,
                Rs = new BorromeanRingSignature
                {
                    E = e,
                    S = s
                }
            };
        }

        protected static void ReadInversedSurjectionProof(ref Memory<byte> span, ref int readBytes, out InversedSurjectionProof surjectionProof)
        {
            byte[] assetCommitment = span.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
            readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

            byte[] e = span.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
            readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

            ushort assetCommitmentsLength = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(readBytes).Span);
            readBytes += sizeof(ushort);

            byte[][] s = new byte[assetCommitmentsLength][];

            for (int i = 0; i < assetCommitmentsLength; i++)
            {
                s[i] = span.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytes += Globals.NODE_PUBLIC_KEY_SIZE;
            }

            BorromeanRingSignature rs = new BorromeanRingSignature
            {
                E = e,
                S = s
            };

            surjectionProof = new InversedSurjectionProof
            {
                AssetCommitment = assetCommitment,
                Rs = rs
            };
        }

        protected static void ReadBorromeanRingSignature(ref Memory<byte> spanBody, ref int readBytes, out BorromeanRingSignature borromeanRingSignature)
        {
            byte[] e = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
            readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

            ushort sCount = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span.Slice(readBytes));
            readBytes += 2;

            byte[][] s = new byte[sCount][];
            for (int i = 0; i < sCount; i++)
            {
                s[i] = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytes += Globals.NODE_PUBLIC_KEY_SIZE;
            }

            borromeanRingSignature = new BorromeanRingSignature { E = e, S = s };
        }

        protected static void ReadEcdhTupleCA(ref Memory<byte> spanBody, ref int readBytes, out EcdhTupleCA ecdhTuple)
        {
            byte[] mask = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
            readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

            byte[] assetId = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
            readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

            ecdhTuple = new EcdhTupleCA
            {
                Mask = mask,
                AssetId = assetId
            };
        }

        protected static void ReadEcdhTupleIP(ref Memory<byte> spanBody, ref int readBytes, out EcdhTupleIP ecdhTuple)
        {
            byte[] issuer = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
            readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

            byte[] payload = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
            readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

            ecdhTuple = new EcdhTupleIP
            {
                Issuer = issuer,
                Payload = payload
            };
        }

        protected static void ReadEcdhTupleProofs(ref Memory<byte> spanBody, ref int readBytes, out EcdhTupleProofs ecdhTuple)
        {
            byte[] mask = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
            readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

            byte[] assetId = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
            readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

            byte[] assetIssuer = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
            readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

            byte[] payload = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
            readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

            ecdhTuple = new EcdhTupleProofs
            {
                Mask = mask,
                AssetId = assetId,
                AssetIssuer = assetIssuer,
                Payload = payload
            };
        }
    }
}
