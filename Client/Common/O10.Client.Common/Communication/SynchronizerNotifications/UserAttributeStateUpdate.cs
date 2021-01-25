namespace O10.Client.Common.Communication.SynchronizerNotifications
{
    public class UserAttributeStateUpdate : SynchronizerNotificationBase
	{
        public byte[] Issuer { get; set; }
        public byte[] AssetId { get; set; }
		public byte[] BlindingFactor { get; set; }
		public byte[] AssetCommitment { get; set; }
		public byte[] TransactionKey { get; set; }
		public byte[] DestinationKey { get; set; }
	}
}
