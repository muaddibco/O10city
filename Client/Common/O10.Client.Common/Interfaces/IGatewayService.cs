using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.Interfaces;
using O10.Core.Architecture;
using O10.Core.Models;
using O10.Client.Common.Interfaces.Inputs;
using System.Threading.Tasks;
using O10.Core.Notifications;
using O10.Transactions.Core.DTOs;
using O10.Transactions.Core.Ledgers;

namespace O10.Client.Common.Interfaces
{
    [ServiceContract]
    public interface IGatewayService : ISyncStateProvider
	{
        /// <summary>
        /// Pipeline for sending transactions to gateways
        /// </summary>
		ITargetBlock<TaskCompletionWrapper<IPacketBase>> PipeInTransactions { get; }
        ISourceBlock<NotificationBase> PipeOutNotifications { get; }


        bool Initialize(string gatewayUri, CancellationToken cancellationToken);

        Task<ulong> GetCombinedBlockByAccountHeight(byte[] accountPublicKey, ulong height);

        Task<OutputModel[]> GetOutputs(int amount);
        Task<byte[][]> GetIssuanceCommitments(Memory<byte> issuer, int amount);

        Task<bool> IsRootAttributeValid(Memory<byte> issuer, Memory<byte> commitment);
        Task<bool> AreRootAttributesValid(Memory<byte> issuer, IEnumerable<Memory<byte>> commitments);
        Task<bool> AreAssociatedAttributesExist(Memory<byte> issuer, (Memory<byte> issuanceCommitment, Memory<byte> commitmenttoRoot)[] attrs);
        Task<bool> WasRootAttributeValid(byte[] issuer, byte[] commitment, long combinedBlockHeight);

        Task<byte[]> GetEmployeeRecordGroup(byte[] issuer, byte[] registrationCommitment);

        Task<IEnumerable<WitnessPackage>> GetWitnessesRange(ulong rangeStart, ulong rangeEnd);

        string GetNotificationsHubUri();

		Task<PacketInfo> GetTransactionBySourceAndHeight(string source, ulong height);

        Task<string> PushRelationProofSession(RelationProofsData relationProofSession);

        Task<RelationProofsData> PopRelationProofSession(string sessionKey);

		Task<byte[]> GetHashByKeyImage(byte[] keyImage);

        Task<bool> IsKeyImageCompromised(byte[] keyImage);
        Task<IEnumerable<InfoMessage>> GetInfo();
    }
}
