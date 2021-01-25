using System.Threading.Tasks;
using NSubstitute;
using O10.Client.Common.Identities;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.AttributesScheme;
using O10.Core.ExtensionMethods;
using O10.Core.Cryptography;
using O10.Core.HashCalculations;
using O10.Crypto.ConfidentialAssets;
using O10.Crypto.HashCalculations;
using Xunit;
using Xunit.Abstractions;
using O10.Client.Common.Entities;
using O10.Core.Identity;
using System.Text;

namespace O10.Client.Common.Tests
{
    public class IdentityLogicTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IAssetsService _assetsService;
        private readonly string _issuer;

        public IdentityLogicTests(ITestOutputHelper testOutputHelper)
        {
            _issuer = ConfidentialAssetsHelper.GetRandomSeed().ToHexString();
            _testOutputHelper = testOutputHelper;
            IHashCalculationsRepository hashCalculationsRepository = Substitute.For<IHashCalculationsRepository>();
            hashCalculationsRepository.Create(HashType.Tiger4).Returns(new Tiger4HashCalculation());
            ISchemeResolverService schemeResolverService = Substitute.For<ISchemeResolverService>();
            schemeResolverService
                .ResolveAttributeScheme(null, null)
                .ReturnsForAnyArgs(ci => 
                {
                    return (ci.ArgAt<string>(1)) switch
                    {
                        AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD => new AttributeDefinition { SchemeId = 3 },
                        _ => null,
                    };
                });
            _assetsService = new AssetsService(hashCalculationsRepository, schemeResolverService);
        }

        [Fact]
        public async void ProofToValueAndRootTest()
        {
            string pwd = "Password";
            string rootAttributeContent = "mail@server.com";
            byte[] rootAttributeAssetId = _assetsService.GenerateAssetId(1, rootAttributeContent);

            TaskCompletionSource<byte[]> bindingKeySource = new TaskCompletionSource<byte[]>();
            bindingKeySource.SetResult(ConfidentialAssetsHelper.PasswordHash(pwd));

            byte[] commitment = ConfidentialAssetsHelper.GetNonblindedAssetCommitment(rootAttributeAssetId);
            byte[] bf = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] commitmentToRoot = ConfidentialAssetsHelper.BlindAssetCommitment(commitment, bf);

            (SurjectionProof surjectionProofValue, SurjectionProof surjectionProofRoot) = 
                await GetAttributeProofs(bf, commitmentToRoot, rootAttributeContent, rootAttributeAssetId, bindingKeySource)
                .ConfigureAwait(false);

            Assert.NotNull(surjectionProofValue);
            Assert.NotNull(surjectionProofRoot);

        }

        private async Task<(SurjectionProof surjectionProofValue, SurjectionProof surjectionProofRoot)> GetAttributeProofs(byte[] bf, byte[] commitmentToRoot, string rootAttributeContent, byte[] rootAttributeAssetId, TaskCompletionSource<byte[]> bindingKeySource)
        {
            var rootAttributeContentBytes = Encoding.UTF8.GetBytes(rootAttributeContent);
            byte[] bfRoot = _assetsService.GetBlindingFactor(await bindingKeySource.Task.ConfigureAwait(false), rootAttributeContentBytes);
            byte[] blindingPointRoot = _assetsService.GetBlindingPoint(await bindingKeySource.Task.ConfigureAwait(false), rootAttributeContentBytes);
            byte[] bfValue = _assetsService.GetBlindingFactor(await bindingKeySource.Task.ConfigureAwait(false), rootAttributeContentBytes, await bindingKeySource.Task.ConfigureAwait(false));
            byte[] blindingPointValue = _assetsService.GetBlindingPoint(await bindingKeySource.Task.ConfigureAwait(false), rootAttributeContentBytes, await bindingKeySource.Task.ConfigureAwait(false));
            byte[] nonBlindedRootCommitment = ConfidentialAssetsHelper.GetNonblindedAssetCommitment(rootAttributeAssetId);
            byte[] protectionAssetId = await _assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD, rootAttributeAssetId.ToHexString(), _issuer).ConfigureAwait(false);
            byte[] nonBlindedProtectionCommitment = ConfidentialAssetsHelper.GetNonblindedAssetCommitment(protectionAssetId);
            byte[] commitmentToValueProtection = ConfidentialAssetsHelper.SumCommitments(blindingPointValue, nonBlindedProtectionCommitment);
            byte[] commitmentToRootProtection = ConfidentialAssetsHelper.SumCommitments(blindingPointRoot, nonBlindedRootCommitment);
            commitmentToRootProtection = ConfidentialAssetsHelper.SumCommitments(commitmentToRootProtection, nonBlindedProtectionCommitment);

            byte[] bfProtection = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] bfToValueDiff = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(bfProtection, bfValue);
            byte[] commitmentToProtectionValue = ConfidentialAssetsHelper.BlindAssetCommitment(nonBlindedProtectionCommitment, bfProtection);
            var surjectionProofValue = ConfidentialAssetsHelper.CreateSurjectionProof(commitmentToProtectionValue, new byte[][] { commitmentToValueProtection }, 0, bfToValueDiff);
            byte[] commitmentToProtectionRoot = ConfidentialAssetsHelper.SumCommitments(commitmentToProtectionValue, commitmentToRoot);
            byte[] bfProtectionAndRoot = ConfidentialAssetsHelper.SumScalars(bfProtection, bf);
            byte[] bfToRootDiff = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(bfProtectionAndRoot, bfRoot);
            var surjectionProofRoot = ConfidentialAssetsHelper.CreateSurjectionProof(commitmentToProtectionRoot, new byte[][] { commitmentToRootProtection }, 0, bfToRootDiff);

            bool resProtectionValue = ConfidentialAssetsHelper.VerifySurjectionProof(surjectionProofValue, commitmentToProtectionValue);
            bool resProtectionRoot = ConfidentialAssetsHelper.VerifySurjectionProof(surjectionProofRoot, commitmentToProtectionRoot);

            Assert.True(resProtectionValue, "Verification of Proof to Value failed");
            Assert.True(resProtectionRoot, "Verification of Proof to Binding failed");

            return (surjectionProofValue, surjectionProofRoot);
        }
    }
}
