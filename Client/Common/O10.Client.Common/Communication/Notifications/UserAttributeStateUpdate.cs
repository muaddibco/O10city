using O10.Core.Identity;
using O10.Core.Notifications;

namespace O10.Client.Common.Communication.Notifications
{
    public class UserAttributeStateUpdate : NotificationBase
	{
        public byte[] Issuer { get; set; }
        public byte[] AssetId { get; set; }
		public byte[] BlindingFactor { get; set; }
		public IKey AssetCommitment { get; set; }
		public IKey TransactionKey { get; set; }
		public IKey DestinationKey { get; set; }
	}
}
