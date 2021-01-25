using System;

namespace O10.Client.Mobile.Base.Models.StateNotifications
{
    public abstract class StateNotificationBase : ICloneable
    {
        public abstract object Clone();
    }
}
