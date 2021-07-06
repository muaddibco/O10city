using O10.Core.Cryptography;
using O10.Crypto.ConfidentialAssets;
using Xunit;

namespace O10.Crypto.Tests
{
    public class RangeProofTests
	{
		[Fact]
		public void TestRangeProof()
		{
			ulong amount = 100;
			byte[] assetCommitment = CryptoHelper.GetNonblindedAssetCommitment(CryptoHelper.GetRandomSeed());
            RangeProof rangeProof = CryptoHelper.ProveRange(out byte[] C, out byte[] mask, 100, assetCommitment);
			bool res = CryptoHelper.VerRange(C, rangeProof, assetCommitment);

			Assert.True(res);

            RangeProof rangeProof2 = CryptoHelper.ProveRange(out byte[] C2, out byte[] mask2, 101, assetCommitment);
			res = CryptoHelper.VerRange(C2, rangeProof, assetCommitment);

			Assert.False(res);

			byte[] assetCommitment2 = CryptoHelper.GetNonblindedAssetCommitment(CryptoHelper.GetRandomSeed());
			res = CryptoHelper.VerRange(C, rangeProof, assetCommitment2);

			Assert.False(res);
		}

		[Fact]
		public void TestDiffRangeProof()
        {
			ulong amount = 100;
			byte[] assetCommitment = CryptoHelper.GetNonblindedAssetCommitment(CryptoHelper.GetRandomSeed());

			RangeProof rangeProof1 = CryptoHelper.ProveRange(out byte[] C1, out byte[] mask1, 100, assetCommitment);
			bool res1 = CryptoHelper.VerRange(C1, rangeProof1, assetCommitment);

			Assert.True(res1);

			RangeProof rangeProof2 = CryptoHelper.ProveRange(out byte[] C2, out byte[] mask2, 100, assetCommitment);
			bool res2 = CryptoHelper.VerRange(C2, rangeProof2, assetCommitment);

			Assert.True(res2);

			byte[] maskDiff = CryptoHelper.GetDifferentialBlindingFactor(mask2, mask1);

			byte[] CDiff = CryptoHelper.SubCommitments(C2, C1);

			byte[] msg = CryptoHelper.GetRandomSeed();

			var signature = CryptoHelper.GenerateBorromeanRingSignature(msg, new byte[][] { CDiff }, 0, maskDiff);

			bool resDiff = CryptoHelper.VerifyRingSignature(signature, msg, new byte[][] { CDiff });

			Assert.True(resDiff);

			RangeProof rangeProof3 = CryptoHelper.ProveRange(out byte[] C3, out byte[] mask3, 99, assetCommitment);
			bool res3 = CryptoHelper.VerRange(C3, rangeProof2, assetCommitment);

			Assert.True(res2);

			byte[] maskDiff2 = CryptoHelper.GetDifferentialBlindingFactor(mask3, mask1);

			byte[] CDiff2 = CryptoHelper.SubCommitments(C3, C1);

			var signature2 = CryptoHelper.GenerateBorromeanRingSignature(msg, new byte[][] { CDiff2 }, 0, maskDiff2);

			bool resDiff2 = CryptoHelper.VerifyRingSignature(signature2, msg, new byte[][] { CDiff2 });

			Assert.False(resDiff2);
		}

		[Fact]
		public void TestRangeProofStability()
		{
			for (int i = 0; i < 100; i++)
			{
				byte[] assetCommitment = CryptoHelper.GetNonblindedAssetCommitment(CryptoHelper.GetRandomSeed());
                RangeProof rangeProof = CryptoHelper.ProveRange(out byte[] C, out byte[] mask, 100, assetCommitment);
				bool res = CryptoHelper.VerRange(C, rangeProof, assetCommitment);

				Assert.True(res);
			}
		}
	}
}
