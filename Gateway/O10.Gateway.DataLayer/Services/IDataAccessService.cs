using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using O10.Gateway.DataLayer.Model;
using O10.Gateway.DataLayer.Services.Inputs;
using O10.Core.Architecture;
using O10.Transactions.Core.Enums;
using O10.Core.Identity;

namespace O10.Gateway.DataLayer.Services
{
    [ServiceContract]
    public interface IDataAccessService
    {
        void Initialize(CancellationToken cancellationToken);
        void UpdateLastSyncBlock(long height, IKey hash);
        bool GetLastSyncBlock(out ulong height, out byte[] hash);
        void StoreRegistryCombinedBlock(long height, string content);
        void CutExcessedPackets(long combinedBlockHeight);
        void StoreRegistryFullBlock(ulong height, byte[] content);
        TaskCompletionSource<WitnessPacket> StoreWitnessPacket(long syncBlockHeight, long round, long combinedBlockHeight, LedgerType referencedLedgerType, ushort referencedPacketType, IKey referencedBodyHash, IKey referencedDestinationKey, IKey referencedDestinationKey2, IKey referencedTransactionKey, IKey referencedKeyImage);
        WitnessPacket GetWitnessPacket(long witnessPacketId);
        Dictionary<long, List<WitnessPacket>> GetWitnessPackets(long combinedBlockHeightStart, long combinedBlockHeightEnd);

		bool GetLastRegistryCombinedBlock(out long height, out string content);

		/// <summary>
		/// Returns content of RegistryFulBlocks and height of corresponding CombinedBlocks
		/// </summary>
		/// <param name="heightStart"></param>
		/// <param name="heights"></param>
		/// <param name="contents"></param>
		/// <returns></returns>
		bool GetRegistryFullBlocks(ulong heightStart, out ulong[] heights, out byte[][][] contents);

        void StoreIncomingTransactionalBlock(StateIncomingStoreInput storeInput);
        void StoreIncomingUtxoTransactionBlock(UtxoIncomingStoreInput storeInput);

        StatePacket GetTransactionalIncomingBlock(long witnessid);
        StealthPacket GetUtxoIncomingBlock(long witnessid);
        StealthPacket GetStealthPacket(long combinedRegistryBlockHeight, string hashString);

        int GetTotalUtxoOutputsAmount();
		
		byte[][] GetRootAttributeCommitments(byte[] issuer, int amount);
		StealthOutput[] GetOutputs(int amount);

        void StoreRootAttributeIssuance(IKey issuer, IKey issuanceCommitment, IKey rootCommitment, long combinedBlockHeight);
        void StoreAssociatedAttributeIssuance(IKey? issuer, IKey issuanceCommitment, IKey rootIssuanceCommitment);

        bool CheckRootAttributeExist(byte[] issuer, byte[] issuanceCommitment);
        bool CheckRootAttributeWasValid(byte[] issuer, byte[] issuanceCommitment, long combinedBlockHeight);
        bool CheckAssociatedAtributeExist(Memory<byte>? issuer, Memory<byte> issuanceCommitment, Memory<byte> rootCommitment);

        void SetRootAttributesOverriden(IKey issuer, IKey issuanceCommitment, long combinedBlockHeight);

		void AddEmployeeRecord(IKey issuer, IKey registrationCommitment, IKey groupCommitment);
		void CancelEmployeeRecord(IKey issuer, IKey registrationCommitment);

		byte[] GetEmployeeRecordGroup(string issuer, string registrationCommitment);

		StatePacket GetTransactionBySourceAndHeight(string source, ulong blockHeight);

        void AddCompromisedKeyImage(string keyImage);
        bool GetIsKeyImageCompomised(string keyImage);
    }
}
