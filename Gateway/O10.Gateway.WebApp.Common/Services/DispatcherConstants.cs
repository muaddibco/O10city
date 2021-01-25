namespace O10.Gateway.WebApp.Common.Services
{
    public static class DispatcherConstants
    {
        public static string[] SubscriptionTags { get; set; } = { "default" };

        public static string FullAccessConnectionString = "Endpoint=sb://o10demonotificationhub.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=iQP219EcbmviOD6s+L3761W8eBFs3EQgiKQnkobENKw=";

        public static string NotificationHubName = "O10Firebase";
    }
}
