using System.Fabric;
using System.Fabric.Description;
using O10.Core.Configuration;
using O10.Core.Exceptions;

namespace O10.Node.ServiceFabric.WebApp
{
	public class ServiceFabricAppConfig : IAppConfig
	{
		private readonly ConfigurationPackage _configurationPackage;

		public ServiceFabricAppConfig(ConfigurationPackage configurationPackage)
		{
			_configurationPackage = configurationPackage;
		}

		public bool GetBool(string key, bool required = true)
		{
			ExtractSectionAndProperty(key, out string section, out string name);
			if (_configurationPackage.Settings.Sections.TryGetValue(section, out ConfigurationSection configurationSection))
			{
				if (configurationSection.Parameters.TryGetValue(name, out ConfigurationProperty configurationProperty))
				{
					string valueExpr = configurationProperty.Value;
					if (bool.TryParse(valueExpr, out bool value))
					{
						return value;
					}
				}
			}

			if (required)
			{
				throw new RequiredConfigurationParameterNotSpecifiedException(key);
			}

			return default;
		}

		public long GetLong(string key, bool required = true)
		{
			ExtractSectionAndProperty(key, out string section, out string name);
			if (_configurationPackage.Settings.Sections.TryGetValue(section, out ConfigurationSection configurationSection))
			{
				if (configurationSection.Parameters.TryGetValue(name, out ConfigurationProperty configurationProperty))
				{
					string valueExpr = configurationProperty.Value;
					if (long.TryParse(valueExpr, out long value))
					{
						return value;
					}
				}
			}

			if (required)
			{
				throw new RequiredConfigurationParameterNotSpecifiedException(key);
			}

			return default;
		}

		public string GetString(string key, bool required = true)
		{
			ExtractSectionAndProperty(key, out string section, out string name);
			if(_configurationPackage.Settings.Sections.TryGetValue(section, out ConfigurationSection configurationSection))
			{
				if (configurationSection.Parameters.TryGetValue(name, out ConfigurationProperty configurationProperty))
				{
					return configurationProperty.Value;
				}
			}

			if (required)
			{
				throw new RequiredConfigurationParameterNotSpecifiedException(key);
			}

			return default;
		}

		private void ExtractSectionAndProperty(string key, out string section, out string name)
		{
			string[] pair = key.Split(':');
			section = pair[0];
			name = pair[1];
		}
	}
}
