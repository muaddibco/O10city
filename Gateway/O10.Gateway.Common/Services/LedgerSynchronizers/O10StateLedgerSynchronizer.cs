using Newtonsoft.Json;
using O10.Core.Architecture;
using O10.Core.HashCalculations;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Core.Models;
using O10.Core.Translators;
using O10.Crypto.Models;
using O10.Gateway.Common.Exceptions;
using O10.Gateway.DataLayer.Model;
using O10.Gateway.DataLayer.Services;
using O10.Gateway.DataLayer.Services.Inputs;
using O10.Transactions.Core.Accessors;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Ledgers.O10State.Transactions;
using System;
using System.Text.Json.Serialization;

namespace O10.Gateway.Common.Services.LedgerSynchronizers
{
    [RegisterExtension(typeof(ILedgerSynchronizer), Lifetime = LifetimeManagement.Singleton)]
    public class O10StateLedgerSynchronizer : O10LedgerSynchronizerBase
    {
        private readonly IDataAccessService _dataAccessService;
        private readonly IIdentityKeyProvider _identityKeyProvider;

        public O10StateLedgerSynchronizer(IDataAccessService dataAccessService,
                                          IAccessorProvider accessorProvider,
                                          ITranslatorsRepository translatorsRepository,
                                          IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
                                          IHashCalculationsRepository hashCalculationsRepository,
                                          ILoggerService loggerService) 
            : base(accessorProvider, translatorsRepository, hashCalculationsRepository, loggerService)
        {
            _dataAccessService = dataAccessService;
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
        }

        public override LedgerType LedgerType => LedgerType.O10State;

        public override TransactionBase? GetByWitness(WitnessPacket witnessPacket)
        {
            if (witnessPacket is null)
            {
                throw new ArgumentNullException(nameof(witnessPacket));
            }

            var transaction = _dataAccessService.GetStateTransaction(witnessPacket.WitnessPacketId);
            if(transaction == null)
            {
                throw new NoTransactionFoundByWitnessIdException(witnessPacket.WitnessPacketId);
            }

            JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
            return JsonConvert.DeserializeObject(transaction.Content, serializerSettings) as TransactionBase;
        }

        protected override void StoreTransaction(WitnessPacket wp, TransactionBase transaction)
        {
            if (wp is null)
            {
                throw new ArgumentNullException(nameof(wp));
            }

            if (transaction is null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            StateIncomingStoreInput storeInput = transaction switch
            {
                IssueBlindedAssetTransaction issueBlindedAssetTransaction => StoreIssueBlindedAsset(wp.WitnessPacketId, wp.CombinedBlockHeight, issueBlindedAssetTransaction),
                IssueAssociatedBlindedAssetTransaction issueAssociatedBlindedAssetTransaction => StoreIssueAssociatedBlindedAsset(wp.WitnessPacketId, wp.CombinedBlockHeight, issueAssociatedBlindedAssetTransaction),
                TransferAssetToStealthTransaction transferAssetToStealthTransaction => StoreTransferAsset(wp.WitnessPacketId, wp.CombinedBlockHeight, transferAssetToStealthTransaction),
                RelationTransaction relationTransaction => StoreRelationRecordPacket(wp.WitnessPacketId, wp.CombinedBlockHeight, relationTransaction),
                CancelRelationTransaction cancelRelationTransaction => StoreCancelRelationRecordPacket(wp.WitnessPacketId, wp.CombinedBlockHeight, cancelRelationTransaction),
                DocumentRecordTransaction documentRecordTransaction => StoreDocumentRecord(wp.WitnessPacketId, wp.CombinedBlockHeight, documentRecordTransaction),
                DocumentSignTransaction documentSignTransaction => StoreDocumentSignRecord(wp.WitnessPacketId, wp.CombinedBlockHeight, documentSignTransaction),
                _ => throw new ArgumentOutOfRangeException(nameof(transaction)),
            };

            _dataAccessService.StoreStateTransaction(storeInput);
        }

        private StateIncomingStoreInput StoreDocumentSignRecord(long witnessId, long registryCombinedBlockHeight, DocumentSignTransaction transaction)
            => new StateIncomingStoreInput
            {
                CombinedRegistryBlockHeight = registryCombinedBlockHeight,
                WitnessId = witnessId,
                TransactionType = transaction.TransactionType,
                Commitment = transaction.SignerCommitment,
                Source = transaction.Source,
                Content = transaction.ToJson(),
                Hash = _identityKeyProvider.GetKey(HashCalculation.CalculateHash(transaction.ToString()))
            };

        private StateIncomingStoreInput StoreDocumentRecord(long witnessId, long registryCombinedBlockHeight, DocumentRecordTransaction transaction)
            => new StateIncomingStoreInput
            {
                CombinedRegistryBlockHeight = registryCombinedBlockHeight,
                WitnessId = witnessId,
                TransactionType = transaction.TransactionType,
                Commitment = _identityKeyProvider.GetKey(transaction.DocumentHash),
                Source = transaction.Source,
                Content = transaction.ToJson(),
                Hash = _identityKeyProvider.GetKey(HashCalculation.CalculateHash(transaction.ToString()))
            };

        private StateIncomingStoreInput StoreCancelRelationRecordPacket(long witnessId, long registryCombinedBlockHeight, CancelRelationTransaction transaction)
        {
            _dataAccessService.CancelRelationRecord(transaction.Source, transaction.RegistrationCommitment);

            return new StateIncomingStoreInput
            {
                CombinedRegistryBlockHeight = registryCombinedBlockHeight,
                WitnessId = witnessId,
                TransactionType = transaction.TransactionType,
                Commitment = transaction.RegistrationCommitment,
                Source = transaction.Source,
                Content = transaction.ToJson(),
                Hash = _identityKeyProvider.GetKey(HashCalculation.CalculateHash(transaction.ToString()))
            };
        }

        private StateIncomingStoreInput StoreRelationRecordPacket(long witnessId, long registryCombinedBlockHeight, RelationTransaction transaction)
        {
            _dataAccessService.AddRelationRecord(transaction.Source, transaction.RegistrationCommitment);

            return new StateIncomingStoreInput
            {
                CombinedRegistryBlockHeight = registryCombinedBlockHeight,
                WitnessId = witnessId,
                TransactionType = transaction.TransactionType,
                Commitment = transaction.RegistrationCommitment,
                Source = transaction.Source,
                Content = transaction.ToJson(),
                Hash = _identityKeyProvider.GetKey(HashCalculation.CalculateHash(transaction.ToString()))
            };
        }

        private StateIncomingStoreInput StoreIssueAssociatedBlindedAsset(long witnessId, long registryCombinedBlockHeight, IssueAssociatedBlindedAssetTransaction transaction)
        {
            _dataAccessService.StoreAssociatedAttributeIssuance(transaction.Source, transaction.AssetCommitment, transaction.RootAssetCommitment);
         
            return new StateIncomingStoreInput
            {
                CombinedRegistryBlockHeight = registryCombinedBlockHeight,
                WitnessId = witnessId,
                TransactionType = transaction.TransactionType,
                Commitment = transaction.AssetCommitment,
                Source = transaction.Source,
                Content = transaction.ToJson(),
                Hash = _identityKeyProvider.GetKey(HashCalculation.CalculateHash(transaction.ToString()))
            };
        }

        private StateIncomingStoreInput StoreIssueBlindedAsset(long witnessId, long registryCombinedBlockHeight, IssueBlindedAssetTransaction transaction)
            => new StateIncomingStoreInput
            {
                CombinedRegistryBlockHeight = registryCombinedBlockHeight,
                WitnessId = witnessId,
                TransactionType = transaction.TransactionType,
                Commitment = transaction.AssetCommitment,
                Source = transaction.Source,
                Content = transaction.ToJson(),
                Hash = _identityKeyProvider.GetKey(HashCalculation.CalculateHash(transaction.ToString()))
            };

        private StateIncomingStoreInput StoreTransferAsset(long witnessId, long registryCombinedBlockHeight, TransferAssetToStealthTransaction transaction)
        {
            var originatingCommitment = _identityKeyProvider.GetKey(transaction.SurjectionProof.AssetCommitments[0]);
            
            _dataAccessService.SetRootAttributesOverriden(transaction.Source, originatingCommitment, registryCombinedBlockHeight);
            _dataAccessService.StoreRootAttributeIssuance(transaction.Source, originatingCommitment, transaction.TransferredAsset.AssetCommitment, registryCombinedBlockHeight);

            return new StateIncomingStoreInput
            {
                CombinedRegistryBlockHeight = registryCombinedBlockHeight,
                WitnessId = witnessId,
                TransactionType = transaction.TransactionType,
                Commitment = transaction.TransferredAsset.AssetCommitment,
                Destination = transaction.DestinationKey,
                TransactionKey = transaction.TransactionPublicKey,
                Source = transaction.Source,
                OriginatingCommitment = originatingCommitment,
                Content = transaction.ToJson(),
                Hash = _identityKeyProvider.GetKey(HashCalculation.CalculateHash(transaction.ToString()))
            };
        }
    }
}
