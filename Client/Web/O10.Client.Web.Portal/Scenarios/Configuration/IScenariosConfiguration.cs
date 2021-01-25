using O10.Core.Configuration;

namespace O10.Client.Web.Portal.Scenarios.Configuration
{
    public interface IScenariosConfiguration : IConfigurationSection
    {
        string FolderPath { get; set; }
        string ContentBasePath { get; set; }
    }
}
