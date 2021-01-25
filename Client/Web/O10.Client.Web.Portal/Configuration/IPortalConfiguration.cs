using O10.Core.Configuration;

namespace O10.Client.Web.Portal.Configuration
{
    public interface IPortalConfiguration : IConfigurationSection
    {
        string FacePersonGroupId { get; set; }
        bool DemoMode { get; set; }
        string IdentityProviderUri { get; set; }
        string ElectionCommitteeUri { get; set; }
    }
}
