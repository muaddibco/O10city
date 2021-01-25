using O10.Core.Configuration;

namespace O10.Client.Mobile.Base.Services.EmbeddedIdPs
{
    public interface IO10IdpConfiguration : IConfigurationSection
    {
        string ApiUri { get; set; }
        string ConfirmationUri { get; set; }
    }
}
