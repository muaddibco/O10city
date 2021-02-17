using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using O10.Gateway.DataLayer.Model;
using O10.Gateway.DataLayer.Services.Inputs;
using O10.Core.Architecture;
using O10.Transactions.Core.Enums;

namespace O10.Gateway.DataLayer.Services
{
    [ServiceContract]
    public interface IDataAccessService
    {
        void Initialize(CancellationToken cancellationToken);
        void UpdateLastSyncBlock(ulong height, byte[] hash);
        bool GetLastSyncBlock(out ulong height, out byte[] hash);
        void StoreRegistryCombinedBlock(ulong height, byte[] content);
        void CutExcessedPackets(long combinedBlockHeight);
        void StoreRegistryFullBlock(ulong height, byte[] content);
        TaskCompletionSource<WitnessPacket> StoreWitnessPacket(ulong syncBlockHeight, long round, ulong combinedBlockHeight, LedgerType referencedPacketType, ushort referencedBlockType, byte[] referencedBodyHash, byte[] referencedDestinationKey, byte[] referencedDestinationKey2, byte[] referencedTransactionKey, byte[] referencedKeyImage);
        WitnessPacket GetWitnessPacket(long witnessPacketId);
        Dictionary<long, List<WitnessPacket>> GetWitnessPackets(long combinedBlockHeightStart, long combinedBlockHeightEnd);

		bool GetLastRegistryCombinedBlock(out ulong height, out byte[] content);

		/// <summary>
		/// Returns content of RegistryFulBlocks and height of corresponding CombinedBlocks
		/// </summary>
		/// <param name="heightStart"></param>
		/// <param name="heights"></param>
		/// <param name="contents"></param>
		/// <returns></returns>
		bool GetRegistryFullBlocks(ulong heightStart, out ulong[] heights, out byte[][][] contents);

        void StoreIncomingTransactionalBlock(StateIncomingStoreInput storeInput, byte[] groupId);
        void StoreIncomingTransitionTransactionalBlock(StateTransitionIncomingStoreInput storeInput, byte[] groupId, Span<byte> originatingCommitment);
        void StoreIncomingUtxoTransactionBlock(UtxoIncomingStoreInput storeInput);

        StatePacket GetTransactionalIncomingBlock(long witnessid);
        StealthPacket GetUtxoIncomingBlock(long witnessid);
        StealthPacket GetStealthPacket(long syncBlockHeight, long combinedRegistryBlockHeight, string hashString);

        int GetTotalUtxoOutputsAmount();
		
		byte[][] GetRootAttributeCommitments(byte[] issuer, int amount);
		StealthOutput[] GetOutputs(int amount);

        void StoreRootAttributeIssuance(Memory<byte> issuer, Memory<byte> issuanceCommitment, Memory<byte> rootCommitment, long combinedBlockHeight);
        void StoreAssociatedAttributeIssuance(Memory<byte> issuer, Memory<byte> issuanceCommitment, Memory<byte> rootIssuanceCommitment);

        bool CheckRootAttributeExist(byte[] issuer, byte[] issuanceCommitment);
        bool CheckRootAttributeWasValid(byte[] issuer, byte[] issuanceCommitment, long combinedBlockHeight);
        bool CheckAssociatedAtributeExist(Memory<byte>? issuer, Memory<byte> issuanceCommitment, Memory<byte> rootCommitment);

        void SetRootAttributesOverriden(Memory<byte> issuer, Memory<byte> issuanceCommitment, long combinedBlockHeight);

		void AddEmployeeRecord(Memory<byte> issuer, Memory<byte> registrationCommitment, Memory<byte> groupCommitment);
		void CancelEmployeeRecord(Memory<byte> issuer, Memory<byte> registrationCommitment);

		byte[] GetEmployeeRecordGroup(string issuer, string registrationCommitment);

		StatePacket GetTransactionBySourceAndHeight(string source, ulong blockHeight);

        void AddCompromisedKeyImage(string keyImage);
        bool GetIsKeyImageCompomised(string keyImage);
    }
}
