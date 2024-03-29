﻿using Newtonsoft.Json;
using O10.Core.Architecture;
using O10.Core.HashCalculations;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Core.Translators;
using O10.Crypto.Models;
using O10.Gateway.Common.Exceptions;
using O10.Gateway.DataLayer.Model;
using O10.Gateway.DataLayer.Services;
using O10.Gateway.DataLayer.Services.Inputs;
using O10.Transactions.Core.Accessors;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Ledgers.Stealth.Transactions;
using System;

namespace O10.Gateway.Common.Services.LedgerSynchronizers
{
    [RegisterExtension(typeof(ILedgerSynchronizer), Lifetime = LifetimeManagement.Singleton)]
    public class StealthLedgerSynchronizer : O10LedgerSynchronizerBase
    {
        private readonly IDataAccessService _dataAccessService;
		private readonly IIdentityKeyProvider _identityKeyProvider;

		public StealthLedgerSynchronizer(IDataAccessService dataAccessService,
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

		public override LedgerType LedgerType => LedgerType.Stealth;

        public override TransactionBase? GetByWitness(WitnessPacket witnessPacket)
        {
            if (witnessPacket is null)
            {
                throw new ArgumentNullException(nameof(witnessPacket));
            }

            var transaction = _dataAccessService.GetStealthTransaction(witnessPacket.WitnessPacketId);
            if (transaction == null)
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

            var registryCombinedBlockHeight = wp.CombinedBlockHeight;

			switch (transaction)
			{
                case KeyImageCompromisedTransaction compromizationProofsTransaction:
                    ProcessCompromizedProofs(compromizationProofsTransaction);
                    break;
                case RevokeIdentityTransaction revokeIdentityTransaction:
					ProcessRevokeIdentity(revokeIdentityTransaction, registryCombinedBlockHeight);
					break;
			}

			StoreTransaction(wp.WitnessPacketId, registryCombinedBlockHeight, transaction as O10StealthTransactionBase);
		}

        private void ProcessCompromizedProofs(KeyImageCompromisedTransaction transaction)
        {
			_dataAccessService.AddCompromisedKeyImage(transaction.KeyImage);
        }

        private void StoreTransaction(long witnessId, long registryCombinedBlockHeight, O10StealthTransactionBase transaction)
		{
            if (transaction is null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            StealthStoreInput storeInput = new StealthStoreInput
			{
				CombinedRegistryBlockHeight = registryCombinedBlockHeight,
				WitnessId = witnessId,
				TransactionType = transaction.TransactionType,
				Commitment = transaction.AssetCommitment,
				Destination = transaction.DestinationKey,
				DestinationKey2 = transaction.DestinationKey2,
				KeyImage = transaction.KeyImage,
				TransactionKey = transaction.TransactionPublicKey,
				Content = transaction.ToJson(),
                Hash = _identityKeyProvider.GetKey(HashCalculation.CalculateHash(transaction.ToString()))
            };

			_dataAccessService.StoreStealthTransaction(storeInput);
		}

		private void ProcessRevokeIdentity(RevokeIdentityTransaction transaction, long registryCombinedBlockHeight)
		{
			_dataAccessService.SetRootAttributesOverriden(
				transaction.DestinationKey2,
				_identityKeyProvider.GetKey(transaction.EligibilityProof.AssetCommitments[0]),
                registryCombinedBlockHeight);
		}
	}
}
