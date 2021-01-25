using O10.Core.Architecture;

using O10.Core.Configuration;

namespace O10.Client.Web.Portal.Telegram.Configuration
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class TelegramConfiguration : ConfigurationSectionBase, ITelegramConfiguration
    {
        public const string NAME = "Telegram";

        public TelegramConfiguration(IAppConfig appConfig) : base(appConfig, NAME)
        {
        }

        public string ApiKeyName { get; set; }
    }
}
