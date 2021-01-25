using O10.Core.Configuration;

namespace O10.Client.Web.Portal.Telegram.Configuration
{
    public interface ITelegramConfiguration : IConfigurationSection
    {
        string ApiKeyName { get; set; }
    }
}
