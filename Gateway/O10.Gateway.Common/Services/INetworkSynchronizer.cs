using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using O10.Core.Architecture;
using O10.Core.Models;
using O10.Transactions.Core.DTOs;
using O10.Transactions.Core.Interfaces;
using O10.Transactions.Core.Ledgers;

namespace O10.Gateway.Common.Services
{
    [ServiceContract]
    public interface INetworkSynchronizer : ISyncStateProvider
    {
        DateTime LastSyncTime { get; set; }

		IPropagatorBlock<WitnessPackage, WitnessPackage> PipeOut { get; set; }

        ITargetBlock<TaskCompletionWrapper<IPacketBase>> PipeIn { get; }

		void SendPacket(TaskCompletionWrapper<IPacketBase> packetWrapper);

        void Initialize(CancellationToken cancellationToken);

        void Start();

		//TransactionalBlockEssense GetLastBlock(byte[] key);

		Task<IEnumerable<WitnessPackage>> GetWitnessRange(long combinedBlockHeightStart, long combinedBlockHeightEnd = 0);

		Task ProcessRtPackage(RtPackage rtPackage);

        Task<List<InfoMessage>> GetConnectedNodesInfo();

        TaskCompletionSource<bool> GetConnectivityCheckAwaiter(int nonce);
        void ConnectivityCheckSet(int nonce);
    }
}