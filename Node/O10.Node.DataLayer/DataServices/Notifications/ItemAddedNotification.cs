using O10.Core.Notifications;
using O10.Node.DataLayer.DataServices.Keys;

namespace O10.Node.DataLayer.DataServices.Notifications
{
    public class ItemAddedNotification : SucceededNotification
    {
        public ItemAddedNotification(IDataKey dataKey)
        {
            DataKey = dataKey;
        }

        public IDataKey DataKey { get; }
    }
}
