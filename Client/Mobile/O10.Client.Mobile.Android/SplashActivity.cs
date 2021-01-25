using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Util;

namespace O10Wallet.Droid
{
    [Activity(Theme = "@style/MyTheme.Splash", Label = "O10 Id",
        MainLauncher = true, NoHistory = true,
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    [IntentFilter(new[] { Intent.ActionView },
              Categories = new[] { Intent.ActionView, Intent.CategoryBrowsable, Intent.CategoryDefault },
              DataScheme = "http",
              DataHost = "o10demo.azurewebsites.net",
              DataPathPrefix = "/idpregconfirm",
              AutoVerify = true)]
    [IntentFilter(new[] { Intent.ActionView },
              Categories = new[] { Intent.ActionView, Intent.CategoryBrowsable, Intent.CategoryDefault },
              DataScheme = "https",
              DataHost = "o10demo.azurewebsites.net",
              DataPathPrefix = "/idpregconfirm",
              AutoVerify = true)]
    public class SplashActivity : AppCompatActivity
    {
        static readonly string TAG = "X:" + typeof(SplashActivity).Name;

        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState);
            Log.Debug(TAG, "SplashActivity.OnCreate");

            //string action = Intent.Action;
            //string strLink = Intent.DataString;
            //Intent intent = new Intent(Application.Context, typeof(MainActivity));
            //if (Intent.ActionView == action && !string.IsNullOrWhiteSpace(strLink))
            //{
            //    intent.SetAction(Intent.ActionView);
            //    intent.SetData(Intent.Data);
            //}
            //StartActivity(intent);
            //Finish();
        }

        // Launches the startup task
        protected override void OnResume()
        {
            base.OnResume();
            //StartActivity(new Intent(Application.Context, typeof(MainActivity)));

            string action = Intent.Action;
            string strLink = Intent.DataString;
            Intent intent = new Intent(Application.Context, typeof(MainActivity));
            if (Intent.ActionView == action && !string.IsNullOrWhiteSpace(strLink))
            {
                intent.SetAction(Intent.ActionView);
                intent.SetData(Intent.Data);
            }
            StartActivity(intent);
            Finish();
        }

        // Prevent the back button from canceling the startup process
        public override void OnBackPressed() { }
    }
}