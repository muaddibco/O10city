namespace O10.Client.Mobile.Base.Models.StateNotifications
{
    public class RootAttributeDisabledStateNotification : StateNotificationBase
    {
        public RootAttributeDisabledStateNotification(long attributeId)
        {
            AttributeId = attributeId;
        }

        public long AttributeId { get; }

        public override object Clone()
        {
            return new RootAttributeDisabledStateNotification(AttributeId);
        }
    }
}
