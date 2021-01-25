namespace O10.Client.Common.Communication.SynchronizerNotifications
{
    public class KeyImageCorruptedNotification : SynchronizerNotificationBase
    {
        public byte[] KeyImage { get; set; }

        public byte[] ExistingHash { get; set; }
    }
}
