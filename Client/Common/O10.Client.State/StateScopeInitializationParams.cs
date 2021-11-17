using O10.Client.Common.Services;

namespace O10.Client.State
{
    public class StateScopeInitializationParams : ScopeInitializationParams
    {
        public StateScopeInitializationParams()
        {

        }

        public byte[] SecretKey { get; set; }
    }
}
