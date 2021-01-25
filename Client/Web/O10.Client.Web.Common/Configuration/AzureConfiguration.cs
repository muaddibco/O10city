using O10.Core.Architecture;

using O10.Core.Configuration;

namespace O10.Client.Web.Common.Configuration
{
	[RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
	public class AzureConfiguration : ConfigurationSectionBase, IAzureConfiguration
	{
		public const string SECTION_NAME = "azure";

		public AzureConfiguration(IAppConfig appConfig) : base(appConfig, SECTION_NAME)
		{
		}

		public string AzureADCertThumbprint { get; set; }
		public string KeyVaultName { get; set; }
		public string AzureADApplicationId { get; set; }
	}
}
