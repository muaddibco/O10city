namespace O10.Client.Common.Communication.SynchronizerNotifications
{
    public class AssociatedAttributeIssuance : SynchronizerNotificationBase
    {
        public byte[] Issuer { get; set; }

        public byte[] GroupId { get; set; }

        public byte[] IssuanceCommitment { get; set; }

        public byte[] RootAttributeCommitment { get; set; }
    }
}
