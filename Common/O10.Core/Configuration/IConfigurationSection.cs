using O10.Core.Architecture;

namespace O10.Core.Configuration
{
    [ExtensionPoint]
    public interface IConfigurationSection
    {
        string SectionName { get; }

        void Initialize();
    }
}
