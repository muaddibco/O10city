using O10.Transactions.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace O10.Transactions.Core.Interfaces
{
    public interface IReferencedPayloadHandler
    {
        public PacketType ReferencedPacketType { get; }
    }
}
