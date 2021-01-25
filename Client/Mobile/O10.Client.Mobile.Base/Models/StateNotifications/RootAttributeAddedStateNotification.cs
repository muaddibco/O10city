namespace O10.Client.Mobile.Base.Models.StateNotifications
{
    public class RootAttributeAddedStateNotification : StateNotificationBase
    {
        public RootAttributeAddedStateNotification(UserAttributeModel model)
        {
            Attribute = model;
        }

        public UserAttributeModel Attribute { get; }

        public override object Clone()
        {
            return new RootAttributeAddedStateNotification(
                new UserAttributeModel
                {
                    AssetId = Attribute.AssetId,
                    SchemeName = Attribute.SchemeName,
                    Content = Attribute.Content,
                    IsOverriden = Attribute.IsOverriden,
                    LastBlindingFactor = Attribute.LastBlindingFactor,
                    LastCommitment = Attribute.LastCommitment,
                    LastDestinationKey = Attribute.LastDestinationKey,
                    LastTransactionKey = Attribute.LastTransactionKey,
                    OriginalBlindingFactor = Attribute.OriginalBlindingFactor,
                    OriginalCommitment = Attribute.OriginalCommitment,
                    OriginatingCommitment = Attribute.OriginatingCommitment,
                    Source = Attribute.Source,
                    UserAttributeId = Attribute.UserAttributeId,
                    Validated = Attribute.Validated
                });
        }
    }
}
