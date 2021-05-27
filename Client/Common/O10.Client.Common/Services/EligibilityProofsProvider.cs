using O10.Client.Common.Configuration;
using O10.Client.Common.Interfaces;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.Cryptography;
using O10.Core.Logging;
using O10.Crypto.ConfidentialAssets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace O10.Client.Common.Services
{
    [RegisterDefaultImplementation(typeof(IEligibilityProofsProvider), Lifetime = LifetimeManagement.Singleton)]
    public class EligibilityProofsProvider : IEligibilityProofsProvider
    {
        private readonly ILogger _logger;
        private readonly IGatewayService _gatewayService;
        private readonly IRestApiConfiguration _restApiConfiguration;

        public EligibilityProofsProvider(
            ILoggerService loggerService,
            IGatewayService gatewayService,
            IConfigurationService configurationService)
        {
            if (loggerService is null)
            {
                throw new ArgumentNullException(nameof(loggerService));
            }

            _logger = loggerService.GetLogger(nameof(EligibilityProofsProvider));
            _gatewayService = gatewayService;
            _restApiConfiguration = configurationService.Get<IRestApiConfiguration>();
        }

        public async Task<SurjectionProof> CreateEligibilityProof(byte[] originalCommitment, byte[] originalBlindingFactor, byte[] assetCommitment, byte[] newBlindingFactor, Memory<byte> issuer)
        {
            byte[][] issuanceCommitments = await _gatewayService.GetIssuanceCommitments(issuer, _restApiConfiguration.RingSize + 1).ConfigureAwait(false);

            
            GetEligibilityCommitmentAndProofs(originalCommitment, issuanceCommitments, out int actualAssetPos, out byte[][] commitments);
            byte[] blindingFactorToEligibility = O10.Crypto.ConfidentialAssets.CryptoHelper.GetDifferentialBlindingFactor(newBlindingFactor, originalBlindingFactor);
            SurjectionProof eligibilityProof = O10.Crypto.ConfidentialAssets.CryptoHelper.CreateSurjectionProof(assetCommitment, commitments, actualAssetPos, blindingFactorToEligibility);
            bool res = O10.Crypto.ConfidentialAssets.CryptoHelper.VerifySurjectionProof(eligibilityProof, assetCommitment);
            return eligibilityProof;
        }

        public void GetEligibilityCommitmentAndProofs(byte[] ownedCommitment, byte[][] inputCommitments, out int actualAssetPos, out byte[][] outputCommitments)
        {
            if (inputCommitments is null)
            {
                throw new ArgumentNullException(nameof(inputCommitments));
            }

            Random random = new Random(BitConverter.ToInt32(ownedCommitment, 0));
            int totalAssets = inputCommitments.Length;
            byte[] commitment = ownedCommitment;
            actualAssetPos = random.Next(totalAssets);
            outputCommitments = new byte[totalAssets][];

            List<int> pickedPositions = new List<int>();

            for (int i = 0; i < totalAssets; i++)
            {
                if (i == actualAssetPos)
                {
                    outputCommitments[i] = commitment;
                }
                else
                {
                    bool completedLoop = false;

                    do
                    {
                        int randomPos = random.Next(totalAssets);

                        if (pickedPositions.Contains(randomPos))
                        {
                            continue;
                        }

                        Memory<byte> commitmentTmp = inputCommitments[randomPos];

                        if (commitment.AsSpan().SequenceEqual(commitmentTmp.Span))
                        {
                            continue;
                        }

                        outputCommitments[i] = commitmentTmp.ToArray();
                        completedLoop = true;
                        pickedPositions.Add(randomPos);
                    } while (!completedLoop);
                }
            }
        }
    }
}
