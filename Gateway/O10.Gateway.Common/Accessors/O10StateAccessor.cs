using O10.Transactions.Core.Accessors;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Ledgers;
using System;
using System.Collections.Generic;
using System.Text;

namespace O10.Gateway.Common.Accessors
{
    public class O10StateAccessor : AccessorBase
    {
        public override LedgerType LedgerType => LedgerType.O10State;

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
