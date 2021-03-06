﻿using System.IO;
using O10.Transactions.Core.Ledgers.O10State;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Parsers.Transactional;
using O10.Crypto.ConfidentialAssets;
using O10.Tests.Core;
using O10.Tests.Core.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace O10.Transactions.Core.Tests.ParserTests
{
    public class TransactionalParsersTests : BlockchainCoreTestBase
	{
		public TransactionalParsersTests(CoreFixture coreFixture, ITestOutputHelper testOutputHelper) : base(coreFixture, testOutputHelper)
		{

		}

		[Fact]
		public void IssueAssetTest()
		{
			ulong tagId = 147;
			ulong syncBlockHeight = 1;
			uint nonce = 4;
			byte[] powHash = BinaryHelper.GetPowHash(1234);
			ushort version = 1;
			ulong blockHeight = 9;
			byte[] body;
			ulong uptodateFunds = 10001;

			byte[] assetId = CryptoHelper.GetRandomSeed();
			byte[] assetCommitment = CryptoHelper.GetRandomSeed();
			byte[] mask = CryptoHelper.GetRandomSeed();
			byte[] maskedAssetId = CryptoHelper.GetRandomSeed();

			using (MemoryStream ms = new MemoryStream())
			{
				using (BinaryWriter bw = new BinaryWriter(ms))
				{
					bw.Write(tagId);
					bw.Write(uptodateFunds);
					bw.Write(assetId);
					bw.Write(assetCommitment);
					bw.Write(mask);
					bw.Write(maskedAssetId);
				}

				body = ms.ToArray();
			}
		}

		[Fact]
		public void IssueBlindedAssetParserTest()
		{
			byte[] groupId = CryptoHelper.GetRandomSeed();
			ulong syncBlockHeight = 1;
			uint nonce = 4;
			byte[] powHash = BinaryHelper.GetPowHash(1234);
			ushort version = 1;
			ulong blockHeight = 9;
			byte[] body;
			ulong uptodateFunds = 10001;

			byte[] assetCommitment = CryptoHelper.GetRandomSeed();
			byte[] mask = CryptoHelper.GetRandomSeed();
			byte[] maskedAssetId = CryptoHelper.GetRandomSeed();
			byte[] keyImage = CryptoHelper.GetRandomSeed();
			byte[] c = CryptoHelper.GetRandomSeed();
			byte[] r = CryptoHelper.GetRandomSeed();

			using (MemoryStream ms = new MemoryStream())
			{
				using (BinaryWriter bw = new BinaryWriter(ms))
				{
					bw.Write(uptodateFunds);
					bw.Write(groupId);
					bw.Write(assetCommitment);
					//bw.Write(mask);
					//bw.Write(maskedAssetId);
					bw.Write(keyImage);
					bw.Write(c);
					bw.Write(r);
				}

				body = ms.ToArray();
			}

			byte[] packet = BinaryHelper.GetSignedPacket(LedgerType.O10State, syncBlockHeight, nonce, powHash, version,
				TransactionTypes.Transaction_IssueBlindedAsset, blockHeight, null, body, _privateKey, out byte[] expectedSignature);

			IssueBlindedAssetParser parser = new IssueBlindedAssetParser(_identityKeyProvidersRegistry);
			IssueBlindedAsset block = (IssueBlindedAsset)parser.Parse(packet);

			Assert.Equal(syncBlockHeight, block.SyncHeight);
			Assert.Equal(nonce, block.Nonce);
			Assert.Equal(powHash, block.PowHash);
			Assert.Equal(version, block.Version);
			Assert.Equal(blockHeight, block.Height);
			Assert.Equal(uptodateFunds, block.UptodateFunds);
			Assert.Equal(groupId, block.GroupId);
			Assert.Equal(assetCommitment, block.AssetCommitment);
			Assert.Equal(keyImage, block.KeyImage);
			Assert.Equal(c, block.UniquencessProof.C);
			Assert.Equal(r, block.UniquencessProof.R);
			Assert.Equal(expectedSignature, block.Signature.ToArray());
		}

		[Fact]
		public void transferAssetToStealthParserTest()
		{
			ulong syncBlockHeight = 1;
			uint nonce = 4;
			byte[] powHash = BinaryHelper.GetPowHash(1234);
			ushort version = 1;
			ulong blockHeight = 9;
			ulong uptodateFunds = 10002;

			byte[] destinationKey = CryptoHelper.GetRandomSeed();
			byte[] transactionKey = CryptoHelper.GetRandomSeed();

			byte[] body;

			byte[] assetCommitment = CryptoHelper.GetRandomSeed();
			byte[] mask = CryptoHelper.GetRandomSeed();
			byte[] maskedAssetId = CryptoHelper.GetRandomSeed();

			ushort assetCommitmentsCount = 10;
			byte[][] assetCommitments = new byte[assetCommitmentsCount][];
			byte[] e = CryptoHelper.GetRandomSeed();
			byte[][] s = new byte[assetCommitmentsCount][];

			for (int i = 0; i < assetCommitmentsCount; i++)
			{
				assetCommitments[i] = CryptoHelper.GetRandomSeed();
				s[i] = CryptoHelper.GetRandomSeed();
			}

			using (MemoryStream ms = new MemoryStream())
			{
				using (BinaryWriter bw = new BinaryWriter(ms))
				{
					bw.Write(uptodateFunds);
					bw.Write(destinationKey);
					bw.Write(transactionKey);
					bw.Write(assetCommitment);
					bw.Write(maskedAssetId);
					bw.Write(mask);
					bw.Write(assetCommitmentsCount);
					for (int i = 0; i < assetCommitmentsCount; i++)
					{
						bw.Write(assetCommitments[i]);
					}
					bw.Write(e);
					for (int i = 0; i < assetCommitmentsCount; i++)
					{
						bw.Write(s[i]);
					}
				}

				body = ms.ToArray();
			}

			byte[] packet = BinaryHelper.GetSignedPacket(
				LedgerType.Synchronization,
				syncBlockHeight,
				nonce, powHash, version,
				TransactionTypes.Transaction_TransferAssetToStealth, blockHeight, null, body, _privateKey, out byte[] expectedSignature);

			transferAssetToStealthParser parser = new transferAssetToStealthParser(_identityKeyProvidersRegistry);
			TransferAssetToStealth block = (TransferAssetToStealth)parser.Parse(packet);

			Assert.Equal(syncBlockHeight, block.SyncHeight);
			Assert.Equal(nonce, block.Nonce);
			Assert.Equal(powHash, block.PowHash);
			Assert.Equal(version, block.Version);
			Assert.Equal(blockHeight, block.Height);
			Assert.Equal(destinationKey, block.DestinationKey);
			Assert.Equal(transactionKey, block.TransactionPublicKey);
			Assert.Equal(assetCommitment, block.TransferredAsset.AssetCommitment);
			Assert.Equal(maskedAssetId, block.TransferredAsset.EcdhTuple.AssetId);
			Assert.Equal(mask, block.TransferredAsset.EcdhTuple.Mask);
			Assert.Equal(assetCommitmentsCount, block.SurjectionProof.AssetCommitments.Length);
			Assert.Equal(e, block.SurjectionProof.Rs.E);

			for (int i = 0; i < assetCommitmentsCount; i++)
			{
				Assert.Equal(assetCommitments[i], block.SurjectionProof.AssetCommitments[i]);
				Assert.Equal(s[i], block.SurjectionProof.Rs.S[i]);
			}
			Assert.Equal(expectedSignature, block.Signature.ToArray());
		}
	}
}