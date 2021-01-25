using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using O10.Client.Mobile.Base.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: Dependency(typeof(O10Wallet.Droid.Services.AndroidSystemService))]
namespace O10Wallet.Droid.Services
{
    public class AndroidSystemService : IAndroidSystemService
    {
        private const string TAG = "AndroidSystemService";
        public const int ACTION_MANAGE_OVERLAY_PERMISSION_REQUEST_CODE = 1221;

        public static FormsAppCompatActivity Context { get; set; }
        public static TaskCompletionSource<bool> OverflowTaskCompletionSource { get; set; }

        public bool IsAutoStartPermissionAvailable()
        {
            return AutoStartPermissionHelper.IsAutoStartPermissionAvailable(Context);
        }

        public bool IsOverflowSettingsAllowed()
        {
            return IsFloatWindowOpAllowed(Context);
        }

        public void OpenAutoStartSettings()
        {
            AutoStartPermissionHelper.GetAutoStartPermission(Context);
        }

        public async Task<bool> OpenOverflowSettings()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                OverflowTaskCompletionSource = new TaskCompletionSource<bool>();
                var intent = new Intent(Android.Provider.Settings.ActionManageOverlayPermission, Android.Net.Uri.Parse("package:" + Context.PackageName));
                Context.StartActivityForResult(intent, ACTION_MANAGE_OVERLAY_PERMISSION_REQUEST_CODE);

                return await OverflowTaskCompletionSource.Task;
            }

            return await Task.FromResult(false);
        }

        public static bool IsFloatWindowOpAllowed(Context context)
        {
            return CheckOp(context, AppOpsManager.OpstrSystemAlertWindow);
        }

        public static bool CheckOp(Context context, string op)
        {
            AppOpsManager manager = (AppOpsManager)context.GetSystemService(Android.Content.Context.AppOpsService);
            try
            {
                return (AppOpsManagerMode.Allowed == manager.CheckOpNoThrow(op, Process.MyUid(), context.PackageName));
            }
            catch (Exception e)
            {
                Log.Error(TAG, e.Message);
            }

            return false;
        }
    }
}