namespace O10.Client.Common.Communication.SynchronizerNotifications
{
    public class RootAttributeIssuance : SynchronizerNotificationBase
    {
        public byte[] Issuer { get; set; }

        public byte[] IssuanceCommitment { get; set; }
    }
}
