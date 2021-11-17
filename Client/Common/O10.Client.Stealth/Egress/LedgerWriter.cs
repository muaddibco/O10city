using O10.Client.Common.Communication.LedgerWriters;
using O10.Client.Common.Configuration;
using O10.Client.Common.Interfaces;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.ExtensionMethods;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Core.Models;
using O10.Crypto.Models;
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

namespace O10.Client.Stealth.Egress
{
    [RegisterExtension(typeof(ILedgerWriter), Lifetime = LifetimeManagement.Scoped)]
    public class LedgerWriter : LedgerWriterBase
    {
        private readonly IPropagatorBlock<TaskCompletionWrapper<TransactionBase>, DependingTaskCompletionWrapper<IPacketBase, TransactionBase>> _pipeIn;
        private readonly IStealthClientCryptoService _clientCryptoService;
        private readonly IGatewayService _gatewayService;
        private readonly IIdentityKeyProvider _identityKeyProvider;
        private readonly IRestApiConfiguration _restApiConfiguration;

        public LedgerWriter(IStealthClientCryptoService clientCryptoService,
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
            _pipeIn.LinkTo(_gatewayService.PipeInTransactions);
        }

        public override LedgerType LedgerType => LedgerType.Stealth;

        public override ITargetBlock<TaskCompletionWrapper<TransactionBase>> PipeIn => _pipeIn;

        public override async Task Initialize(long accountId)
        {
            await base.Initialize(accountId).ConfigureAwait(false);
        }

        private async Task<DependingTaskCompletionWrapper<IPacketBase, TransactionBase>> ProducePacket(TaskCompletionWrapper<TransactionBase> wrapper)
        {
            if (wrapper.State is not O10StealthTransactionBase o10StealthTransaction)
            {
                wrapper.TaskCompletion.SetException(new ArgumentOutOfRangeException(nameof(wrapper)));
                return null;
            }

            if (wrapper.Argument is not PropagationArgument arg)
            {
                wrapper.TaskCompletion.SetException(new ArgumentOutOfRangeException(nameof(wrapper)));
                return null;
            }

            StealthPayload payload = new(o10StealthTransaction);

            try
            {
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
            catch (AggregateException ex)
            {
                Logger.Error($"Failed to produce and send stealth packet", ex.InnerException);
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to produce and send stealth packet", ex);
                return null;
            }
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
            Random random = new(BitConverter.ToInt32(prevDestinationKey.ToByteArray(), 0));
            actualPos = random.Next(destinationKeysPool.Length);
            destinationKeys = new IKey[destinationKeysPool.Length];
            List<int> pickedPositions = new();

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
