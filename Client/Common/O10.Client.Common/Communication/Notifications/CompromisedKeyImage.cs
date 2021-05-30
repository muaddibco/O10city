using O10.Core.Identity;
using O10.Core.Notifications;

namespace O10.Client.Common.Communication.Notifications
{
    public class CompromisedKeyImage : NotificationBase
    {
        public IKey? KeyImage { get; set; }

        public IKey? TransactionKey { get; set; }

        public IKey? DestinationKey { get; set; }
        public IKey? Target { get; set; }
    }
}
