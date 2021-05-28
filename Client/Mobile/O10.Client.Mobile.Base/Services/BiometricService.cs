using Flurl;
using Flurl.Http;
using System.Linq;
using System.Threading.Tasks;
using O10.Transactions.Core.Ledgers.Stealth.Internal;
using O10.Client.Common.Configuration;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.AttributesScheme;
using O10.Client.DataLayer.Services;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.Cryptography;
using O10.Core.ExtensionMethods;
using O10.Crypto.ConfidentialAssets;
using O10.Client.Mobile.Base.Dtos;
using O10.Client.Mobile.Base.Interfaces;
using O10.Client.Mobile.Base.Models;

namespace O10.Client.Mobile.Base.Services
{
    [RegisterDefaultImplementation(typeof(IBiometricService), Lifetime = LifetimeManagement.Singleton)]
    public class BiometricService : IBiometricService
    {
        private readonly IExecutionContext _executionContext;
        private readonly IDataAccessService _dataAccessService;
        private readonly IRestClientService _restClientService;
        private readonly IAssetsService _assetsService;
        private readonly IConfigurationService _configurationService;
        private readonly IRestApiConfiguration _restApiConfiguration;

        public BiometricService(IExecutionContext executionContext, IDataAccessService dataAccessService,
            IRestClientService restClientService,
            IAssetsService assetsService, IConfigurationService configurationService)
        {
            _executionContext = executionContext;
            _dataAccessService = dataAccessService;
            _restClientService = restClientService;
            _assetsService = assetsService;
            _configurationService = configurationService;
            _restApiConfiguration = configurationService.Get<IRestApiConfiguration>();
        }

        public async Task<BiometricProof> CheckBiometrics(string imageContent, RootAttributeModel rootAttribute, byte[] bindingKey)
        {
            bool proceed = true;
            BiometricProof biometricProof = null;

            if (!string.IsNullOrEmpty(imageContent) && !string.IsNullOrEmpty(rootAttribute.Content))
            {
                var (schemeName, content) = _dataAccessService.GetUserAssociatedAttributes(_executionContext.AccountId, rootAttribute.Issuer).FirstOrDefault(t => t.schemeName == AttributesSchemes.ATTR_SCHEME_NAME_PASSPORTPHOTO);
                string sourceImage = content;

                if (string.IsNullOrEmpty(sourceImage))
                {
                    return new BiometricProof();
                }

                (BiometricPersonDataForSignatureDto biometricPersonDataForSignature, byte[] sourceImageBlindingFactor) = await GetInputDataForBiometricSignature(sourceImage, imageContent, rootAttribute.Issuer);

                BiometricSignedVerificationDto biometricSignedVerification = null;

                Url url = _restApiConfiguration.InherenceUri.AppendPathSegment("SignPersonFaceVerification");

                _restClientService.Request(url)
                    .PostJsonAsync(biometricPersonDataForSignature)
                    .ReceiveJson<BiometricSignedVerificationDto>()
                    .ContinueWith(t =>
                    {
                        if (t.IsCompleted && !t.IsFaulted)
                        {
                            proceed = true;
                            biometricSignedVerification = t.Result;
                        }
                        else
                        {
                            proceed = false;
                        }
                    }, TaskScheduler.Default)
                    .Wait();

                if (proceed)
                {
                    biometricProof = await GetBiometricProof(biometricPersonDataForSignature, biometricSignedVerification, rootAttribute, bindingKey, sourceImageBlindingFactor);
                }
            }

            return biometricProof;
        }

        private async Task<(BiometricPersonDataForSignatureDto signatureData, byte[] sourceImageBlindingFactor)> GetInputDataForBiometricSignature(string sourceImage, string targetImage, string issuer)
        {
            byte[] sourceImageAssetId = await _assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_PASSPORTPHOTO, sourceImage, issuer);
            byte[] sourceImageBlindingFactor = CryptoHelper.GetRandomSeed();
            byte[] sourceImageCommitment = CryptoHelper.GetAssetCommitment(sourceImageBlindingFactor, sourceImageAssetId);
            SurjectionProof surjectionProof = CryptoHelper.CreateNewIssuanceSurjectionProof(sourceImageCommitment, new byte[][] { sourceImageAssetId }, 0, sourceImageBlindingFactor);

            BiometricPersonDataForSignatureDto biometricPersonDataForSignature = new BiometricPersonDataForSignatureDto
            {
                ImageSource = sourceImage,
                ImageTarget = targetImage,
                SourceImageCommitment = sourceImageCommitment.ToHexString(),
                SourceImageProofCommitment = surjectionProof.AssetCommitments[0].ToHexString(),
                SourceImageProofSignatureE = surjectionProof.Rs.E.ToHexString(),
                SourceImageProofSignatureS = surjectionProof.Rs.S[0].ToHexString()
            };

            return (biometricPersonDataForSignature, sourceImageBlindingFactor);
        }

        private async Task<BiometricProof> GetBiometricProof(BiometricPersonDataForSignatureDto biometricPersonDataForSignature, BiometricSignedVerificationDto biometricSignedVerification, RootAttributeModel rootAttribute, byte[] bindingKey, byte[] sourceImageBlindingFactor)
        {
            byte[] assetId = await _assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_PASSPORTPHOTO, biometricPersonDataForSignature.ImageSource, rootAttribute.Issuer);
            _assetsService.GetBlindingPoint(bindingKey, rootAttribute.AssetId, out byte[] blindingPoint, out byte[] blindingFactor);

            byte[] photoIssuanceCommitment = _assetsService.GetCommitmentBlindedByPoint(assetId, blindingPoint);
            byte[] sourceImageCommitment = biometricPersonDataForSignature.SourceImageCommitment.HexStringToByteArray();
            byte[] diffBF = CryptoHelper.GetDifferentialBlindingFactor(sourceImageBlindingFactor, blindingFactor);
            SurjectionProof surjectionProof = CryptoHelper.CreateSurjectionProof(sourceImageCommitment, new byte[][] { photoIssuanceCommitment }, 0, diffBF);

            return new BiometricProof
            {
                BiometricCommitment = sourceImageCommitment,
                BiometricSurjectionProof = surjectionProof,
                VerifierPublicKey = biometricSignedVerification.PublicKey.HexStringToByteArray(),
                VerifierSignature = biometricSignedVerification.Signature.HexStringToByteArray()
            };
        }
    }
}
