using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using O10Wallet.Droid.Services;

namespace O10Wallet.Droid
{
    //[BroadcastReceiver(Enabled = true, Exported = true, DirectBootAware = true)]
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { Intent.ActionBootCompleted, Intent.ActionLockedBootCompleted, "android.intent.action.QUICKBOOT_POWERON", "com.htc.intent.action.QUICKBOOT_POWERON" }, Priority = (int)IntentFilterPriority.HighPriority)]
    public class MyBootReceiver : BroadcastReceiver
    {
        const string TAG = "MyBootReceiver";

        public override void OnReceive(Context context, Intent intent)
        {
            Log.Debug(TAG, "MyBootReceiver.OnReceive");
            try
            {
                var startServiceIntent = new Intent(context, typeof(SynchronizerService));
                startServiceIntent.AddFlags(ActivityFlags.NewTask);
                startServiceIntent.SetAction(Constants.ACTION_START_SERVICE);
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    Application.Context.StartForegroundService(startServiceIntent);
                }
                else
                {
                    Application.Context.StartService(startServiceIntent);
                }

            }
            catch (System.Exception ex)
            {
                Log.Error(TAG, ex.Message);
            }        
        }
    }
}