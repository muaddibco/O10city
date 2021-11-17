using O10.Client.Common.Services;

namespace O10.Client.Stealth
{
    public class StealthScopeInitializationParams : ScopeInitializationParams
    {
        public StealthScopeInitializationParams()
        {

        }

        public byte[] SecretSpendKey { get; set; }
        public byte[] SecretViewKey { get; set; }
        public byte[] PwdSecretKey { get; set; }
    }
}
