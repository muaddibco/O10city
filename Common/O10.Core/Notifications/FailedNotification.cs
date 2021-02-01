using System;

namespace O10.Core.Notifications
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
