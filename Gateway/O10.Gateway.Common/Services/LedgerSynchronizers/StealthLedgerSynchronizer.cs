using Newtonsoft.Json;
using O10.Core.Configuration;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Core.Models;
using O10.Crypto.Models;
using O10.Gateway.DataLayer.Model;
using O10.Gateway.DataLayer.Services;
using O10.Gateway.DataLayer.Services.Inputs;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Ledgers;
using O10.Transactions.Core.Ledgers.Stealth;
using O10.Transactions.Core.Ledgers.Stealth.Transactions;
using System;
using StealthPacket = O10.Transactions.Core.Ledgers.Stealth.StealthPacket;

namespace O10.Gateway.Common.Services.LedgerSynchronizers
{
	public class StealthLedgerSynchronizer : O10LedgerSynchronizerBase
    {
        private readonly IDataAccessService _dataAccessService;
		private readonly IIdentityKeyProvider _identityKeyProvider;

		public StealthLedgerSynchronizer(IDataAccessService dataAccessService,
                                         IConfigurationService configurationService,
                                         IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
										 ILoggerService loggerService) 
			: base(configurationService, loggerService)
        {
            _dataAccessService = dataAccessService;
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
		}

		public override LedgerType LedgerType => LedgerType.Stealth;

        public override TransactionBase GetByWitness(WitnessPacket witnessPacket)
        {
            if (witnessPacket is null)
            {
                throw new ArgumentNullException(nameof(witnessPacket));
            }

            DataLayer.Model.StealthTransaction utxoIncomingBlock = _dataAccessService.GetStealthTransaction(witnessPacket.WitnessPacketId);
			return SerializableEntity<IPacketBase>.Create(utxoIncomingBlock.Content).Body;
		}

		protected override void StorePacket(WitnessPacket wp, TransactionBase transaction)
        {
            if (wp is null)
            {
                throw new ArgumentNullException(nameof(wp));
            }

            if (transaction is null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            var registryCombinedBlockHeight = wp.CombinedBlockHeight;

			switch (transaction)
			{
                case CompromizationProofsTransaction compromizationProofsTransaction:
                    ProcessCompromizedProofs(compromizationProofsTransaction);
                    break;
                case RevokeIdentityTransaction revokeIdentityTransaction:
					ProcessRevokeIdentity(revokeIdentityTransaction, registryCombinedBlockHeight);
					break;
			}
			StoreUtxoPacket(wp.WitnessPacketId, registryCombinedBlockHeight, transaction);
		}

        private void ProcessCompromizedProofs(CompromizationProofsTransaction transaction)
        {
			_dataAccessService.AddCompromisedKeyImage(transaction.KeyImage);
        }

        private void StoreUtxoPacket(long witnessId, long registryCombinedBlockHeight, O10StealthTransactionBase transaction)
		{
			UtxoIncomingStoreInput storeInput = new UtxoIncomingStoreInput
			{
				CombinedRegistryBlockHeight = registryCombinedBlockHeight,
				WitnessId = witnessId,
				TransactionType = transaction.TransactionType,
				Commitment = transaction.AssetCommitment,
				Destination = transaction.DestinationKey,
				DestinationKey2 = transaction.DestinationKey2,
				KeyImage = transaction.KeyImage,
				TransactionKey = transaction.TransactionPublicKey,
				Content = transaction.ToString()
			};

			_dataAccessService.StoreIncomingUtxoTransactionBlock(storeInput);
		}

		private void ProcessRevokeIdentity(RevokeIdentityTransaction transaction, long registryCombinedBlockHeight)
		{
			_dataAccessService.SetRootAttributesOverriden(
				transaction.DestinationKey2,
				_identityKeyProvider.GetKey(transaction.OwnershipProof.AssetCommitments[0]),
                registryCombinedBlockHeight);
		}
	}
}
