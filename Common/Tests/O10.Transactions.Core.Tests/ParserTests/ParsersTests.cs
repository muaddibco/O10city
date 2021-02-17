using Chaos.NaCl;
using System;
using System.IO;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Parsers.Synchronization;
using O10.Core.Identity;
using O10.Core.ExtensionMethods;
using O10.Tests.Core;
using Xunit;
using NSubstitute;
using O10.Transactions.Core.Ledgers.Synchronization;
using O10.Transactions.Core.Parsers.Registry;
using O10.Transactions.Core.Ledgers.Registry;
using O10.Core.Cryptography;
using O10.Transactions.Core.Serializers.Signed.Registry;
using System.Linq;
using O10.Transactions.Core.Parsers;
using O10.Crypto.ConfidentialAssets;
using O10.Tests.Core.Fixtures;
using Xunit.Abstractions;

namespace O10.Transactions.Core.Tests.ParserTests
{
	public class ParsersTests : BlockchainCoreTestBase
	{
		public ParsersTests(CoreFixture coreFixture, ITestOutputHelper testOutputHelper) : base(coreFixture, testOutputHelper)
		{
		}

		[Fact]
		public void SynchronizationConfirmedBlockParserTest()
		{
			byte signersCount = 10;
			byte[] body;
			ushort round = 1;
			ulong syncBlockHeight = 1;
			uint nonce = 4;
			ushort version = 1;
			ulong blockHeight = 9;

			byte[][] expectedSignerPKs = new byte[signersCount][];
			byte[][] expectedSignerSignatures = new byte[signersCount][];

			DateTime expectedDateTime = DateTime.Now;

			byte[] powHash = BinaryHelper.GetPowHash(1234);
			byte[] prevHash = BinaryHelper.GetDefaultHash(1234);

			for (int i = 0; i < signersCount; i++)
			{
				byte[] privateSignerKey = ConfidentialAssetsHelper.GetRandomSeed();
				Ed25519.KeyPairFromSeed(out byte[] publicSignerKey, out byte[] expandedSignerKey, privateSignerKey);

				expectedSignerPKs[i] = publicSignerKey;

				byte[] roundBytes = BitConverter.GetBytes(round);

				byte[] signerSignature = Ed25519.Sign(roundBytes, expandedSignerKey);

				expectedSignerSignatures[i] = signerSignature;
			}

			using (MemoryStream ms = new MemoryStream())
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

				body = ms.ToArray();
			}

			byte[] packet = BinaryHelper.GetSignedPacket(
				LedgerType.Synchronization,
				syncBlockHeight,
				nonce, powHash, version,
				PacketTypes.Synchronization_ConfirmedBlock, blockHeight, prevHash, body, _privateKey, out byte[] expectedSignature);
			string packetExpectedString = packet.ToHexString();

			SynchronizationConfirmedBlockParser synchronizationConfirmedBlockParser = new SynchronizationConfirmedBlockParser(_identityKeyProvidersRegistry);
			SynchronizationConfirmedBlock block = (SynchronizationConfirmedBlock)synchronizationConfirmedBlockParser.Parse(packet);

			string packetActualString = block.RawData.ToHexString();
			Assert.Equal(syncBlockHeight, block.SyncHeight);
			Assert.Equal(nonce, block.Nonce);
			Assert.Equal(powHash, block.PowHash);
			Assert.Equal(version, block.Version);
			Assert.Equal(blockHeight, block.Height);
			Assert.Equal(prevHash, block.HashPrev);
			Assert.Equal(expectedDateTime, block.ReportedTime);
			Assert.Equal(round, block.Round);

			for (int i = 0; i < signersCount; i++)
			{
				Assert.Equal(expectedSignerPKs[i], block.PublicKeys[i]);
				Assert.Equal(expectedSignerSignatures[i], block.Signatures[i]);
			}

			Assert.Equal(_publicKey, block.Signer.Value.ToArray());
			Assert.Equal(expectedSignature, block.Signature.ToArray());
		}

		[Fact]
		public void RegistryRegisterBlockParserTest()
		{
			ulong syncBlockHeight = 1;
			uint nonce = 4;
			byte[] powHash = BinaryHelper.GetPowHash(1234);
			ushort version = 1;
			ulong blockHeight = 9;
			byte[] prevHash = null;

			LedgerType expectedReferencedPacketType = LedgerType.O10State;
			ushort expectedReferencedBlockType = PacketTypes.Transaction_IssueBlindedAsset;
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

			byte[] packet = BinaryHelper.GetSignedPacket(
				LedgerType.Registry,
				syncBlockHeight,
				nonce, powHash, version,
				PacketTypes.Registry_Register, blockHeight, prevHash, body, _privateKey, out byte[] expectedSignature);

			RegistryRegisterBlockParser registryRegisterBlockParser = new RegistryRegisterBlockParser(_identityKeyProvidersRegistry);
			RegistryRegisterBlock block = (RegistryRegisterBlock)registryRegisterBlockParser.Parse(packet);

			Assert.Equal(syncBlockHeight, block.SyncHeight);
			Assert.Equal(nonce, block.Nonce);
			Assert.Equal(powHash, block.PowHash);
			Assert.Equal(version, block.Version);
			Assert.Equal(blockHeight, block.Height);

			Assert.Equal(expectedReferencedPacketType, block.ReferencedPacketType);
			Assert.Equal(expectedReferencedBlockType, block.ReferencedBlockType);
			Assert.Equal(expectedReferencedBodyHash, block.ReferencedBodyHash);
			Assert.Equal(expectedTarget, block.ReferencedTarget);

			Assert.Equal(_publicKey, block.Signer.Value.ToArray());
			Assert.Equal(expectedSignature, block.Signature.ToArray());
		}

		[Fact]
		public void RegistryRegisterBlockParserTransitionalTest()
		{
			ulong syncBlockHeight = 1;
			uint nonce = 4;
			byte[] powHash = BinaryHelper.GetPowHash(1234);
			ushort version = 1;
			ulong blockHeight = 9;
			byte[] prevHash = null;

			LedgerType expectedReferencedPacketType = LedgerType.O10State;
			ushort expectedReferencedBlockType = PacketTypes.Transaction_transferAssetToStealth;
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

			byte[] packet = BinaryHelper.GetSignedPacket(
				LedgerType.Registry,
				syncBlockHeight,
				nonce, powHash, version,
				PacketTypes.Registry_Register, blockHeight, prevHash, body, _privateKey, out byte[] expectedSignature);

			RegistryRegisterBlockParser registryRegisterBlockParser = new RegistryRegisterBlockParser(_identityKeyProvidersRegistry);
			RegistryRegisterBlock block = (RegistryRegisterBlock)registryRegisterBlockParser.Parse(packet);

			Assert.Equal(syncBlockHeight, block.SyncHeight);
			Assert.Equal(nonce, block.Nonce);
			Assert.Equal(powHash, block.PowHash);
			Assert.Equal(version, block.Version);
			Assert.Equal(blockHeight, block.Height);

			Assert.Equal(expectedReferencedPacketType, block.ReferencedPacketType);
			Assert.Equal(expectedReferencedBlockType, block.ReferencedBlockType);
			Assert.Equal(expectedReferencedBodyHash, block.ReferencedBodyHash);
			Assert.Equal(expectedTarget, block.ReferencedTarget);
			Assert.Equal(transactionKey, block.ReferencedTransactionKey);

			Assert.Equal(_publicKey, block.Signer.Value.ToArray());
			Assert.Equal(expectedSignature, block.Signature.ToArray());
		}
		[Fact]
		public void RegistryRegisterStealthBlockParserTest()
		{
			ulong syncBlockHeight = 1;
			uint nonce = 4;
			byte[] powHash = BinaryHelper.GetPowHash(1234);
			ushort version = 1;
			byte[] keyImage = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] destinationKey = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] transactionPublicKey = ConfidentialAssetsHelper.GetRandomSeed();

			LedgerType expectedReferencedPacketType = LedgerType.O10State;
			ushort expectedReferencedBlockType = PacketTypes.Stealth_OnboardingRequest;
			byte[] destinationKey2 = ConfidentialAssetsHelper.GetRandomSeed();
			byte[] expectedReferencedBodyHash = BinaryHelper.GetDefaultHash(473826643);

			int ringSize = 3;
			int outputIndex = 0;
			byte[][] secretKeys = new byte[ringSize][];
			byte[][] pubkeys = new byte[ringSize][];

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

			byte[] packet = BinaryHelper.GetStealthPacket(
				LedgerType.Registry, syncBlockHeight, nonce, powHash, version,
				PacketTypes.Registry_RegisterStealth, keyImage, destinationKey, destinationKey2, transactionPublicKey, body, pubkeys, secretKeys[outputIndex], outputIndex, out RingSignature[] expectedSignatures);

			RegistryRegisterStealthBlockParser registryRegisterBlockParser = new RegistryRegisterStealthBlockParser(_identityKeyProvidersRegistry);
			RegistryRegisterStealth block = (RegistryRegisterStealth)registryRegisterBlockParser.Parse(packet);

			Assert.Equal(syncBlockHeight, block.SyncHeight);
			Assert.Equal(nonce, block.Nonce);
			Assert.Equal(powHash, block.PowHash);
			Assert.Equal(version, block.Version);

			Assert.Equal(expectedReferencedPacketType, block.ReferencedPacketType);
			Assert.Equal(expectedReferencedBlockType, block.ReferencedBlockType);
			Assert.Equal(destinationKey2, block.DestinationKey2);
			Assert.Equal(expectedReferencedBodyHash, block.ReferencedBodyHash);
			Assert.Equal(new Key32(keyImage), block.KeyImage);
			Assert.Equal(destinationKey, block.DestinationKey);
			Assert.Equal(transactionPublicKey, block.TransactionPublicKey);
		}

		[Fact]
		public void RegistryShortBlockParserTest()
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
				witnessStateKeys[i] = new WitnessStateKey { PublicKey = new Key32(ConfidentialAssetsHelper.GetRandomSeed()), Height = (ulong)random.Next(0, int.MaxValue) };
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

			byte[] packet = BinaryHelper.GetSignedPacket(
				LedgerType.Registry,
				syncBlockHeight,
				nonce, powHash, version,
				PacketTypes.Registry_ShortBlock, blockHeight, prevHash, body, _privateKey, out byte[] expectedSignature);

			RegistryShortBlockParser registryFullBlockParser = new RegistryShortBlockParser(_identityKeyProvidersRegistry);
			RegistryShortBlock block = (RegistryShortBlock)registryFullBlockParser.Parse(packet);

			Assert.Equal(syncBlockHeight, block.SyncHeight);
			Assert.Equal(nonce, block.Nonce);
			Assert.Equal(powHash, block.PowHash);
			Assert.Equal(version, block.Version);
			Assert.Equal(blockHeight, block.Height);

			for (int i = 0; i < witnessStateKeys.Length; i++)
			{
				Assert.Equal(witnessStateKeys[i].PublicKey, block.WitnessStateKeys[i].PublicKey);
				Assert.Equal(witnessStateKeys[i].Height, block.WitnessStateKeys[i].Height);
			}

			for (int i = 0; i < witnessUtxoKeys.Length; i++)
			{
				Assert.Equal(witnessUtxoKeys[i].KeyImage, block.WitnessUtxoKeys[i].KeyImage);
			}

			Assert.Equal(_publicKey, block.Signer.Value.ToArray());
			Assert.Equal(expectedSignature, block.Signature.ToArray());
		}

		[Fact]
		public void RegistryFullBlockParserTest()
		{
			IBlockParser blockParser = new RegistryRegisterBlockParser(_identityKeyProvidersRegistry);
			_blockParsersRepository.GetInstance(0).ReturnsForAnyArgs(blockParser);

			ulong syncBlockHeight = 1;
			uint nonce = 4;
			byte[] powHash = BinaryHelper.GetPowHash(1234);
			ushort version = 1;
			ulong blockHeight = 9;
			byte[] prevHash = null;

			LedgerType expectedReferencedPacketType = LedgerType.O10State;
			ushort expectedReferencedBlockType = PacketTypes.Transaction_IssueBlindedAsset;

			byte[] body;

			ushort expectedCount = 1;
			byte[] expectedShortBlockHash;

			RegistryRegisterBlock[] stateWitnesses = new RegistryRegisterBlock[expectedCount];
			RegistryRegisterStealth[] utxoWitnesses = new RegistryRegisterStealth[expectedCount];

			for (ushort i = 0; i < expectedCount; i++)
			{
				RegistryRegisterBlock registryRegisterBlock = new RegistryRegisterBlock
				{
					SyncHeight = syncBlockHeight,
					Nonce = nonce + i,
					PowHash = BinaryHelper.GetPowHash(1234 + i),
					Height = blockHeight,
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
					SyncHeight = syncBlockHeight,
					Nonce = nonce + i,
					PowHash = BinaryHelper.GetPowHash(1234 + i),
					KeyImage = new Key32(ConfidentialAssetsHelper.GetRandomSeed()),
					ReferencedPacketType = expectedReferencedPacketType,
					ReferencedBlockType = expectedReferencedBlockType,
					ReferencedBodyHash = BinaryHelper.GetDefaultHash(473826643 + i),
					DestinationKey = ConfidentialAssetsHelper.GetRandomSeed(),
					DestinationKey2 = ConfidentialAssetsHelper.GetRandomSeed(),
					TransactionPublicKey = ConfidentialAssetsHelper.GetRandomSeed(),
					PublicKeys = new Key32[] { new Key32(ConfidentialAssetsHelper.GetRandomSeed()), new Key32(ConfidentialAssetsHelper.GetRandomSeed()), new Key32(ConfidentialAssetsHelper.GetRandomSeed()) },
					Signatures = new RingSignature[]
					{
						new RingSignature { R = ConfidentialAssetsHelper.GetRandomSeed(), C = ConfidentialAssetsHelper.GetRandomSeed() },
						new RingSignature { R = ConfidentialAssetsHelper.GetRandomSeed(), C = ConfidentialAssetsHelper.GetRandomSeed() },
						new RingSignature { R = ConfidentialAssetsHelper.GetRandomSeed(), C = ConfidentialAssetsHelper.GetRandomSeed() }
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

					expectedShortBlockHash = BinaryHelper.GetDefaultHash(1111);
					bw.Write(expectedShortBlockHash);
				}

				body = ms.ToArray();
			}

			byte[] packet = BinaryHelper.GetSignedPacket(
				LedgerType.Registry,
				syncBlockHeight,
				nonce, powHash, version,
				PacketTypes.Registry_FullBlock, blockHeight, prevHash, body, _privateKey, out byte[] expectedSignature);

			RegistryFullBlockParser registryFullBlockParser = new RegistryFullBlockParser(_identityKeyProvidersRegistry);
			RegistryFullBlock block = (RegistryFullBlock)registryFullBlockParser.Parse(packet);

			Assert.Equal(syncBlockHeight, block.SyncHeight);
			Assert.Equal(nonce, block.Nonce);
			Assert.Equal(powHash, block.PowHash);
			Assert.Equal(version, block.Version);
			Assert.Equal(blockHeight, block.Height);

			for (int i = 0; i < expectedCount; i++)
			{
				RegistryRegisterBlock registryRegisterBlock = block.StateWitnesses[i];
				Assert.Equal(stateWitnesses[i].LedgerType, registryRegisterBlock.LedgerType);
				Assert.Equal(stateWitnesses[i].SyncHeight, registryRegisterBlock.SyncHeight);
				Assert.Equal(stateWitnesses[i].Nonce, registryRegisterBlock.Nonce);
				Assert.Equal(stateWitnesses[i].PowHash, registryRegisterBlock.PowHash);
				Assert.Equal(stateWitnesses[i].Height, registryRegisterBlock.Height);
				Assert.Equal(stateWitnesses[i].PacketType, registryRegisterBlock.PacketType);
				Assert.Equal(stateWitnesses[i].ReferencedPacketType, registryRegisterBlock.ReferencedPacketType);
				Assert.Equal(stateWitnesses[i].ReferencedBlockType, registryRegisterBlock.ReferencedBlockType);
				Assert.Equal(stateWitnesses[i].ReferencedBodyHash, registryRegisterBlock.ReferencedBodyHash);
				Assert.Equal(stateWitnesses[i].ReferencedTarget, registryRegisterBlock.ReferencedTarget);
				Assert.Equal(stateWitnesses[i].Signature.ToArray(), registryRegisterBlock.Signature.ToArray());
				Assert.Equal(stateWitnesses[i].Signer, registryRegisterBlock.Signer);

				RegistryRegisterStealth registryRegisterStealthBlock = block.StealthWitnesses[i];
				Assert.Equal(utxoWitnesses[i].LedgerType, registryRegisterStealthBlock.LedgerType);
				Assert.Equal(utxoWitnesses[i].SyncHeight, registryRegisterStealthBlock.SyncHeight);
				Assert.Equal(utxoWitnesses[i].Nonce, registryRegisterStealthBlock.Nonce);
				Assert.Equal(utxoWitnesses[i].PowHash, registryRegisterStealthBlock.PowHash);
				Assert.Equal(utxoWitnesses[i].KeyImage, registryRegisterStealthBlock.KeyImage);
				Assert.Equal(utxoWitnesses[i].PacketType, registryRegisterStealthBlock.PacketType);
				Assert.Equal(utxoWitnesses[i].ReferencedPacketType, registryRegisterStealthBlock.ReferencedPacketType);
				Assert.Equal(utxoWitnesses[i].ReferencedBlockType, registryRegisterStealthBlock.ReferencedBlockType);
				Assert.Equal(utxoWitnesses[i].DestinationKey2, registryRegisterStealthBlock.DestinationKey2);
				Assert.Equal(utxoWitnesses[i].ReferencedBodyHash, registryRegisterStealthBlock.ReferencedBodyHash);
				Assert.Equal(utxoWitnesses[i].DestinationKey, registryRegisterStealthBlock.DestinationKey);
			}

			Assert.Equal(expectedShortBlockHash, block.ShortBlockHash);

			Assert.Equal(_publicKey, block.Signer.Value.ToArray());
			Assert.Equal(expectedSignature, block.Signature.ToArray());
		}

		[Fact]
		public void RegistryConfidenceBlockParserTest()
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

			byte[] packet = BinaryHelper.GetSignedPacket(
				LedgerType.Registry,
				syncBlockHeight,
				nonce, powHash, version,
				PacketTypes.Registry_ConfidenceBlock, blockHeight, prevHash, body, _privateKey, out byte[] expectedSignature);

			RegistryConfidenceBlockParser registryFullBlockParser = new RegistryConfidenceBlockParser(_identityKeyProvidersRegistry);
			RegistryConfidenceBlock block = (RegistryConfidenceBlock)registryFullBlockParser.Parse(packet);

			Assert.Equal(syncBlockHeight, block.SyncHeight);
			Assert.Equal(nonce, block.Nonce);
			Assert.Equal(powHash, block.PowHash);
			Assert.Equal(version, block.Version);
			Assert.Equal(blockHeight, block.Height);

			Assert.Equal(bitMask, block.BitMask);
			Assert.Equal(expectedProof, block.ConfidenceProof);
			Assert.Equal(expectedReferencedBodyHash, block.ReferencedBlockHash);

			Assert.Equal(_publicKey, block.Signer.Value.ToArray());
			Assert.Equal(expectedSignature, block.Signature.ToArray());
		}

		[Fact]
		public void SynchronizationRegistryCombinedBlockParserTest()
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

			byte[] packet = BinaryHelper.GetSignedPacket(
				LedgerType.Synchronization,
				syncBlockHeight,
				nonce, powHash, version,
				PacketTypes.Synchronization_RegistryCombinationBlock, blockHeight, prevHash, body, _privateKey, out byte[] expectedSignature);


			SynchronizationRegistryCombinedBlockParser parser = new SynchronizationRegistryCombinedBlockParser(_identityKeyProvidersRegistry);
			SynchronizationRegistryCombinedBlock block = (SynchronizationRegistryCombinedBlock)parser.Parse(packet);

			Assert.Equal(syncBlockHeight, block.SyncHeight);
			Assert.Equal(nonce, block.Nonce);
			Assert.Equal(powHash, block.PowHash);
			Assert.Equal(version, block.Version);
			Assert.Equal(blockHeight, block.Height);

			Assert.Equal(expectedHashes.Length, block.BlockHashes.Length);
			for (int i = 0; i < expectedHashes.Length; i++)
			{
				Assert.Equal(expectedHashes[i], block.BlockHashes[i]);
			}

			Assert.Equal(_publicKey, block.Signer.Value.ToArray());
			Assert.Equal(expectedSignature, block.Signature.ToArray());
		}
	}
}
