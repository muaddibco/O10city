using O10.Core.Models;
using O10.Transactions.Core.Accessors;
using O10.Transactions.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace O10.Gateway.Common.Accessors
{
    public class O10StateAccessor : AccessorBase
    {
        public override LedgerType PacketType => LedgerType.O10State;

        protected override IEnumerable<string> GetAccessingKeys()
        {
            throw new NotImplementedException();
        }

        protected override PacketBase GetPacketInner(EvidenceDescriptor accessDescriptor)
        {
            throw new NotImplementedException();
        }
    }
}
