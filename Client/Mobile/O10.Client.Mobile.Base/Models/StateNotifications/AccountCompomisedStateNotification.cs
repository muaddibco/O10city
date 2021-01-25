namespace O10.Client.Mobile.Base.Models.StateNotifications
{
    public class AccountCompomisedStateNotification : StateNotificationBase
    {
        public byte[] KeyImage { get; set; }

        public byte[] Target { get; set; }

        public override object Clone()
        {
            return new AccountCompomisedStateNotification
            {
                KeyImage = (byte[])KeyImage.Clone(),
                Target = (byte[])Target.Clone()
            };
        }
    }
}
