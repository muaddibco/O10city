﻿using O10.Core.Notifications;

namespace O10.Client.Common.Communication.Notifications
{
    public class RootAttributeIssuance : NotificationBase
    {
        public byte[] Issuer { get; set; }

        public byte[] IssuanceCommitment { get; set; }
    }
}
