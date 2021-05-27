using Chaos.NaCl.Internal.Ed25519Ref10;
using HashLib;
using Isopoh.Cryptography.Argon2;
using Isopoh.Cryptography.SecureArray;
using System.Diagnostics;
using System.Text;
using O10.Core;
using O10.Core.Cryptography;
using O10.Core.ExtensionMethods;
using O10.Crypto.ConfidentialAssets;
using Xunit;
using Xunit.Abstractions;

namespace O10.Crypto.Tests
{
    public class CoreTests
	{
		private readonly ITestOutputHelper _testOutputHelper;

		public CoreTests(ITestOutputHelper testOutputHelper)
		{
			_testOutputHelper = testOutputHelper;
		}

		[Fact]
		public void TestDifferentHash2Point()
		{
			byte[] sk = CryptoHelper.GetRandomSeed();
			byte[] pk = CryptoHelper.GetPublicKey(sk);
            GroupElementP3 hash2P_P3 = CryptoHelper.Hash2Point(pk);

			byte[] keyImage1 = CryptoHelper.GenerateKeyImage(sk);

			GroupOperations.ge_scalarmult(out GroupElementP2 keyImage2_P2, sk, ref hash2P_P3);
			byte[] keyImage2 = new byte[32];
			GroupOperations.ge_tobytes(keyImage2, 0, ref keyImage2_P2);

			Assert.True(keyImage1.Equals32(keyImage2));
		}

		[Fact]
		public void TestMLSAG_Sign()
		{
			byte[] sk = CryptoHelper.GetRandomSeed();
            GroupElementP3 pk_P3 = CryptoHelper.GetPublicKeyP3(sk);

			byte[] alpha = CryptoHelper.GetRandomSeed();
			GroupOperations.ge_scalarmult_base(out GroupElementP3 A, alpha, 0);
			byte[] A_bytes = new byte[32];
			GroupOperations.ge_p3_tobytes(A_bytes, 0, ref A);

			byte[] c = CryptoHelper.GetRandomSeed();
			
			byte[] s = new byte[32];

			ScalarOperations.sc_mulsub(s, c, sk, alpha);

			GroupOperations.ge_double_scalarmult_vartime(out GroupElementP2 p2, c, ref pk_P3, s);
			byte[] p2_bytes = new byte[32];
			GroupOperations.ge_tobytes(p2_bytes, 0, ref p2);

			Assert.True(A_bytes.Equals32(p2_bytes));
		}

		[Fact]
		public void TestMLSAG_SignatureIsCorrect()
		{
			byte[] msg = CryptoHelper.GetRandomSeed();

			byte[] myPrevSeed = CryptoHelper.GetRandomSeed();
			byte[] aliceSeed = CryptoHelper.GetRandomSeed();
			byte[] bobSeed = CryptoHelper.GetRandomSeed();

			byte[] myPrevPublicKey = CryptoHelper.GetPublicKey(myPrevSeed);
			byte[] alicePublicKey = CryptoHelper.GetPublicKey(aliceSeed);
			byte[] bobPublicKey = CryptoHelper.GetPublicKey(bobSeed);

			byte[] myPrevBlindingFactor = CryptoHelper.GetRandomSeed();
			byte[] aliceBlindingFactor = CryptoHelper.GetRandomSeed();
			byte[] bobBlindingFactor = CryptoHelper.GetRandomSeed();


			IHash hash = HashFactory.Crypto.SHA3.CreateKeccak256();

			byte[] myAsset = hash.ComputeBytes(Encoding.UTF8.GetBytes("assetMine")).GetBytes();
			byte[] aliceAsset = hash.ComputeBytes(Encoding.UTF8.GetBytes("assetAlice")).GetBytes();
			byte[] bobAsset = hash.ComputeBytes(Encoding.UTF8.GetBytes("assetBob")).GetBytes();

			byte[] myPrevCommitment = CryptoHelper.GetAssetCommitment(myPrevBlindingFactor, myAsset);
			byte[] aliceCommitment = CryptoHelper.GetAssetCommitment(aliceBlindingFactor, aliceAsset);
			byte[] bobCommitment = CryptoHelper.GetAssetCommitment(bobBlindingFactor, bobAsset);

			CtTuple[][] pubs = new CtTuple[][]
			{
				new CtTuple[] { new CtTuple { Dest = myPrevPublicKey, Mask = myPrevCommitment} },
				new CtTuple[] { new CtTuple { Dest = alicePublicKey, Mask = aliceCommitment} },
				new CtTuple[] { new CtTuple { Dest = bobPublicKey, Mask = bobCommitment} },
			};

			byte[] myNewBlindingFactor = CryptoHelper.GetRandomSeed();
			byte[] myNewCommitment = CryptoHelper.GetAssetCommitment(myNewBlindingFactor, myAsset);

			byte[] newSk = CryptoHelper.GetRandomSeed();
			byte[] newPk = CryptoHelper.GetPublicKey(newSk);

			CtTuple[] inSk = new CtTuple[] { new CtTuple { Dest = myPrevSeed, Mask = myPrevBlindingFactor} };
			CtTuple[] outSk = new CtTuple[] { new CtTuple { Dest = newSk, Mask = myNewBlindingFactor } };
			CtTuple[] outPk = new CtTuple[] { new CtTuple { Dest = newPk, Mask = myNewCommitment } };

            MgSig mgSig = CryptoHelper.ProveRctMG(msg, pubs, inSk, outSk, outPk, 0);
            bool res = CryptoHelper.VerRctMG(mgSig, pubs, outPk, msg);

			Assert.True(res);
		}

		[Fact]
		public void TestMLSAG_Mixed_SignatureIsIncorrect()
		{
			byte[] msg = CryptoHelper.GetRandomSeed();

			byte[] myPrevSeed = CryptoHelper.GetRandomSeed();
			byte[] aliceSeed = CryptoHelper.GetRandomSeed();
			byte[] bobSeed = CryptoHelper.GetRandomSeed();

			byte[] myPrevPublicKey = CryptoHelper.GetPublicKey(myPrevSeed);
			byte[] alicePublicKey = CryptoHelper.GetPublicKey(aliceSeed);
			byte[] bobPublicKey = CryptoHelper.GetPublicKey(bobSeed);

			byte[] myPrevBlindingFactor = CryptoHelper.GetRandomSeed();
			byte[] aliceBlindingFactor = CryptoHelper.GetRandomSeed();
			byte[] bobBlindingFactor = CryptoHelper.GetRandomSeed();


			IHash hash = HashFactory.Crypto.SHA3.CreateKeccak256();

			byte[] myAsset = hash.ComputeBytes(Encoding.UTF8.GetBytes("assetMine")).GetBytes();
			byte[] aliceAsset = hash.ComputeBytes(Encoding.UTF8.GetBytes("assetAlice")).GetBytes();
			byte[] bobAsset = hash.ComputeBytes(Encoding.UTF8.GetBytes("assetBob")).GetBytes();

			byte[] myPrevCommitment = CryptoHelper.GetAssetCommitment(myPrevBlindingFactor, myAsset);
			byte[] aliceCommitment = CryptoHelper.GetAssetCommitment(aliceBlindingFactor, aliceAsset);
			byte[] bobCommitment = CryptoHelper.GetAssetCommitment(bobBlindingFactor, bobAsset);

			CtTuple[][] pubs = new CtTuple[][]
			{
				new CtTuple[] { new CtTuple { Dest = myPrevPublicKey, Mask = myPrevCommitment} },
				new CtTuple[] { new CtTuple { Dest = alicePublicKey, Mask = aliceCommitment} },
				new CtTuple[] { new CtTuple { Dest = bobPublicKey, Mask = bobCommitment} },
			};

			byte[] myNewBlindingFactor = CryptoHelper.GetRandomSeed();
			byte[] myNewCommitment = CryptoHelper.GetAssetCommitment(myNewBlindingFactor, aliceAsset);

			byte[] newSk = CryptoHelper.GetRandomSeed();
			byte[] newPk = CryptoHelper.GetPublicKey(newSk);

			CtTuple[] inSk = new CtTuple[] { new CtTuple { Dest = myPrevSeed, Mask = myPrevBlindingFactor } };
			CtTuple[] outSk = new CtTuple[] { new CtTuple { Dest = newSk, Mask = myNewBlindingFactor } };
			CtTuple[] outPk = new CtTuple[] { new CtTuple { Dest = newPk, Mask = myNewCommitment } };

            MgSig mgSig = CryptoHelper.ProveRctMG(msg, pubs, inSk, outSk, outPk, 0);
			bool res = CryptoHelper.VerRctMG(mgSig, pubs, outPk, msg);

			Assert.False(res);
		}

		[Theory, InlineData(50)]
		public void PasswordHashBruteForceTest(int count)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			for (int i = 0; i < count; i++)
			{
                CryptoHelper.PasswordHash("Password");
			}

			stopwatch.Stop();
			long elapsed = stopwatch.ElapsedMilliseconds;
			_testOutputHelper.WriteLine($"elapsed = {elapsed} ms for {count} loops");
			Assert.True(elapsed > 50000);
		}

		[Fact]
		public void Argon2BruteForceTest()
		{
			var conf1 = new Argon2Config
			{
				HashLength = Globals.DEFAULT_HASH_SIZE,
				TimeCost = 3,
				Password = Encoding.ASCII.GetBytes("password")
			};
			using Argon2 hasher = new Argon2(conf1);
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			for (int i = 0; i < 10; i++)
			{
				SecureArray<byte> secureArray = hasher.Hash();
			}
			stopwatch.Stop();

			long elapsed = stopwatch.ElapsedMilliseconds;
			_testOutputHelper.WriteLine($"elapsed = {elapsed} while type is {conf1.Type}");
			Assert.True(elapsed > 10000);

			var conf2 = new Argon2Config
			{
				HashLength = Globals.DEFAULT_HASH_SIZE,
				Type = Argon2Type.DataIndependentAddressing,
				TimeCost = 3,
				Password = Encoding.ASCII.GetBytes("password")
			};
			using Argon2 hasher2 = new Argon2(conf2);
			Stopwatch stopwatch2 = new Stopwatch();
			stopwatch2.Start();
			for (int i = 0; i < 10; i++)
			{
				SecureArray<byte> secureArray = hasher2.Hash();
			}
			stopwatch2.Stop();

			long elapsed2 = stopwatch2.ElapsedMilliseconds;
			_testOutputHelper.WriteLine($"elapsed2 = {elapsed2} while type is {conf2.Type}");
			Assert.True(elapsed2 > 20000);
		}

		[Fact]
		public void Argon2SingleTest()
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			var conf1 = new Argon2Config
			{
				HashLength = Globals.DEFAULT_HASH_SIZE,
				TimeCost = 3,
				Password = Encoding.ASCII.GetBytes("password")
			};
			using Argon2 hasher = new Argon2(conf1);

			SecureArray<byte> secureArray = hasher.Hash();
			stopwatch.Stop();

			long elapsed = stopwatch.ElapsedMilliseconds;
			_testOutputHelper.WriteLine($"elapsed = {elapsed} while type is {conf1.Type}");

			Assert.True(elapsed > 1000);

			Stopwatch stopwatch2 = new Stopwatch();
			stopwatch2.Start();
			var conf2 = new Argon2Config
			{
				HashLength = Globals.DEFAULT_HASH_SIZE,
				Type = Argon2Type.DataIndependentAddressing,
				TimeCost = 3,
				Password = Encoding.ASCII.GetBytes("password")
			};
			using Argon2 hasher2 = new Argon2(conf2);

			SecureArray<byte> secureArray2 = hasher2.Hash();
			stopwatch2.Stop();

			long elapsed2 = stopwatch2.ElapsedMilliseconds;
			_testOutputHelper.WriteLine($"elapsed2 = {elapsed2} while type is {conf2.Type}");

			Assert.True(elapsed2 > 1000);

			bool equals = secureArray.Buffer.Equals32(secureArray2.Buffer);

			Assert.False(equals);
		}

		[Fact]
		public void MultipleAssetBasedRegistrationTest()
        {
			byte[] assetId1 = CryptoHelper.GetRandomSeed();
			byte[] assetId2 = CryptoHelper.GetRandomSeed();
			byte[] bfReg = CryptoHelper.GetRandomSeed();

			byte[] commitmentReg = CryptoHelper.GetAssetCommitment(bfReg, assetId1, assetId2);

			byte[] bfVer = CryptoHelper.GetRandomSeed();
			byte[] bfDiff = CryptoHelper.GetDifferentialBlindingFactor(bfVer, bfReg);
			byte[] commitmentVer = CryptoHelper.GetAssetCommitment(bfVer, assetId1, assetId2);

            SurjectionProof surjectionProof = CryptoHelper.CreateSurjectionProof(commitmentVer, new byte[][] { commitmentReg }, 0, bfDiff);

			bool res = CryptoHelper.VerifySurjectionProof(surjectionProof, commitmentVer);

			Assert.True(res);
		}

		[Fact]
		public void MultipleAssetBasedRegistration2Test()
		{
			byte[] assetId1 = CryptoHelper.GetRandomSeed();
			byte[] assetId2 = CryptoHelper.GetRandomSeed();
			byte[] bfReg = CryptoHelper.GetRandomSeed();

			byte[] commitmentReg = CryptoHelper.GetAssetCommitment(bfReg, assetId1, assetId2);

			byte[] bfVer1 = CryptoHelper.GetRandomSeed();
			byte[] bfVer2 = CryptoHelper.GetRandomSeed();
			byte[] commitmentVer1 = CryptoHelper.GetAssetCommitment(bfVer1, assetId1);
			byte[] commitmentVer2 = CryptoHelper.GetAssetCommitment(bfVer2, assetId2);
			byte[] bfVerTotal = CryptoHelper.SumScalars(bfVer1, bfVer2);
			byte[] bfDiff = CryptoHelper.GetDifferentialBlindingFactor(bfVerTotal, bfReg);
			byte[] commitmentVerSum = CryptoHelper.SumCommitments(commitmentVer1, commitmentVer2);

            SurjectionProof surjectionProof = CryptoHelper.CreateSurjectionProof(commitmentVerSum, new byte[][] { commitmentReg }, 0, bfDiff);

			bool res = CryptoHelper.VerifySurjectionProof(surjectionProof, commitmentVerSum);

			Assert.True(res);
		}
	}
}
