namespace O10Wallet.Droid
{
	public static class Constants
	{
		public const string NOTIFICATION_CHANNEL_ID = "O10Wallet.notification.channel";
		public const string ACTION_START_SERVICE = "O10Wallet.action.START_SERVICE";
		public const string ACTION_STOP_SERVICE = "O10Wallet.action.STOP_SERVICE";
		public const string SERVICE_STARTED_KEY = "O10Wallet.service.STARTED";
		public const string ACTION_MAIN_ACTIVITY = "O10Wallet.action.MAIN_ACTIVITY";
		public const int SERVICE_RUNNING_NOTIFICATION_ID = 10000;
		public const int IDENTITY_REVOKED_NOTIFICATION_ID = 10001;
		public const int DELAY_BETWEEN_LOG_MESSAGES = 5000;

		public const string ListenConnectionString = "Endpoint=sb://o10demonotificationhub.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=oa1svMD+27qhttzeulFGbqezqu87nfNMjfgC3InD98o=";
		public const string NotificationHubName = "O10Firebase";
	}
}