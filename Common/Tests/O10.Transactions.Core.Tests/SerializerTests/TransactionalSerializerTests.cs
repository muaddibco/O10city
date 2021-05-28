using System.IO;
using O10.Transactions.Core.Ledgers.O10State;
using O10.Transactions.Core.Ledgers.O10State.Internal;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Serializers.Signed.Transactional;
using O10.Core.Cryptography;
using O10.Crypto.ConfidentialAssets;
using O10.Tests.Core;
using O10.Tests.Core.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace O10.Transactions.Core.Tests.SerializerTests
{
    public class TransactionalSerializerTests : BlockchainCoreTestBase
	{
		public TransactionalSerializerTests(CoreFixture coreFixture, ITestOutputHelper testOutputHelper) : base(coreFixture, testOutputHelper)
		{

		}

		[Fact]
		public void IssueBlindedAssetSerializerTest()
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
					bw.Write(keyImage);
					bw.Write(c);
					bw.Write(r);
				}

				body = ms.ToArray();
			}

			byte[] expectedPacket = BinaryHelper.GetSignedPacket(LedgerType.O10State, syncBlockHeight, nonce, powHash, version,
				TransactionTypes.Transaction_IssueBlindedAsset, blockHeight, null, body, _privateKey, out byte[] expectedSignature);

			IssueBlindedAsset issueBlindedAsset = new IssueBlindedAsset
			{
				SyncHeight = syncBlockHeight,
				Nonce = nonce,
				PowHash = powHash,
				Height = blockHeight,
				GroupId = groupId,
				UptodateFunds = uptodateFunds,
				AssetCommitment = assetCommitment,
				KeyImage = keyImage,
				UniquencessProof = new RingSignature
				{
					 C = c,
					 R = r
				}
			};

			using IssueBlindedAssetSerializer serializer = new IssueBlindedAssetSerializer(null);
			serializer.Initialize(issueBlindedAsset);
			serializer.SerializeBody();
			_signingService.Sign(issueBlindedAsset);

			byte[] actualPacket = serializer.GetBytes();

			Assert.Equal(expectedPacket, actualPacket);
		}

		[Fact]
		public void transferAssetToStealthSerializerTest()
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

			byte[] expectedPacket = BinaryHelper.GetSignedPacket(LedgerType.O10State, syncBlockHeight, nonce, powHash, version,
				TransactionTypes.Transaction_TransferAssetToStealth, blockHeight, null, body, _privateKey, out byte[] expectedSignature);

			TransferAssetToStealth packet = new TransferAssetToStealth
			{
				SyncHeight = syncBlockHeight,
				Nonce = nonce,
				PowHash = powHash,
				Height = blockHeight,
				UptodateFunds = uptodateFunds,
				DestinationKey = destinationKey, 
				TransactionPublicKey = transactionKey,
				TransferredAsset = new EncryptedAsset
				{
					AssetCommitment = assetCommitment,
					EcdhTuple = new EcdhTupleCA
					{
						Mask = mask,
						AssetId = maskedAssetId
					}
				},
				SurjectionProof = new SurjectionProof
				{
					AssetCommitments = assetCommitments,
					Rs = new BorromeanRingSignature
					{
						E = e,
						S = s
					}
				}
			};

			using transferAssetToStealthSerializer serializer = new transferAssetToStealthSerializer(null);
			serializer.Initialize(packet);
			serializer.SerializeBody();
			_signingService.Sign(packet);

			byte[] actualPacket = serializer.GetBytes();

			Assert.Equal(expectedPacket, actualPacket);
		}
	}
}
