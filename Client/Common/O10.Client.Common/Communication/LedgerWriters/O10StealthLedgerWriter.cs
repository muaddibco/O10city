using O10.Client.Common.Configuration;
using O10.Client.Common.Interfaces;
using O10.Client.Common.Interfaces.Inputs;
using O10.Client.DataLayer.Services;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.Cryptography;
using O10.Core.ExtensionMethods;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Core.Models;
using O10.Crypto.Models;
using O10.Crypto.Services;
using O10.Transactions.Core.DTOs;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Ledgers;
using O10.Transactions.Core.Ledgers.Stealth;
using O10.Transactions.Core.Ledgers.Stealth.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace O10.Client.Common.Communication.LedgerWriters
{
    [RegisterExtension(typeof(ILedgerWriter), Lifetime = LifetimeManagement.Scoped)]
    public class O10StealthLedgerWriter : LedgerWriterBase
    {
        private readonly IPropagatorBlock<TaskCompletionWrapper<TransactionBase>, DependingTaskCompletionWrapper<IPacketBase, TransactionBase>> _pipeIn;
        private readonly IStealthClientCryptoService _clientCryptoService;
        private readonly IGatewayService _gatewayService;
        private readonly IIdentityKeyProvider _identityKeyProvider;
        private readonly IRestApiConfiguration _restApiConfiguration;

        public O10StealthLedgerWriter(IStealthClientCryptoService clientCryptoService,
                                      IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
                                      IGatewayService gatewayService,
                                      IConfigurationService configurationService,
                                      ILoggerService loggerService)
            : base(loggerService)
        {
            _clientCryptoService = clientCryptoService;
            _gatewayService = gatewayService;
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
            _restApiConfiguration = configurationService.Get<IRestApiConfiguration>();
            _pipeIn = new TransformBlock<TaskCompletionWrapper<TransactionBase>, DependingTaskCompletionWrapper<IPacketBase, TransactionBase>>(ProducePacket);
        }

        public override LedgerType LedgerType => LedgerType.Stealth;

        public override ITargetBlock<TaskCompletionWrapper<TransactionBase>> PipeIn => _pipeIn;

        public override async Task Initialize(long accountId)
        {
            await base.Initialize(accountId).ConfigureAwait(false);
        }

        private async Task<DependingTaskCompletionWrapper<IPacketBase, TransactionBase>> ProducePacket(TaskCompletionWrapper<TransactionBase> wrapper)
        {
            if (!(wrapper.State is O10StealthTransactionBase o10StealthTransaction))
            {
                throw new ArgumentOutOfRangeException(nameof(wrapper));
            }

            if (!(wrapper.Argument is StealthPropagationArgument arg))
            {
                throw new ArgumentOutOfRangeException(nameof(wrapper));
            }

            StealthPayload payload = new StealthPayload(o10StealthTransaction);

            OutputSources[] outputModels = await _gatewayService.GetOutputs(_restApiConfiguration.RingSize + 1).ConfigureAwait(false);
            GetDestinationKeysRing(arg.PrevDestinationKey, outputModels.Select(m => m.DestinationKey).ToArray(), out int pos, out IKey[] destinationKeys);

            var signature = _clientCryptoService.Sign(payload, new StealthSignatureInput(arg.PrevTransactionKey, destinationKeys, pos, arg.PreSigningAction)) as StealthSignature;

            var packet = new StealthPacket
            {
                Payload = payload,
                Signature = signature
            };

            return new DependingTaskCompletionWrapper<IPacketBase, TransactionBase>(packet, wrapper);
        }

        /// <summary>
        /// Returns existing Asset Commitments
        /// </summary>
        /// <param name="prevDestinationKey"></param>
        /// <param name="destinationKeysPool"></param>
        /// <param name="actualPos"></param>
        /// <param name="destinationKeys"></param>
        private void GetDestinationKeysRing(IKey prevDestinationKey, IKey[] destinationKeysPool, out int actualPos, out IKey[] destinationKeys)
        {
            Random random = new Random(BitConverter.ToInt32(prevDestinationKey.ToByteArray(), 0));
            actualPos = random.Next(destinationKeysPool.Length);
            destinationKeys = new IKey[destinationKeysPool.Length];
            List<int> pickedPositions = new List<int>();

            for (int i = 0; i < destinationKeysPool.Length; i++)
            {
                if (i == actualPos)
                {
                    destinationKeys[i] = prevDestinationKey;
                }
                else
                {
                    bool found = false;
                    do
                    {
                        int randomPos = random.Next(destinationKeysPool.Length);
                        if (pickedPositions.Contains(randomPos))
                        {
                            continue;
                        }

                        if (destinationKeysPool[randomPos].Equals(prevDestinationKey))
                        {
                            continue;
                        }

                        destinationKeys[i] = destinationKeysPool[randomPos];
                        pickedPositions.Add(randomPos);
                        found = true;
                    } while (!found);
                }
            }
        }
    }
}
