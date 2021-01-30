using System.Collections.Generic;

namespace O10.Client.Common.Communication.Notifications
{
    public class EligibilityCommitmentsDisabled : NotificationBase
    {
        public List<long> DisabledIds { get; set; }
    }
}
