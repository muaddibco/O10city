using Microsoft.Extensions.Configuration;
using IMicrosoftConfigurationSection = Microsoft.Extensions.Configuration.IConfigurationSection;
using O10.Core.Exceptions;
using System;
using System.Text.RegularExpressions;

namespace O10.Core.Configuration
{
    public class JsonAppConfig : IAppConfig
    {
        private readonly IConfiguration _configuration;

        public JsonAppConfig(IConfiguration configuration)
        {
            _configuration = configuration;
        }

		public bool GetBool(string key, bool required = true)
        {
            var section = GetConfigurationEntry(key, required);

            bool value;

            try
            {
                value = section.Get<bool>();
            }
            catch (Exception ex)
            {
                throw new ConfigurationParameterInvalidValueException(key, section.Value, "true or false", ex);
            }

            return value;
        }

        public long GetLong(string key, bool required = true)
		{
            var section = GetConfigurationEntry(key, required);
            long value;

            try
            {
                value = section.Get<long>();
            }
            catch (Exception ex)
            {
                throw new ConfigurationParameterInvalidValueException(key, section.Value, "numeric value", ex);
            }

            return value;
        }

        public string GetString(string key, bool required = true)
		{
            var section = GetConfigurationEntry(key, required);
            
            string value = section.Value;

			return value;
		}

        private IMicrosoftConfigurationSection GetConfigurationEntry(string key, bool required)
        {
            ExtractSectionAndProperty(key, out string sectionName, out string valueName);

            IMicrosoftConfigurationSection section = null;

            if (!string.IsNullOrEmpty(sectionName))
            {
                section = _configuration.GetSection(sectionName);

                if (!section.Exists() && required)
                {
                    throw new RequiredConfigurationParameterNotSpecifiedException(key);
                }
            }

            if (section == null)
            {
                section = _configuration.GetSection(valueName);
            }
            else
            {
                section = section.GetSection(valueName);
            }

            if (!section.Exists() && required)
            {
                throw new RequiredConfigurationParameterNotSpecifiedException(key);
            }

            return section;
        }

        private static void ExtractSectionAndProperty(string key, out string section, out string name)
        {
            string[] pair = key.Split(':');
			section = pair.Length > 1 ? pair[0] : null;
            name = pair.Length > 1 ? pair[1] : pair[0];
        }

        public string ReplaceToken(string src)
        {
            return Regex.Replace(src, @"{(\w+)}", (m) =>
            {
                var key = m.Groups[1].Value;

                return GetString(key, false);
            });
        }

    }
}
