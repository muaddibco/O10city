using O10.Core.Configuration;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Gateway.DataLayer.Model;
using O10.Gateway.DataLayer.Services;
using O10.Gateway.DataLayer.Services.Inputs;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Ledgers;
using O10.Transactions.Core.Ledgers.O10State;
using O10.Transactions.Core.Ledgers.O10State.Transactions;
using System;

namespace O10.Gateway.Common.Services.LedgerSynchronizers
{
    public class O10StateLedgerSynchronizer : O10LedgerSynchronizerBase
    {
        private readonly IDataAccessService _dataAccessService;
        private readonly IIdentityKeyProvider _identityKeyProvider;

        public O10StateLedgerSynchronizer(IDataAccessService dataAccessService,
                                          IConfigurationService configurationService,
                                          IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
                                          ILoggerService loggerService) : base(configurationService, loggerService)
        {
            _dataAccessService = dataAccessService;
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
        }

        public override LedgerType LedgerType => LedgerType.O10State;

        public override IPacketBase GetByWitness(WitnessPacket witnessPacket)
        {
            throw new NotImplementedException();
        }

        protected override void StorePacket(WitnessPacket wp, IPacketBase packet)
        {
            throw new NotImplementedException();
        }

        private void StoreDocumentSignRecord(long witnessId, long registryCombinedBlockHeight, IPacketBase packet)
        {
            var transaction = packet.AsPacket<O10StatePacket>().With<DocumentSignTransaction>();

            StateIncomingStoreInput storeInput = new StateIncomingStoreInput
            {
                CombinedRegistryBlockHeight = registryCombinedBlockHeight,
                WitnessId = witnessId,
                BlockHeight = transaction.Height,
                BlockType = transaction.TransactionType,
                Commitment = transaction.SignerCommitment,
                Source = transaction.Source,
                Content = packet.ToByteArray()
            };

            _dataAccessService.StoreIncomingTransactionalBlock(storeInput);
        }

        private void StoreDocumentRecord(long witnessId, long registryCombinedBlockHeight, IPacketBase packet)
        {
            var transaction = packet.AsPacket<O10StatePacket>().With<DocumentRecordTransaction>();
            StateIncomingStoreInput storeInput = new StateIncomingStoreInput
            {
                CombinedRegistryBlockHeight = registryCombinedBlockHeight,
                WitnessId = witnessId,
                BlockHeight = transaction.Height,
                BlockType = transaction.TransactionType,
                Commitment = transaction.DocumentHash,
                Source = transaction.Source,
                Content = packet.ToByteArray()
            };

            _dataAccessService.StoreIncomingTransactionalBlock(storeInput);
        }

        private void StoreCancelEmployeeRecordPacket(long witnessId, long registryCombinedBlockHeight, IPacketBase packet)
        {
            var transaction = packet.AsPacket<O10StatePacket>().With<CancelEmploymentTransaction>();
            StateIncomingStoreInput storeInput = new StateIncomingStoreInput
            {
                CombinedRegistryBlockHeight = registryCombinedBlockHeight,
                WitnessId = witnessId,
                BlockHeight = transaction.Height,
                BlockType = transaction.TransactionType,
                Commitment = transaction.RegistrationCommitment,
                Source = transaction.Source,
                Content = packet.ToByteArray()
            };

            _dataAccessService.StoreIncomingTransactionalBlock(storeInput);
            _dataAccessService.CancelEmployeeRecord(transaction.Source, transaction.RegistrationCommitment);
        }

        private void StoreRelationRecordPacket(long witnessId, long registryCombinedBlockHeight, IPacketBase packet)
        {
            var transaction = packet.AsPacket<O10StatePacket>().With<RelationTransaction>();
            StateIncomingStoreInput storeInput = new StateIncomingStoreInput
            {
                CombinedRegistryBlockHeight = registryCombinedBlockHeight,
                WitnessId = witnessId,
                BlockHeight = transaction.Height,
                BlockType = transaction.TransactionType,
                Commitment = transaction.RegistrationCommitment,
                Source = transaction.Source,
                Content = packet.ToByteArray()
            };

            _dataAccessService.StoreIncomingTransactionalBlock(storeInput);
            _dataAccessService.AddEmployeeRecord(transaction.Source, transaction.RegistrationCommitment, transaction.GroupCommitment);
        }

        private void StoreIssueAssociatedBlindedAsset(long witnessId, long registryCombinedBlockHeight, IPacketBase packet)
        {
            var transaction = packet.AsPacket<O10StatePacket>().With<IssueAssociatedBlindedAssetTransaction>();
            StateIncomingStoreInput storeInput = new StateIncomingStoreInput
            {
                CombinedRegistryBlockHeight = registryCombinedBlockHeight,
                WitnessId = witnessId,
                BlockHeight = transaction.Height,
                BlockType = transaction.TransactionType,
                Commitment = transaction.AssetCommitment,
                Source = transaction.Source,
                Content = packet.ToByteArray()
            };

            _dataAccessService.StoreIncomingTransactionalBlock(storeInput);

            _dataAccessService.StoreAssociatedAttributeIssuance(transaction.Source, transaction.AssetCommitment, transaction.RootAssetCommitment);
        }

        private void StoreIssueBlindedAsset(long witnessId, long registryCombinedBlockHeight, IPacketBase packet)
        {
            var transaction = packet.AsPacket<O10StatePacket>().With<IssueBlindedAssetTransaction>();
            StateIncomingStoreInput storeInput = new StateIncomingStoreInput
            {
                CombinedRegistryBlockHeight = registryCombinedBlockHeight,
                WitnessId = witnessId,
                BlockHeight = transaction.Height,
                BlockType = transaction.TransactionType,
                Commitment = transaction.AssetCommitment,
                Source = transaction.Source,
                Content = packet.ToByteArray()
            };

            _dataAccessService.StoreIncomingTransactionalBlock(storeInput);
        }

        private void StoreTransferAsset(long witnessId, long registryCombinedBlockHeight, IPacketBase packet)
        {
            var transaction = packet.AsPacket<O10StatePacket>().With<TransferAssetToStealthTransaction>();
            var originatingCommitment = _identityKeyProvider.GetKey(transaction.SurjectionProof.AssetCommitments[0]);
            var storeInput = new StateIncomingStoreInput
            {
                CombinedRegistryBlockHeight = registryCombinedBlockHeight,
                WitnessId = witnessId,
                BlockHeight = transaction.Height,
                BlockType = transaction.TransactionType,
                Commitment = transaction.TransferredAsset.AssetCommitment,
                Destination = transaction.DestinationKey,
                TransactionKey = transaction.TransactionPublicKey,
                Source = transaction.Source,
                OriginatingCommitment = originatingCommitment,
                Content = packet.ToByteArray()
            };

            _dataAccessService.StoreIncomingTransactionalBlock(storeInput);
            _dataAccessService.SetRootAttributesOverriden(transaction.Source, originatingCommitment, registryCombinedBlockHeight);
            _dataAccessService.StoreRootAttributeIssuance(transaction.Source, originatingCommitment, transaction.TransferredAsset.AssetCommitment, registryCombinedBlockHeight);
        }
    }
}
