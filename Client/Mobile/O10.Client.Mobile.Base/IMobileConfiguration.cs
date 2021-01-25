using O10.Core.Configuration;

namespace O10.Client.Mobile.Base
{
    public interface IMobileConfiguration : IConfigurationSection
    {
        bool IsSimulator { get; set; }

        bool IsEmulated { get; set; }
    }
}
