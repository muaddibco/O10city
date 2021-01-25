using O10.Core.Configuration;

namespace O10.Client.Mobile.Base.Services.Inherence
{
    public interface IO10InherenceConfiguration : IConfigurationSection
    {
        string Uri { get; set; }
    }
}
