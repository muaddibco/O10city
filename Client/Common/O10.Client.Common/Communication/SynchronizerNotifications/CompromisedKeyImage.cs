namespace O10.Client.Common.Communication.SynchronizerNotifications
{
    public class CompromisedKeyImage : SynchronizerNotificationBase
    {
        public byte[] KeyImage { get; set; }

        public byte[] TransactionKey { get; set; }

        public byte[] DestinationKey { get; set; }
        public byte[] Target { get; set; }
    }
}
