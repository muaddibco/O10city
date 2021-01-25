using O10.Core.Architecture;

namespace O10.Core.Configuration
{
    [ServiceContract]
    public interface IConfigurationService
    {
        IConfigurationSection this[string sectionName] { get; }

        T Get<T>() where T: class, IConfigurationSection;
    }
}
