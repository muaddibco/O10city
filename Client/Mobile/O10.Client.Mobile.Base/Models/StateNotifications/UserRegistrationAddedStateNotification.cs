namespace O10.Client.Mobile.Base.Models.StateNotifications
{
    public class UserRegistrationAddedStateNotification : StateNotificationBase
    {
        public UserRegistrationAddedStateNotification(UserRegistrationModel userRegistration)
        {
            UserRegistration = userRegistration;
        }

        public UserRegistrationModel UserRegistration { get; set; }

        public override object Clone()
        {
            return new UserRegistrationAddedStateNotification(new UserRegistrationModel
            {
                AssetId = UserRegistration.AssetId,
                Commitment = UserRegistration.Commitment,
                Issuer = UserRegistration.Commitment,
                UserRegistrationId = UserRegistration.UserRegistrationId
            });
        }
    }
}
