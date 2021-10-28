using O10.Core.Notifications;

namespace O10.Gateway.Common.Services.Results
{
    public class KeyImageViolatedNotification : NotificationBase
    {
        public byte[] ExistingHash { get; set; }
    }
}
