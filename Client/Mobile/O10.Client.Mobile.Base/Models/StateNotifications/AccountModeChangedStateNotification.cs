namespace O10.Client.Mobile.Base.Models.StateNotifications
{
    public class AccountModeChangedStateNotification : StateNotificationBase
    {
        public bool IsProtectionEnabled { get; set; }

        public override object Clone()
        {
            return new AccountModeChangedStateNotification
            {
                IsProtectionEnabled = IsProtectionEnabled
            };
        }
    }
}
