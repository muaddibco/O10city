namespace O10.Client.Mobile.Base.Models.StateNotifications
{
    public class GroupRelationAddedStateNotification : StateNotificationBase
    {
        public GroupRelationAddedStateNotification(GroupRelationModel groupRelation)
        {
            GroupRelation = groupRelation;
        }

        public GroupRelationModel GroupRelation { get; }

        public override object Clone()
        {
            return new GroupRelationAddedStateNotification(new GroupRelationModel
            {
                AssetId = GroupRelation.AssetId,
                GroupName = GroupRelation.GroupName,
                GroupOwnerKey = GroupRelation.GroupOwnerKey,
                GroupOwnerName = GroupRelation.GroupOwnerName,
                GroupRelationId = GroupRelation.GroupRelationId,
                Issuer = GroupRelation.Issuer
            });
        }
    }
}
