using System.Globalization;
using System.Threading;
using Java.Util;
using O10.Client.Mobile.Base.Interfaces;
using O10.Client.Mobile.Base.Services;

namespace O10Wallet.Droid.Services
{
    public class LocaleAndroid : ILocale
    {
        //============================================================================
        //                                FUNCTIONS
        //============================================================================

        #region ============ PUBLIC FUNCTIONS =============  

        public CultureInfo GetCurrentCultureInfo()
        {
            var netLanguage = "en";
            var androidLocale = Locale.Default;

            netLanguage = AndroidToDotnetLanguage(androidLocale.ToString().Replace("_", "-"));

            // this gets called a lot - try/catch can be expensive so consider caching or something
            CultureInfo cultureInfo;
            try
            {
                cultureInfo = new CultureInfo(netLanguage);
            }
#pragma warning disable CS0168 // The variable 'e1' is declared but never used
            catch (CultureNotFoundException e1)
#pragma warning restore CS0168 // The variable 'e1' is declared but never used
            {
                // iOS locale not valid .NET culture (eg. "en-ES" : English in Spain)
                // fallback to first characters, in this case "en"
                try
                {
                    var fallback = ToDotnetFallbackLanguage(new PlatformCulture(netLanguage));
                    //DependencyService.Get<ILogger>().Info(netLanguage + " failed, trying " + fallback + " (" + e1.Message + ")");
                    cultureInfo = new CultureInfo(fallback);
                }
#pragma warning disable CS0168 // The variable 'e2' is declared but never used
                catch (CultureNotFoundException e2)
#pragma warning restore CS0168 // The variable 'e2' is declared but never used
                {
                    // iOS language not valid .NET culture, falling back to English
                    //DependencyService.Get<ILogger>().Info(netLanguage + " couldn't be set, using 'en' (" + e2.Message + ")");
                    cultureInfo = new CultureInfo("en");
                }
            }
            return cultureInfo;
        }

        public void SetLocale(CultureInfo cultureInfo)
        {
            Thread.CurrentThread.CurrentCulture = cultureInfo;

            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }

        #endregion

        #region ============ PRIVATE FUNCTIONS ============ 

        private string AndroidToDotnetLanguage(string androidLanguage)
        {
            //DependencyService.Get<ILogger>().Info("Android Language:" + androidLanguage);
            var netLanguage = androidLanguage;

            //certain languages need to be converted to CultureInfo equivalent
            switch (androidLanguage)
            {
                case "ms-BN":   // "Malaysian (Brunei)" not supported .NET culture
                case "ms-MY":   // "Malaysian (Malaysia)" not supported .NET culture
                case "ms-SG":   // "Malaysian (Singapore)" not supported .NET culture
                    netLanguage = "ms"; // closest supported
                    break;
                case "in-ID":  // "Indonesian (Indonesia)" has different code in  .NET 
                    netLanguage = "id-ID"; // correct code for .NET
                    break;
                case "gsw-CH":  // "Schwiizertüütsch (Swiss German)" not supported .NET culture
                    netLanguage = "de-CH"; // closest supported
                    break;
                case "iw-IL":
                case "IL":
                    netLanguage = "he-IL";
                    break;
                    // add more application-specific cases here (if required)
                    // ONLY use cultures that have been tested and known to work
            }
            //DependencyService.Get<INotificationService>().ShowMessage(".NET Language/Locale:" + netLanguage);

            return netLanguage;
        }
        private string ToDotnetFallbackLanguage(PlatformCulture platformCulture)
        {
            var netLanguage = platformCulture.LanguageCode; // use the first part of the identifier (two chars, usually);

            switch (platformCulture.LanguageCode)
            {
                case "gsw":
                    netLanguage = "de-CH"; // equivalent to German (Switzerland) for this app
                    break;
                    // add more application-specific cases here (if required)
                    // ONLY use cultures that have been tested and known to work
            }
            return netLanguage;
        }

        #endregion

    }
}