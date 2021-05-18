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
using O10.Crypto.Models;
using O10.Core.Identity;

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

        /// <summary>
        /// Returns the height of the Aggregated Transactions Registry Packet that was at the time when the account's packet with specified height was registered
        /// This function will be obsolete - must be replaced by another one where the height of aggregated registration will be obtained using the hash of the account transaction
        /// </summary>
        /// <param name="accountPublicKey"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        Task<ulong> GetCombinedBlockByTransactionHash(byte[] accountPublicKey, byte[] transactionHash);

        Task<OutputModel[]> GetOutputs(int amount);
        Task<byte[][]> GetIssuanceCommitments(Memory<byte> issuer, int amount);

        Task<bool> IsRootAttributeValid(Memory<byte> issuer, Memory<byte> commitment);
        Task<bool> AreRootAttributesValid(Memory<byte> issuer, IEnumerable<Memory<byte>> commitments);
        Task<bool> AreAssociatedAttributesExist(Memory<byte> issuer, (Memory<byte> issuanceCommitment, Memory<byte> commitmenttoRoot)[] attrs);
        Task<bool> WasRootAttributeValid(IKey issuer, Memory<byte> commitment, long combinedBlockHeight);

        Task<byte[]> GetEmployeeRecordGroup(byte[] issuer, byte[] registrationCommitment);

        Task<IEnumerable<WitnessPackage>> GetWitnessesRange(long rangeStart, long rangeEnd);

        string GetNotificationsHubUri();

		Task<TransactionBase> GetTransaction(string source, byte[] transactionHash);

        Task<string> PushRelationProofSession(RelationProofsData relationProofSession);

        Task<RelationProofsData> PopRelationProofSession(string sessionKey);

		Task<byte[]> GetHashByKeyImage(byte[] keyImage);

        Task<bool> IsKeyImageCompromised(IKey keyImage);
        Task<IEnumerable<InfoMessage>> GetInfo();
    }
}
