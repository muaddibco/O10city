using Newtonsoft.Json;
using O10.Core.Configuration;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Core.Models;
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

        public override IPacketBase GetByWitness(WitnessPacket witnessPacket)
        {
            if (witnessPacket is null)
            {
                throw new ArgumentNullException(nameof(witnessPacket));
            }

            DataLayer.Model.StealthPacket utxoIncomingBlock = _dataAccessService.GetUtxoIncomingBlock(witnessPacket.WitnessPacketId);
			return SerializableEntity<IPacketBase>.Create(utxoIncomingBlock.Content);
		}

		protected override void StorePacket(WitnessPacket wp, IPacketBase packet)
        {
            if (wp is null)
            {
                throw new ArgumentNullException(nameof(wp));
            }

            if (packet is null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            var registryCombinedBlockHeight = wp.CombinedBlockHeight;

			switch (packet.Body.TransactionType)
			{
                case TransactionTypes.Stealth_TransitionCompromisedProofs:
                    ProcessCompromizedProofs(packet);
                    break;
                case TransactionTypes.Stealth_RevokeIdentity:
					ProcessRevokeIdentity(packet, registryCombinedBlockHeight);
					break;
			}
			StoreUtxoPacket(wp.WitnessPacketId, registryCombinedBlockHeight, packet);
		}

        private void ProcessCompromizedProofs(IPacketBase packet)
        {
			var transaction = packet.AsPacket<StealthPacket>().With<CompromizationProofsTransaction>();
			
			_dataAccessService.AddCompromisedKeyImage(transaction.KeyImage);
        }

        private void StoreUtxoPacket(long witnessId, long registryCombinedBlockHeight, IPacketBase packet)
		{
            var transaction = packet.AsPacket<StealthPacket>().With<O10StealthTransactionBase>();

			UtxoIncomingStoreInput storeInput = new UtxoIncomingStoreInput
			{
				CombinedRegistryBlockHeight = registryCombinedBlockHeight,
				WitnessId = witnessId,
				BlockType = transaction.TransactionType,
				Commitment = transaction.AssetCommitment,
				Destination = transaction.DestinationKey,
				DestinationKey2 = transaction.DestinationKey2,
				KeyImage = transaction.KeyImage,
				TransactionKey = transaction.TransactionPublicKey,
				Content = transaction.ToString()
			};

			_dataAccessService.StoreIncomingUtxoTransactionBlock(storeInput);
		}

		private void ProcessRevokeIdentity(IPacketBase packet, long registryCombinedBlockHeight)
		{
			var transaction = packet.AsPacket<StealthPacket>().With<RevokeIdentityTransaction>();

			_dataAccessService.SetRootAttributesOverriden(
				transaction.DestinationKey2,
				_identityKeyProvider.GetKey(transaction.OwnershipProof.AssetCommitments[0]),
                registryCombinedBlockHeight);
		}
	}
}
