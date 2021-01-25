using O10.Core.Configuration;

namespace O10.Client.Common.Configuration
{
    public interface IModularityConfiguration : IConfigurationSection
    {
        [Optional]
        string[] Modules { get; set; }
    }
}
