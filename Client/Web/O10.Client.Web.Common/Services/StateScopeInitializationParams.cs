using O10.Client.Common.Services;

namespace O10.Client.Web.Common.Services
{
    public class StateScopeInitializationParams : ScopeInitializationParams
    {
        internal StateScopeInitializationParams()
        {

        }

        public byte[] SecretKey { get; set; }
    }
}
