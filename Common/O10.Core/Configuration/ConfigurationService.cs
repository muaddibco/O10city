using System;
using System.Collections.Generic;
using System.Linq;
using O10.Core.Architecture;


namespace O10.Core.Configuration
{
    [RegisterDefaultImplementation(typeof(IConfigurationService), Lifetime = LifetimeManagement.Singleton)]
    public class ConfigurationService : IConfigurationService
    {
        private readonly IEnumerable<IConfigurationSection> _configurationSections;

        public ConfigurationService(IEnumerable<IConfigurationSection> configurationSections)
        {
            _configurationSections = configurationSections;
        }

        public IConfigurationSection this[string sectionName]
        {
            get
            {
                IConfigurationSection configurationSection = _configurationSections.FirstOrDefault(s => s.SectionName.Equals(sectionName, StringComparison.InvariantCultureIgnoreCase));
                configurationSection.Initialize();
                return configurationSection;
            }
        }

        public T Get<T>() where T : class, IConfigurationSection
        {
            IConfigurationSection configurationSection = _configurationSections.FirstOrDefault(s => s is T);
            configurationSection?.Initialize();
            return (configurationSection as T) ?? default(T);
        }
    }
}
