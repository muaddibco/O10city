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
        #region Initialization
        
        void Initialize(CancellationToken cancellationToken);
        void CutExcessedPackets(long combinedBlockHeight);
        bool GetLastRegistryCombinedBlock(out long height, out string content);

        #endregion Initialization

        #region Processing RT Package

        void UpdateLastSyncBlock(long height, IKey hash);
        void StoreAggregatedRegistrations(long height, string content);
        TaskCompletionSource<WitnessPacket> StoreWitnessPacket(long syncBlockHeight, long round, long combinedBlockHeight, LedgerType referencedLedgerType, ushort referencedPacketType, IKey referencedBodyHash, IKey referencedDestinationKey, IKey referencedDestinationKey2, IKey referencedTransactionKey, IKey referencedKeyImage);
        void StoreStateTransaction(StateIncomingStoreInput storeInput);
        void StoreStealthTransaction(StealthStoreInput storeInput);
        void StoreRootAttributeIssuance(IKey issuer, IKey issuanceCommitment, IKey rootCommitment, long combinedBlockHeight);
        void StoreAssociatedAttributeIssuance(IKey? issuer, IKey issuanceCommitment, IKey rootIssuanceCommitment);
        void SetRootAttributesOverriden(IKey issuer, IKey issuanceCommitment, long combinedBlockHeight);
        void AddRelationRecord(IKey issuer, IKey registrationCommitment);
        void CancelRelationRecord(IKey issuer, IKey registrationCommitment);
        void AddCompromisedKeyImage(IKey keyImage);

        #endregion Processing RT Package

        #region Synchronization Controller

        Task<bool> WaitUntilAggregatedRegistrationsAreStored(long aggregatedRegistrationsHeightStart, long aggregatedRegistrationsHeightEnd, TimeSpan timeout);
        WitnessPacket GetWitnessPacket(long witnessPacketId);
        Dictionary<long, List<WitnessPacket>> GetWitnessPackets(long combinedBlockHeightStart, long combinedBlockHeightEnd);
        StateTransaction? GetStateTransaction(long witnessId);
        StateTransaction? GetStateTransaction(string source, string hashString);
        StealthTransaction? GetStealthTransaction(long witnessId);
        byte[][] GetRootAttributeCommitments(byte[] issuer, int amount);
        StealthOutput[] GetOutputs(int amount);
        bool CheckRootAttributeExist(byte[] issuer, byte[] issuanceCommitment);
        bool CheckRootAttributeWasValid(byte[] issuer, byte[] issuanceCommitment, long combinedBlockHeight);
        bool CheckAssociatedAtributeExist(Memory<byte>? issuer, Memory<byte> issuanceCommitment, Memory<byte> rootCommitment);
        bool GetIsKeyImageCompomised(IKey keyImage);

        #endregion Synchronization Controller
    }
}
