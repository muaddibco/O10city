using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.Ledgers;

namespace O10.Server.IdentityProvider.Common.Services
{
    public class ClientUpdater
	{
		public ITargetBlock<PacketBase> PipeIn { get; }
	}
}
