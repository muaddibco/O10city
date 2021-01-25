using Android.App;
using Android.Content.PM;
using Android.OS;
using O10.Client.Mobile.Base;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using O10.Client.Mobile.Base.Interfaces;
using Xamarin.Forms;
using O10Wallet.Droid.Services;
using System.IO;
using log4net;
using System.Reflection;
using Android.Content;
using System;
using System.Threading.Tasks.Dataflow;
using Android.Util;
using Android.Gms.Common;
using Xamarin.Essentials;
using Android;
using Xamarin.Forms.Platform.Android.AppLinks;
using Microblink.Forms.Droid;
using Plugin.Fingerprint;
using System.Threading.Tasks;
using Firebase.Iid;

namespace O10Wallet.Droid
{
    [Activity(Label = "O10Id", Icon = "@mipmap/Icon", Theme = "@style/MainTheme", 
        MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity, Microblink.Forms.Droid.IMicroblinkScannerAndroidHostActivity
    {
        public const string TAG = "O10WalletMainActivity";
        internal static readonly string CHANNEL_ID = "my_notification_channel";

        private Intent _startServiceIntent;
        private Intent _stopServiceIntent;
        private bool _isStarted = false;
        private SynchronizerServiceConnection _synchronizerServiceConnection;
        private TransformBlock<ISynchronizerServiceBinder, ISynchronizerServiceBinder> _synchronizerConnected;

        public Activity HostActivity => this;
        public MicroblinkScannerImplementation ScannerImplementation { get; set; }

        protected override void OnCreate(Bundle bundle)
        {
            Log.Info(TAG, nameof(OnCreate));

            AppCenter.Start("8d5c4029-c35e-4900-8618-1928d913a2d0", typeof(Analytics), typeof(Crashes));
            Log.Info(TAG, "AppCenter started");

            CrossFingerprint.SetCurrentActivityResolver(() => this);
            Log.Info(TAG, "CrossFingerprint SetCurrentActivityResolver");

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);
            Log.Info(TAG, "base.OnCreate");

            MicroblinkScannerFactoryImplementation.AndroidHostActivity = this;

            OnNewIntent(Intent);
            Log.Info(TAG, "OnNewIntent");

            if (bundle != null)
            {
                _isStarted = bundle.GetBoolean(Constants.SERVICE_STARTED_KEY, false);
            }

            _startServiceIntent = new Intent(this, typeof(SynchronizerService));
            _startServiceIntent.SetAction(Constants.ACTION_START_SERVICE);

            _stopServiceIntent = new Intent(this, typeof(SynchronizerService));
            _stopServiceIntent.SetAction(Constants.ACTION_STOP_SERVICE);

            Rg.Plugins.Popup.Popup.Init(this, bundle);
            Platform.Init(this, bundle);
            ZXing.Net.Mobile.Forms.Android.Platform.Init();
            Plugin.Media.CrossMedia.Current.Initialize();
            Forms.Init(this, bundle);
            FormsMaterial.Init(this, bundle);
            ((NotificationService)DependencyService.Get<INotificationService>()).Context = this;

            AndroidAppLinks.Init(this);
            Stream log4netConfigStream = Assets.Open("log4net.xml");
            var repo = LogManager.GetRepository(Assembly.GetCallingAssembly());
            log4net.Config.XmlConfigurator.Configure(repo, log4netConfigStream);

            try
            {
                if (_synchronizerServiceConnection == null)
                {
                    _synchronizerServiceConnection = new SynchronizerServiceConnection(this);
                }

                StartService(_startServiceIntent);

                _isStarted = true;
            }
            catch (Exception ex)
            {
            }

            _synchronizerConnected = new TransformBlock<ISynchronizerServiceBinder, ISynchronizerServiceBinder>(s => s);

            if (Intent.Extras != null)
            {
                foreach (var key in Intent.Extras.KeySet())
                {
                    if (key != null)
                    {
                        var value = Intent.Extras.GetString(key);
                        Log.Debug(TAG, "Key: {0} Value: {1}", key, value);
                    }
                }
            }

            IsPlayServicesAvailable();
#if DEBUG
            // Force refresh of the token. If we redeploy the app, no new token will be sent but the old one will
            // be invalid.
            Task.Run(() =>
            {
                // This may not be executed on the main thread.
                Log.Info(TAG, "Before FirebaseInstanceId.Instance.DeleteInstanceId");
                FirebaseInstanceId.Instance.DeleteInstanceId();
                Log.Info(TAG, "After FirebaseInstanceId.Instance.DeleteInstanceId");
            });
#endif
            CreateNotificationChannel();

            RequestPermissions(new string[] { Manifest.Permission.Camera }, 1001);

            AndroidSystemService.Context = this;

            Log.Info(TAG, "Loading application...");
            LoadApplication(new App(_synchronizerConnected));
        }

        public override void OnBackPressed()
        {
            if (Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed))
            {
                // Do something if there are some pages in the `PopupStack`
            }
            else
            {
                // Do something if there are not any pages in the `PopupStack`
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (data != null)
            {
                ScannerImplementation.OnActivityResult(requestCode, resultCode, data);
            }

            if (requestCode == AndroidSystemService.ACTION_MANAGE_OVERLAY_PERMISSION_REQUEST_CODE)
            {
                AndroidSystemService.OverflowTaskCompletionSource.SetResult(AndroidSystemService.IsFloatWindowOpAllowed(this));
            }
        }

        protected override void OnStart()
        {
            BindService(_startServiceIntent, _synchronizerServiceConnection, Bind.AutoCreate);
            base.OnStart();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnNewIntent(Intent intent)
        {
            if (intent == null)
            {
                return;
            }

            var bundle = intent.Extras;
            if (bundle != null)
            {
                if (bundle.ContainsKey(Constants.SERVICE_STARTED_KEY))
                {
                    _isStarted = true;
                }
            }
        }

        protected override void OnStop()
        {
            UnbindService(_synchronizerServiceConnection);
            base.OnStop();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutBoolean(Constants.SERVICE_STARTED_KEY, _isStarted);
            base.OnSaveInstanceState(outState);
        }

        public void OnSynchronizerConnected(ISynchronizerServiceBinder synchronizerServiceBinder)
        {
            _synchronizerConnected.Post(synchronizerServiceBinder);
        }

        public bool IsPlayServicesAvailable()
        {
            Log.Info(TAG, "Checking Google Play service availability");

            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (resultCode != ConnectionResult.Success)
            {
                if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
                {
                    Log.Error(TAG, GoogleApiAvailability.Instance.GetErrorString(resultCode));
                }
                else
                {
                    Log.Error(TAG, "This device is not supported");
                    Finish();
                }
                return false;
            }

            Log.Info(TAG, "Google Play Services is available.");
            return true;
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }

            var channelName = CHANNEL_ID;
            var channelDescription = string.Empty;
            var channel = new NotificationChannel(CHANNEL_ID, channelName, NotificationImportance.Default)
            {
                Description = channelDescription
            };

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

        public void ScanningStarted(MicroblinkScannerImplementation implementation)
        {
            ScannerImplementation = implementation;
        }

        public int ScanActivityRequestCode => 101;
    }
}

