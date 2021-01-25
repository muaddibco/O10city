using O10.Client.Mobile.Base.Dtos;

namespace O10.Client.Mobile.Base.Services.Inherence
{
    public class VerificationResult
    {
        public BiometricSignedVerificationDto SignedVerification { get; set; }
        public string ErrorMessage { get; set; }
    }
}
