using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using log4net;
using Newtonsoft.Json.Linq;

namespace O10Wallet.Droid.Services
{
    public class AppSettingsManager
    {
        private static AppSettingsManager _instance;
        private JObject _secrets;
        private readonly ILog _log = LogManager.GetLogger(Assembly.GetCallingAssembly(), typeof(AppSettingsManager));

        private const string _fileName = "appsettings.json";

        private AppSettingsManager()
        {
            try
            {
                var stream = Application.Context.Assets.Open(_fileName);
                using (var reader = new StreamReader(stream))
                {
                    var json = reader.ReadToEnd();
                    _secrets = JObject.Parse(json);
                }
            }
            catch (Exception ex)
            {
                _log.Error("Unable to load secrets file", ex);
                throw;
            }
        }

        public static AppSettingsManager Settings
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AppSettingsManager();
                }

                return _instance;
            }
        }

        public string this[string name]
        {
            get
            {
                try
                {
                    var path = name.Split(':');

                    JToken node = _secrets[path[0]];
                    for (int index = 1; index < path.Length; index++)
                    {
                        node = node[path[index]];
                    }

                    return node?.ToString() ?? string.Empty;
                }
                catch (Exception ex)
                {
                    _log.Error($"Unable to retrieve secret '{name}'", ex);
                    return string.Empty;
                }
            }
        }
    }
}