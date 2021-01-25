using O10.Core.Configuration;

namespace O10.Gateway.Common.Configuration
{
    public interface IModularityConfiguration : IConfigurationSection
    {
        [Optional]
        string[] Modules { get; set; }
    }
}
