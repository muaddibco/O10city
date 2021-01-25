using Android.Util;
using Firebase.Messaging;
using Android.Support.V4.App;
using WindowsAzure.Messaging;
using Android.App;
using Android.Content;
using System.Linq;
using System.Collections.Generic;
using O10Wallet.Droid.Services;

namespace O10Wallet.Droid
{
	[Service]
	[IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
	[IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
	public class MyFirebaseMessagingService : FirebaseMessagingService
	{
        public MyFirebaseMessagingService()
        {
            Log.Info(TAG, $"ctor {nameof(MyFirebaseMessagingService)}");
        }

        const string TAG = "MyFirebaseMsgService";
        NotificationHub hub;

        public override void OnMessageReceived(RemoteMessage message)
        {
            Log.Debug(TAG, "From: " + message.From);

            string notificationMessage = GetNotificationMessage(message);

            Intent startServiceIntent = new Intent(this, typeof(SynchronizerService));
            startServiceIntent.SetAction(Constants.ACTION_START_SERVICE);
            startServiceIntent.PutExtra("msg", notificationMessage);

            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                StartForegroundService(startServiceIntent);
            }
            else
            {
                StartService(startServiceIntent);
            }

            SendNotification(notificationMessage);
        }

        private static string GetNotificationMessage(RemoteMessage message)
        {
            if (message.GetNotification() != null)
            {
                //These is how most messages will be received
                Log.Debug(TAG, "Notification Message Body: " + message.GetNotification().Body);
                return message.GetNotification().Body;
            }
            else
            {
                //Only used for debugging payloads sent from the Azure portal
                return message.Data.Values.First();
            }
        }

        private void SendNotification(string messageBody)
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.OneShot);

            var notificationBuilder = new NotificationCompat.Builder(this, MainActivity.CHANNEL_ID);

            notificationBuilder.SetContentTitle("FCM Message")
                        .SetSmallIcon(Resource.Drawable.ic_sync)
                        .SetContentText(messageBody)
                        .SetAutoCancel(true)
                        .SetShowWhen(false)
                        .SetContentIntent(pendingIntent);

            var notificationManager = NotificationManager.FromContext(ApplicationContext);

            notificationManager.Notify(123, notificationBuilder.Build());
        }

        public override void OnNewToken(string token)
        {
            Log.Info(TAG, "FCM token: " + token);
            SendRegistrationToServer(token);
        }

        private void SendRegistrationToServer(string token)
        {
            // Register with Notification Hubs
            hub = new NotificationHub(Constants.NotificationHubName,
                                        Constants.ListenConnectionString, this);

            var tags = new List<string>();
            var regID = hub.Register(token, tags.ToArray()).RegistrationId;

            Log.Debug(TAG, $"Successful registration of ID {regID}");
        }
    }
}