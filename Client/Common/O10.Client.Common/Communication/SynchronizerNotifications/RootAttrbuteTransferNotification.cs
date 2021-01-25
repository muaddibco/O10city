namespace O10.Client.Common.Communication.SynchronizerNotifications
{
    public class RootAttrbuteTransferNotification : SynchronizerNotificationBase
    {
        public byte[] Issuer { get; set; }

        public byte[] IssuanceCommitment { get; set; }
    }
}
