﻿using O10.Core.Architecture;
using O10.Transactions.Core.Ledgers;
using System.Threading.Tasks.Dataflow;

namespace O10.Client.Web.Common.Services
{
    [ExtensionPoint]
    public interface IExternalUpdater
    {
        ITargetBlock<PacketBase> PipeIn { get; }

        void Initialize(long accountId);
    }
}
