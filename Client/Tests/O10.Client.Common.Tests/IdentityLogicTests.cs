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
            _issuer = CryptoHelper.GetRandomSeed().ToHexString();
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
            bindingKeySource.SetResult(CryptoHelper.PasswordHash(pwd));

            byte[] commitment = CryptoHelper.GetNonblindedAssetCommitment(rootAttributeAssetId);
            byte[] bf = CryptoHelper.GetRandomSeed();
            byte[] commitmentToRoot = CryptoHelper.BlindAssetCommitment(commitment, bf);

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
            byte[] nonBlindedRootCommitment = CryptoHelper.GetNonblindedAssetCommitment(rootAttributeAssetId);
            byte[] protectionAssetId = await _assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD, rootAttributeAssetId.ToHexString(), _issuer).ConfigureAwait(false);
            byte[] nonBlindedProtectionCommitment = CryptoHelper.GetNonblindedAssetCommitment(protectionAssetId);
            byte[] commitmentToValueProtection = CryptoHelper.SumCommitments(blindingPointValue, nonBlindedProtectionCommitment);
            byte[] commitmentToRootProtection = CryptoHelper.SumCommitments(blindingPointRoot, nonBlindedRootCommitment);
            commitmentToRootProtection = CryptoHelper.SumCommitments(commitmentToRootProtection, nonBlindedProtectionCommitment);

            byte[] bfProtection = CryptoHelper.GetRandomSeed();
            byte[] bfToValueDiff = CryptoHelper.GetDifferentialBlindingFactor(bfProtection, bfValue);
            byte[] commitmentToProtectionValue = CryptoHelper.BlindAssetCommitment(nonBlindedProtectionCommitment, bfProtection);
            var surjectionProofValue = CryptoHelper.CreateSurjectionProof(commitmentToProtectionValue, new byte[][] { commitmentToValueProtection }, 0, bfToValueDiff);
            byte[] commitmentToProtectionRoot = CryptoHelper.SumCommitments(commitmentToProtectionValue, commitmentToRoot);
            byte[] bfProtectionAndRoot = CryptoHelper.SumScalars(bfProtection, bf);
            byte[] bfToRootDiff = CryptoHelper.GetDifferentialBlindingFactor(bfProtectionAndRoot, bfRoot);
            var surjectionProofRoot = CryptoHelper.CreateSurjectionProof(commitmentToProtectionRoot, new byte[][] { commitmentToRootProtection }, 0, bfToRootDiff);

            bool resProtectionValue = CryptoHelper.VerifySurjectionProof(surjectionProofValue, commitmentToProtectionValue);
            bool resProtectionRoot = CryptoHelper.VerifySurjectionProof(surjectionProofRoot, commitmentToProtectionRoot);

            Assert.True(resProtectionValue, "Verification of Proof to Value failed");
            Assert.True(resProtectionRoot, "Verification of Proof to Binding failed");

            return (surjectionProofValue, surjectionProofRoot);
        }
    }
}
