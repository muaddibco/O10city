using System;

namespace O10.Client.Common.Communication.Notifications
{
    public class FailedNotification : NotificationBase
    {
        public FailedNotification(Exception ex = null)
        {
            Exception = ex;
        }

        public Exception Exception { get; }
    }
}
