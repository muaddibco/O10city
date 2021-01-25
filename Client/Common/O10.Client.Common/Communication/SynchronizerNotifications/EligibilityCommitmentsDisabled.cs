using System.Collections.Generic;

namespace O10.Client.Common.Communication.SynchronizerNotifications
{
    public class EligibilityCommitmentsDisabled : SynchronizerNotificationBase
    {
        public List<long> DisabledIds { get; set; }
    }
}
