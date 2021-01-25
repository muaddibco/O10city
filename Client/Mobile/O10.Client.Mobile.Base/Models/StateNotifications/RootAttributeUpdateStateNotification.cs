namespace O10.Client.Mobile.Base.Models.StateNotifications
{
    public class RootAttributeUpdateStateNotification : StateNotificationBase
    {
        public RootAttributeUpdateStateNotification(UserAttributeLastUpdateModel lastUpdateModel)
        {
            UserAttributeLastUpdate = lastUpdateModel;
        }

        public UserAttributeLastUpdateModel UserAttributeLastUpdate { get; }

        public override object Clone()
        {
            return new RootAttributeUpdateStateNotification(
                new UserAttributeLastUpdateModel
                {
                    AssetId = UserAttributeLastUpdate.AssetId,
                    LastBlindingFactor = UserAttributeLastUpdate.LastBlindingFactor,
                    LastCommitment = UserAttributeLastUpdate.LastCommitment,
                    LastDestinationKey = UserAttributeLastUpdate.LastDestinationKey,
                    LastTransactionKey = UserAttributeLastUpdate.LastTransactionKey
                });
        }
    }
}
