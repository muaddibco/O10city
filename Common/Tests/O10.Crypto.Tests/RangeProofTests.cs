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
			byte[] assetCommitment = ConfidentialAssetsHelper.GetNonblindedAssetCommitment(ConfidentialAssetsHelper.GetRandomSeed());
			RangeProof rangeProof = ConfidentialAssetsHelper.ProveRange(out byte[] C, out byte[] mask, 100, assetCommitment);
			bool res = ConfidentialAssetsHelper.VerRange(C, rangeProof, assetCommitment);

			Assert.True(res);

			RangeProof rangeProof2 = ConfidentialAssetsHelper.ProveRange(out byte[] C2, out byte[] mask2, 101, assetCommitment);
			res = ConfidentialAssetsHelper.VerRange(C2, rangeProof, assetCommitment);

			Assert.False(res);

			byte[] assetCommitment2 = ConfidentialAssetsHelper.GetNonblindedAssetCommitment(ConfidentialAssetsHelper.GetRandomSeed());
			res = ConfidentialAssetsHelper.VerRange(C, rangeProof, assetCommitment2);

			Assert.False(res);
		}

		[Fact]
		public void TestRangeProofStability()
		{
			for (int i = 0; i < 100; i++)
			{
				byte[] assetCommitment = ConfidentialAssetsHelper.GetNonblindedAssetCommitment(ConfidentialAssetsHelper.GetRandomSeed());
				RangeProof rangeProof = ConfidentialAssetsHelper.ProveRange(out byte[] C, out byte[] mask, 100, assetCommitment);
				bool res = ConfidentialAssetsHelper.VerRange(C, rangeProof, assetCommitment);

				Assert.True(res);
			}
		}
	}
}
