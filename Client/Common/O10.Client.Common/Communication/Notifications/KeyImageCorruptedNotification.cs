namespace O10.Client.Common.Communication.Notifications
{
    public class KeyImageCorruptedNotification : NotificationBase
    {
        public byte[] KeyImage { get; set; }

        public byte[] ExistingHash { get; set; }
    }
}
