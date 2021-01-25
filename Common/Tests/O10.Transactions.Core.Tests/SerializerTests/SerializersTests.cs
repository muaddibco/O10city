using Chaos.NaCl;
using System;
using O10.Transactions.Core.DataModel.Synchronization;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Serializers.Signed.Synchronization;
using O10.Core.Cryptography;
using O10.Core.Identity;
using O10.Tests.Core;
using O10.Core.ExtensionMethods;
using Xunit;
using System.IO;
using O10.Transactions.Core.DataModel.Registry;
using O10.Transactions.Core.Serializers.Signed.Registry;
using System.Diagnostics;
using System.Linq;
using O10.Core;
using O10.Crypto.ConfidentialAssets;
using O10.Tests.Core.Fixtures;
using Xunit.Abstractions;

namespace O10.Transactions.Core.Tests.SerializerTests
{
    public class SerializersTests : BlockchainCoreTestBase
    {

        public SerializersTests(CoreFixture coreFixture, ITestOutputHelper testOutputHelper) : base(coreFixture, testOutputHelper)
        {

        }

        [Fact]
        public void SynchronizationConfirmedBlockSerializerTest()
        {
            ulong syncBlockHeight = 1;
            uint nonce = 4;
            byte[] powHash = BinaryHelper.GetPowHash(1234);
            ushort version = 1;
            ulong blockHeight = 9;
            byte[] prevHash = BinaryHelper.GetDefaultHash(1234);

            ushort round = 1;
            byte signersCount = 10;
            byte[] body = new byte[11 + Globals.NODE_PUBLIC_KEY_SIZE * signersCount + Globals.SIGNATURE_SIZE * signersCount];

            byte[][] expectedSignerPKs = new byte[signersCount][];
            byte[][] expectedSignerSignatures = new byte[signersCount][];

            DateTime expectedDateTime = DateTime.Now;


            for (int i = 0; i < signersCount; i++)
            {
                byte[] privateSignerKey = ConfidentialAssetsHelper.GetRandomSeed();

                Ed25519.KeyPairFromSeed(out byte[] publicSignerKey, out byte[] expandedSignerKey, privateSignerKey);

                expectedSignerPKs[i] = publicSignerKey;

                byte[] roundBytes = BitConverter.GetBytes(round);
                byte[] signerSignature = Ed25519.Sign(roundBytes, expandedSignerKey);

                expectedSignerSignatures[i] = signerSignature;
            }

            using (MemoryStream ms = new MemoryStream(body))
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(expectedDateTime.ToBinary());
                    bw.Write(round);
                    bw.Write(signersCount);

                    for (int i = 0; i < signersCount; i++)
                    {
                        bw.Write(expectedSignerPKs[i]);
                        bw.Write(expectedSignerSignatures[i]);
                    }
                }
            }

            byte[] expectedPacket = BinaryHelper.GetSignedPacket(
                PacketType.Synchronization,
                syncBlockHeight,
                nonce, powHash, version,
                ActionTypes.Synchronization_ConfirmedBlock, blockHeight, prevHash, body, _privateKey, out byte[] expectedSignature);

            SynchronizationConfirmedBlock block = new SynchronizationConfirmedBlock()
            {
                SyncBlockHeight = syncBlockHeight,
                BlockHeight = blockHeight,
                Nonce = nonce,
                PowHash = powHash,
                HashPrev = prevHash,
                ReportedTime = expectedDateTime,
                Round = round,
                PublicKeys = new byte[signersCount][],
                Signatures = new byte[signersCount][]
            };

            for (int i = 0; i < signersCount; i++)
            {
                block.PublicKeys[i] = expectedSignerPKs[i];
                block.Signatures[i] = expectedSignerSignatures[i];
            }

            using SynchronizationConfirmedBlockSerializer serializer = new SynchronizationConfirmedBlockSerializer(null);
            serializer.Initialize(block);
            serializer.SerializeBody();
            _signingService.Sign(block);

            byte[] actualPacket = serializer.GetBytes();

            Assert.Equal(expectedPacket, actualPacket);
        }

        [Fact]
        public void RegistryRegisterBlockSerializerTest()
        {
            ulong syncBlockHeight = 1;
            uint nonce = 4;
            byte[] powHash = BinaryHelper.GetPowHash(1234);
            ushort version = 1;
            ulong blockHeight = 9;
            byte[] prevHash = null;

            PacketType expectedReferencedPacketType = PacketType.Transactional;
            ushort expectedReferencedBlockType = ActionTypes.Transaction_IssueBlindedAsset;
            byte[] expectedReferencedBodyHash = BinaryHelper.GetDefaultHash(473826643);
            byte[] expectedTarget = BinaryHelper.GetDefaultHash(BinaryHelper.GetRandomPublicKey());

            byte[] body;

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write((ushort)expectedReferencedPacketType);
                    bw.Write(expectedReferencedBlockType);
                    bw.Write(expectedReferencedBodyHash);
                    bw.Write(expectedTarget);
                }

                body = ms.ToArray();
            }

            byte[] expectedPacket = BinaryHelper.GetSignedPacket(
                PacketType.Registry,
                syncBlockHeight,
                nonce, powHash, version,
                ActionTypes.Registry_Register, blockHeight, prevHash, body, _privateKey, out byte[] expectedSignature);

            RegistryRegisterBlock block = new RegistryRegisterBlock
            {
                SyncBlockHeight = syncBlockHeight,
                Nonce = nonce,
                PowHash = powHash,
                BlockHeight = blockHeight,
                ReferencedPacketType = expectedReferencedPacketType,
                ReferencedBlockType = expectedReferencedBlockType,
                ReferencedBodyHash = expectedReferencedBodyHash,
                ReferencedTarget = expectedTarget
            };

            using RegistryRegisterBlockSerializer serializer = new RegistryRegisterBlockSerializer(null);
            serializer.Initialize(block);
            serializer.SerializeBody();
            _signingService.Sign(block);

            byte[] actualPacket = serializer.GetBytes();

            Trace.WriteLine(expectedPacket.ToHexString());
            Trace.WriteLine(actualPacket.ToHexString());

            Assert.Equal(expectedPacket, actualPacket);
        }

		[Fact]
		public void RegistryRegisterBlockSerializerTransitionalTest()
		{
			ulong syncBlockHeight = 1;
			uint nonce = 4;
			byte[] powHash = BinaryHelper.GetPowHash(1234);
			ushort version = 1;
			ulong blockHeight = 9;
			byte[] prevHash = null;

			PacketType expectedReferencedPacketType = PacketType.Transactional;
			ushort expectedReferencedBlockType = ActionTypes.Transaction_transferAssetToStealth;
			byte[] expectedReferencedBodyHash = BinaryHelper.GetDefaultHash(473826643);
			byte[] expectedTarget = BinaryHelper.GetDefaultHash(BinaryHelper.GetRandomPublicKey());
			byte[] transactionKey = ConfidentialAssetsHelper.GetRandomSeed();

			byte[] body;

			using (MemoryStream ms = new MemoryStream())
			{
				using (BinaryWriter bw = new BinaryWriter(ms))
				{
					bw.Write((ushort)expectedReferencedPacketType);
					bw.Write(expectedReferencedBlockType);
					bw.Write(expectedReferencedBodyHash);
					bw.Write(expectedTarget);
					bw.Write(transactionKey);
				}

				body = ms.ToArray();
			}

			byte[] expectedPacket = BinaryHelper.GetSignedPacket(
				PacketType.Registry,
				syncBlockHeight,
				nonce, powHash, version,
				ActionTypes.Registry_Register, blockHeight, prevHash, body, _privateKey, out byte[] expectedSignature);

			RegistryRegisterBlock block = new RegistryRegisterBlock
			{
				SyncBlockHeight = syncBlockHeight,
				Nonce = nonce,
				PowHash = powHash,
				BlockHeight = blockHeight,
				ReferencedPacketType = expectedReferencedPacketType,
				ReferencedBlockType = expectedReferencedBlockType,
				ReferencedBodyHash = expectedReferencedBodyHash,
				ReferencedTarget = expectedTarget,
				ReferencedTransactionKey = transactionKey
			};

			using RegistryRegisterBlockSerializer serializer = new RegistryRegisterBlockSerializer(null);
			serializer.Initialize(block);
			serializer.SerializeBody();
			_signingService.Sign(block);

			byte[] actualPacket = serializer.GetBytes();

			Trace.WriteLine(expectedPacket.ToHexString());
			Trace.WriteLine(actualPacket.ToHexString());

			Assert.Equal(expectedPacket, actualPacket);
		}

		[Fact]
        public void RegistryRegisterStealthBlockSerializerTest()
        {
            ulong syncBlockHeight = 1;
            uint nonce = 4;
            byte[] powHash = BinaryHelper.GetPowHash(1234);
            ushort version = 1;
            byte[] keyImage = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] destinationKey = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] destinationKey2 = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] transactionPublicKey = ConfidentialAssetsHelper.GetRandomSeed();

            int ringSize = 3;
            int outputIndex = 0;

            byte[][] secretKeys = new byte[ringSize][];
            byte[][] pubkeys = new byte[ringSize][];

            PacketType expectedReferencedPacketType = PacketType.Transactional;
            ushort expectedReferencedBlockType = ActionTypes.Stealth_OnboardingRequest;
            byte[] expectedReferencedBodyHash = BinaryHelper.GetDefaultHash(473826643);
            byte[] expectedTarget = BinaryHelper.GetDefaultHash(BinaryHelper.GetRandomPublicKey());

            byte[] body;

            for (int i = 0; i < ringSize; i++)
            {
                pubkeys[i] = BinaryHelper.GetRandomPublicKey(out secretKeys[i]);
            }

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write((ushort)expectedReferencedPacketType);
                    bw.Write(expectedReferencedBlockType);
                    bw.Write(expectedReferencedBodyHash);
                }

                body = ms.ToArray();
            }

            byte[] expectedPacket = BinaryHelper.GetStealthPacket(
                PacketType.Registry, syncBlockHeight, nonce, powHash, version,
                ActionTypes.Registry_RegisterStealth, keyImage, 
                destinationKey, destinationKey2, transactionPublicKey, body, pubkeys, secretKeys[outputIndex], outputIndex, out RingSignature[] expectedSignatures);

            RegistryRegisterStealth block = new RegistryRegisterStealth
            {
                SyncBlockHeight = syncBlockHeight,
                Nonce = nonce,
                PowHash = powHash,
                KeyImage = new Key32(keyImage),
                DestinationKey = destinationKey,
				DestinationKey2 = destinationKey2,
                TransactionPublicKey = transactionPublicKey,
                ReferencedPacketType = expectedReferencedPacketType,
                ReferencedBlockType = expectedReferencedBlockType,
                ReferencedBodyHash = expectedReferencedBodyHash,
                PublicKeys = pubkeys.Select(p => new Key32(p)).ToArray(),
                Signatures = expectedSignatures
            };

            using RegistryRegisterStealthBlockSerializer serializer = new RegistryRegisterStealthBlockSerializer(null);
            serializer.Initialize(block);

            byte[] actualPacket = serializer.GetBytes();

            Trace.WriteLine(expectedPacket.ToHexString());
            Trace.WriteLine(actualPacket.ToHexString());

            Assert.Equal(expectedPacket, actualPacket);
        }

        [Fact]
        public void RegistryShortBlockSerializerTest()
        {
            ulong syncBlockHeight = 1;
            uint nonce = 4;
            byte[] powHash = BinaryHelper.GetPowHash(1234);
            ushort version = 1;
            ulong blockHeight = 9;
            byte[] prevHash = null;

            byte[] body;

            ushort expectedCount = 10;

            Random random = new Random(); 

            WitnessStateKey[] witnessStateKeys = new WitnessStateKey[expectedCount];
            WitnessUtxoKey[] witnessUtxoKeys = new WitnessUtxoKey[expectedCount];
            for (ushort i = 0; i < expectedCount; i++)
            {
                witnessStateKeys[i] = new WitnessStateKey { PublicKey = new Key32(ConfidentialAssetsHelper.GetRandomSeed()), Height = (ulong)random.Next(0, int.MaxValue)};
                witnessUtxoKeys[i] = new WitnessUtxoKey { KeyImage = new Key32(ConfidentialAssetsHelper.GetRandomSeed()) };
            }
            
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write((ushort)witnessStateKeys.Length);
                    bw.Write((ushort)witnessUtxoKeys.Length);

                    foreach (WitnessStateKey witnessStateKey in witnessStateKeys)
                    {
                        bw.Write(witnessStateKey.PublicKey.Value.ToArray());
                        bw.Write(witnessStateKey.Height);
                    }

                    foreach (WitnessUtxoKey witnessUtxoKey in witnessUtxoKeys)
                    {
                        bw.Write(witnessUtxoKey.KeyImage.Value.ToArray());
                    }
                }

                body = ms.ToArray();
            }

            byte[] expectedPacket = BinaryHelper.GetSignedPacket(
                PacketType.Registry,
                syncBlockHeight,
                nonce, powHash, version,
                ActionTypes.Registry_ShortBlock, blockHeight, prevHash, body, _privateKey, out byte[] expectedSignature);

            RegistryShortBlock block = new RegistryShortBlock
            {
                SyncBlockHeight = syncBlockHeight,
                Nonce = nonce,
                PowHash = powHash,
                BlockHeight = blockHeight,
                WitnessStateKeys = witnessStateKeys,
                WitnessUtxoKeys = witnessUtxoKeys
            };

            using RegistryShortBlockSerializer serializer = new RegistryShortBlockSerializer(null);
            serializer.Initialize(block);
            serializer.SerializeBody();
            _signingService.Sign(block);

            byte[] actualPacket = serializer.GetBytes();

            Trace.WriteLine(expectedPacket.ToHexString());
            Trace.WriteLine(actualPacket.ToHexString());

            Assert.Equal(expectedPacket, actualPacket);
        }

        [Fact]
        public void RegistryFullBlockSerializerTest()
        {
            ulong syncBlockHeight = 1;
            uint nonce = 4;
            byte[] powHash = BinaryHelper.GetPowHash(1234);
            ushort version = 1;
            ulong blockHeight = 9;
            byte[] prevHash = null;

            PacketType expectedReferencedPacketType = PacketType.Transactional;
            ushort expectedReferencedBlockType = ActionTypes.Transaction_IssueBlindedAsset;
            byte[] expectedReferencedBodyHash = BinaryHelper.GetDefaultHash(473826643);
            byte[] expectedTarget = BinaryHelper.GetDefaultHash(BinaryHelper.GetRandomPublicKey());

            byte[] body;

            ushort expectedCount = 1000;

            RegistryRegisterBlock[] stateWitnesses = new RegistryRegisterBlock[expectedCount];
            RegistryRegisterStealth[] utxoWitnesses = new RegistryRegisterStealth[expectedCount];
            for (ushort i = 0; i < expectedCount; i++)
            {
                RegistryRegisterBlock registryRegisterBlock = new RegistryRegisterBlock
                {
                    SyncBlockHeight = syncBlockHeight,
                    Nonce = nonce + i,
                    PowHash = BinaryHelper.GetPowHash(1234 + i),
                    BlockHeight = blockHeight,
                    ReferencedPacketType = expectedReferencedPacketType,
                    ReferencedBlockType = expectedReferencedBlockType,
                    ReferencedBodyHash = BinaryHelper.GetDefaultHash(473826643 + i),
                    ReferencedTarget = BinaryHelper.GetDefaultHash(BinaryHelper.GetRandomPublicKey())
                };

                using RegistryRegisterBlockSerializer serializer1 = new RegistryRegisterBlockSerializer(null);
                serializer1.Initialize(registryRegisterBlock);
                serializer1.SerializeBody();
                _signingService.Sign(registryRegisterBlock);
                serializer1.SerializeFully();

                stateWitnesses[i] = registryRegisterBlock;

                RegistryRegisterStealth registryRegisterStealthBlock = new RegistryRegisterStealth
                {
                    SyncBlockHeight = syncBlockHeight,
                    Nonce = nonce + i,
                    PowHash = BinaryHelper.GetPowHash(1234 + i),
                    DestinationKey = ConfidentialAssetsHelper.GetRandomSeed(),
                    ReferencedPacketType = expectedReferencedPacketType,
                    ReferencedBlockType = expectedReferencedBlockType,
					DestinationKey2 = ConfidentialAssetsHelper.GetRandomSeed(),
                    ReferencedBodyHash = BinaryHelper.GetDefaultHash(473826643 + i),
                    KeyImage = new Key32(ConfidentialAssetsHelper.GetRandomSeed()),
                    TransactionPublicKey = ConfidentialAssetsHelper.GetRandomSeed(),
                    PublicKeys = new IKey[] { new Key32(ConfidentialAssetsHelper.GetRandomSeed()), new Key32(ConfidentialAssetsHelper.GetRandomSeed()) , new Key32(ConfidentialAssetsHelper.GetRandomSeed()) },
                    Signatures = new RingSignature[] 
                    {
                        new RingSignature { C = ConfidentialAssetsHelper.GetRandomSeed(), R = ConfidentialAssetsHelper.GetRandomSeed() },
                        new RingSignature { C = ConfidentialAssetsHelper.GetRandomSeed(), R = ConfidentialAssetsHelper.GetRandomSeed() },
                        new RingSignature { C = ConfidentialAssetsHelper.GetRandomSeed(), R = ConfidentialAssetsHelper.GetRandomSeed() }
                    }
                };

                using RegistryRegisterStealthBlockSerializer serializer2 = new RegistryRegisterStealthBlockSerializer(null);
                serializer2.Initialize(registryRegisterStealthBlock);
                serializer2.SerializeFully();

                utxoWitnesses[i] = registryRegisterStealthBlock;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write((ushort)stateWitnesses.Length);
                    bw.Write((ushort)utxoWitnesses.Length);

                    foreach (RegistryRegisterBlock witness in stateWitnesses)
                    {
                        bw.Write(witness.RawData.ToArray());
                    }

                    foreach (RegistryRegisterStealth witness in utxoWitnesses)
                    {
                        bw.Write(witness.RawData.ToArray());
                    }

                    bw.Write(BinaryHelper.GetDefaultHash(1111));
                }

                body = ms.ToArray();
            }

            byte[] expectedPacket = BinaryHelper.GetSignedPacket(
                PacketType.Registry,
                syncBlockHeight,
                nonce, powHash, version,
                ActionTypes.Registry_FullBlock, blockHeight, prevHash, body, _privateKey, out byte[] expectedSignature);

            RegistryFullBlock block = new RegistryFullBlock
            {
                SyncBlockHeight = syncBlockHeight,
                Nonce = nonce,
                PowHash = powHash,
                BlockHeight = blockHeight,
                StateWitnesses = stateWitnesses,
                UtxoWitnesses = utxoWitnesses,
                ShortBlockHash = BinaryHelper.GetDefaultHash(1111)
            };

            using RegistryFullBlockSerializer serializer = new RegistryFullBlockSerializer(null);
            serializer.Initialize(block);
            serializer.SerializeBody();
            _signingService.Sign(block);

            byte[] actualPacket = serializer.GetBytes();

            Trace.WriteLine(expectedPacket.ToHexString());
            Trace.WriteLine(actualPacket.ToHexString());

            Assert.Equal(expectedPacket, actualPacket);
        }

        [Fact]
        public void RegistryConfidenceBlockSerializerTest()
        {
            ulong syncBlockHeight = 1;
            uint nonce = 4;
            byte[] powHash = BinaryHelper.GetPowHash(1234);
            ushort version = 1;
            ulong blockHeight = 9;
            byte[] prevHash = null;

            Random randNum = new Random();
            ushort bitMaskLength = 375;
            byte[] bitMask = Enumerable.Repeat(0, bitMaskLength).Select(i => (byte)randNum.Next(0, 255)).ToArray();
            byte[] expectedProof = Enumerable.Repeat(0, 16).Select(i => (byte)randNum.Next(0, 255)).ToArray();
            byte[] expectedReferencedBodyHash = BinaryHelper.GetDefaultHash(473826643);

            byte[] body;

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write((ushort)bitMask.Length);
                    bw.Write(bitMask);
                    bw.Write(expectedProof);
                    bw.Write(expectedReferencedBodyHash);
                }

                body = ms.ToArray();
            }

            byte[] expectedPacket = BinaryHelper.GetSignedPacket(
                PacketType.Registry,
                syncBlockHeight,
                nonce, powHash, version,
                ActionTypes.Registry_ConfidenceBlock, blockHeight, prevHash, body, _privateKey, out byte[] expectedSignature);

            RegistryConfidenceBlock block = new RegistryConfidenceBlock
            {
                SyncBlockHeight = syncBlockHeight,
                Nonce = nonce,
                PowHash = powHash,
                BlockHeight = blockHeight,
                BitMask = bitMask,
                ConfidenceProof = expectedProof,
                ReferencedBlockHash = expectedReferencedBodyHash
            };

            using RegistryConfidenceBlockSerializer serializer = new RegistryConfidenceBlockSerializer(null);
            serializer.Initialize(block);
            serializer.SerializeBody();
            _signingService.Sign(block);

            byte[] actualPacket = serializer.GetBytes();

            Trace.WriteLine(expectedPacket.ToHexString());
            Trace.WriteLine(actualPacket.ToHexString());

            Assert.Equal(expectedPacket, actualPacket);
        }

        [Fact]
        public void SynchronizationRegistryCombinedBlockSerializerTest()
        {
            ulong syncBlockHeight = 1;
            uint nonce = 4;
            byte[] powHash = BinaryHelper.GetPowHash(1234);
            ushort version = 1;
            ulong blockHeight = 9;
            byte[] prevHash = BinaryHelper.GetDefaultHash(1234);

            byte[] body;

            DateTime expectedDateTime = DateTime.Now;
            byte[][] expectedHashes = new byte[2][] { ConfidentialAssetsHelper.GetRandomSeed(), ConfidentialAssetsHelper.GetRandomSeed() };

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(expectedDateTime.ToBinary());
                    bw.Write((ushort)2);
                    bw.Write(expectedHashes[0]);
                    bw.Write(expectedHashes[1]);
                }

                body = ms.ToArray();
            }

            byte[] expectedPacket = BinaryHelper.GetSignedPacket(
                PacketType.Synchronization,
                syncBlockHeight,
                nonce, powHash, version,
                ActionTypes.Synchronization_RegistryCombinationBlock, blockHeight, prevHash, body, _privateKey, out byte[] expectedSignature);

            SynchronizationRegistryCombinedBlock block = new SynchronizationRegistryCombinedBlock
            {
                SyncBlockHeight = syncBlockHeight,
                Nonce = nonce,
                PowHash = powHash,
                BlockHeight = blockHeight,
                HashPrev = prevHash,
                ReportedTime = expectedDateTime,
                BlockHashes = expectedHashes
            };

            using SynchronizationRegistryCombinedBlockSerializer serializer = new SynchronizationRegistryCombinedBlockSerializer(null);
            serializer.Initialize(block);
            serializer.SerializeBody();
            _signingService.Sign(block);

            byte[] actualPacket = serializer.GetBytes();

            Trace.WriteLine(expectedPacket.ToHexString());
            Trace.WriteLine(actualPacket.ToHexString());

            Assert.Equal(expectedPacket, actualPacket);
        }
    }
}
