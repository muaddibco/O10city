using O10.Core.Configuration;

namespace O10.Server.IdentityProvider.Common.Configuration
{
    public interface IO10IdpConfiguration : IConfigurationSection
    {
        long SessionTimeout { get; set; }
    }
}
