using O10.Core.Notifications;

namespace O10.Client.Common.Communication.Notifications
{
    public class AssociatedAttributeIssuance : NotificationBase
    {
        public byte[] Issuer { get; set; }

        public byte[] GroupId { get; set; }

        public byte[] IssuanceCommitment { get; set; }

        public byte[] RootAttributeCommitment { get; set; }
    }
}
