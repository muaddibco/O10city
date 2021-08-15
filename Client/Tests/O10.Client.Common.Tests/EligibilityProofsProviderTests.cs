using NSubstitute;
using O10.Client.Common.Configuration;
using O10.Client.Common.Interfaces;
using O10.Client.Common.Services;
using O10.Core.Configuration;
using O10.Crypto.ConfidentialAssets;
using O10.Tests.Core;
using O10.Tests.Core.Fixtures;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace O10.Client.Common.Tests
{
    public class EligibilityProofsProviderTests : TestBase
    {
        private readonly IGatewayService _gatewayService;
        private readonly IConfigurationService _configurationService;
        private readonly IRestApiConfiguration _restApiConfiguration;

        public EligibilityProofsProviderTests(CoreFixture coreFixture, ITestOutputHelper testOutputHelper) : base(coreFixture, testOutputHelper)
        {
            _configurationService = Substitute.For<IConfigurationService>();
            _restApiConfiguration = Substitute.For<IRestApiConfiguration>();
            _restApiConfiguration.RingSize.ReturnsForAnyArgs<ushort>(1);

            _configurationService.Get<IRestApiConfiguration>().ReturnsForAnyArgs(_restApiConfiguration);

            _gatewayService = Substitute.For<IGatewayService>();
            _gatewayService.GetIssuanceCommitments(Array.Empty<byte>(), 0).ReturnsForAnyArgs(ci =>
            {
                var ringSize = ci.ArgAt<int>(1);

                byte[][] issuanceCommitments = new byte[ringSize][];

                for (int i = 0; i < ringSize; i++)
                {
                    issuanceCommitments[i] = CryptoHelper.GetRandomSeed();
                }

                return issuanceCommitments;
            });
        }

        [Fact]
        public async Task CreateEligibilityProofTest()
        {
            byte[] assetId = CryptoHelper.GetRandomSeed();
            byte[] bfOrig = CryptoHelper.GetRandomSeed();
            byte[] bfNew = CryptoHelper.GetRandomSeed();

            byte[] assetCommitmentOrig = CryptoHelper.GetAssetCommitment(bfOrig, assetId);
            byte[] assetCommitmentNew = CryptoHelper.GetAssetCommitment(bfNew, assetId);
            byte[] issuer = CryptoHelper.GetRandomSeed();

            var eligibilityProofsProvider = new EligibilityProofsProvider(CoreFixture.LoggerService, _gatewayService, _configurationService);

            var sp = await eligibilityProofsProvider.CreateEligibilityProof(assetCommitmentNew, bfNew, assetCommitmentOrig, bfOrig, issuer);
            var res = CryptoHelper.VerifySurjectionProof(sp, assetCommitmentNew);


            Assert.True(res);
        }
    }
}
