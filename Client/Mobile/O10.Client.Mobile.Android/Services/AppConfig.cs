using log4net;
using System.Reflection;
using O10.Core.Exceptions;
using O10Wallet.Droid.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(AppConfig))]
namespace O10Wallet.Droid.Services
{
    public class AppConfig : O10.Core.Configuration.IAppConfig
    {
        private readonly ILog _log = LogManager.GetLogger(Assembly.GetCallingAssembly(), typeof(AppConfig));

        public AppConfig()
        {
            _log.Info($"{GetType().FullName} ctor");
        }

        public bool GetBool(string key, bool required = true)
        {
            _log.Info($"Getting setting {key}");
            string value = AppSettingsManager.Settings[key];
            _log.Info($"Setting {key} = {value}");

            if (string.IsNullOrEmpty(value))
            {
                if (required)
                    throw new RequiredConfigurationParameterNotSpecifiedException(key);

                return false;
            }

            bool bValue;

            if (!bool.TryParse(value, out bValue))
                throw new ConfigurationParameterInvalidValueException(key, value, "true or false");

            return bValue;
        }

        public long GetLong(string key, bool required = true)
        {
            _log.Info($"Getting setting {key}");
            string value = AppSettingsManager.Settings[key];
            _log.Info($"Setting {key} = {value}");

            if (string.IsNullOrEmpty(value))
            {
                if (required)
                    throw new RequiredConfigurationParameterNotSpecifiedException(key);

                return 0;
            }

            if (!long.TryParse(value, out long lValue))
                throw new ConfigurationParameterInvalidValueException(key, value, "numeric value");

            return lValue;
        }

        public string GetString(string key, bool required = true)
        {
            _log.Info($"Getting setting {key}");
            string value = AppSettingsManager.Settings[key];
            _log.Info($"Setting {key} = {value}");

            if (string.IsNullOrEmpty(value))
            {
                if (required)
                    throw new RequiredConfigurationParameterNotSpecifiedException(key);

                return null;
            }

            return value;
        }
    }
}