using log4net;
using System.Configuration;
using System.Reflection;
using O10.Core.Architecture;

using O10.Core.Exceptions;
using System.Text.RegularExpressions;

namespace O10.Core.Configuration
{
    [RegisterDefaultImplementation(typeof(IAppConfig), Lifetime = LifetimeManagement.Singleton)]
    public class AppConfig : IAppConfig
    {
        private readonly ILog _log = LogManager.GetLogger(Assembly.GetCallingAssembly(), typeof(AppConfig));

        public AppConfig()
        {
            _log.Info($"{GetType().FullName} ctor");
            _log.Info($"Configured settings are: {string.Join(",", ConfigurationManager.AppSettings.AllKeys)}");
        }

        public bool GetBool(string key, bool required = true)
        {
            _log.Info($"Getting setting {key}");
            string value = ConfigurationManager.AppSettings.Get(key);

            if (string.IsNullOrEmpty(value))
            {
                if (required)
                    throw new RequiredConfigurationParameterNotSpecifiedException(key);

                return false;
            }

            bool bValue;

            if (!bool.TryParse(value, out bValue))
                throw new ConfigurationParameterInvalidValueException(key, value, "true or false");

			_log.Info($"{key} = {bValue}");

			return bValue;
        }

        public long GetLong(string key, bool required = true)
        {
            _log.Info($"Getting setting {key}");
            string value = ConfigurationManager.AppSettings.Get(key);

            if (string.IsNullOrEmpty(value))
            {
                if (required)
                    throw new RequiredConfigurationParameterNotSpecifiedException(key);

                return 0;
            }
            long lValue;

            if (!long.TryParse(value, out lValue))
                throw new ConfigurationParameterInvalidValueException(key, value, "numeric value");

			_log.Info($"{key} = {lValue}");

			return lValue;
        }

        public string GetString(string key, bool required = true)
        {
            _log.Info($"Getting setting {key}");
            string value = ConfigurationManager.AppSettings.Get(key);

            if (string.IsNullOrEmpty(value))
            {
                if (required)
                    throw new RequiredConfigurationParameterNotSpecifiedException(key);

                return null;
            }

			_log.Info($"{key} = {value}");

			return value;
        }

        public string ReplaceToken(string src)
        {
            return Regex.Replace(src, @"{(\w+)}", (m) =>
            {
                var key = m.Groups[1].Value;

                return GetString(key);
            });
        }
    }
}
