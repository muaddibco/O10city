using O10.Core.Configuration;

namespace O10.Gateway.Common.Configuration
{
    public interface ISecretConfiguration : IConfigurationSection
    {
        public string SecretName { get; set; }
    }
}
