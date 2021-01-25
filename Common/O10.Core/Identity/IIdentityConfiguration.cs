using O10.Core.Configuration;

namespace O10.Core.Identity
{
    public interface IIdentityConfiguration : IConfigurationSection
    {
        string Provider { get; set; }
    }
}
