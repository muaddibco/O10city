namespace O10.Client.Mobile.Base.Models.StateNotifications
{
    public class KeyImageCorruptedStateNotification : StateNotificationBase
    {
        public KeyImageCorruptedStateNotification(byte[] keyImage)
        {
            KeyImage = keyImage;
        }

        public byte[] KeyImage { get; set; }

        public override object Clone()
        {
            return new KeyImageCorruptedStateNotification(KeyImage);
        }
    }
}
