﻿using O10.Core.Notifications;

namespace O10.Client.Common.Communication.Notifications
{
    public class NextKeyImage : NotificationBase
    {
        public byte[] KeyImage { get; set; }
    }
}
