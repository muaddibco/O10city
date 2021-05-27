using O10.Client.Common.Interfaces;
using O10.Core.Architecture;
using O10.Core.Cryptography;
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
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace O10.Client.Common.Communication.LedgerWriters
{
    [RegisterExtension(typeof(ILedgerWriter), Lifetime = LifetimeManagement.Scoped)]
    public class O10StealthLedgerWriter : LedgerWriterBase
    {
        private readonly IPropagatorBlock<TaskCompletionWrapper<TransactionBase>, DependingTaskCompletionWrapper<IPacketBase, TransactionBase>> _pipeIn;
        private readonly IStealthClientCryptoService _clientCryptoService;
        private readonly ISigningService _signingService;
        private readonly IGatewayService _gatewayService;
        private readonly IIdentityKeyProvider _identityKeyProvider;

        public O10StealthLedgerWriter(IStealthClientCryptoService clientCryptoService,
                                      ISigningService signingService,
                                      IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
                                      IGatewayService gatewayService,
                                      ILoggerService loggerService)
            :base(loggerService)
        {
            _clientCryptoService = clientCryptoService;
            _signingService = signingService;
            _gatewayService = gatewayService;
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
        }

        public override LedgerType LedgerType => LedgerType.Stealth;

        public override ITargetBlock<TaskCompletionWrapper<TransactionBase>> PipeIn => _pipeIn;

        public override async Task Initialize(long accountId)
        {
            await base.Initialize(accountId).ConfigureAwait(false);
        }

        private DependingTaskCompletionWrapper<IPacketBase, TransactionBase> ProducePacket(TaskCompletionWrapper<TransactionBase> wrapper)
        {
            if (!(wrapper.State is O10StealthTransactionBase o10StealthTransaction))
            {
                throw new ArgumentOutOfRangeException(nameof(wrapper));
            }

            StealthPayload payload = new StealthPayload(o10StealthTransaction);

            var signature = _signingService.Sign(payload) as StealthSignature;

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
        /// <param name="prevCommitment"></param>
        /// <param name="commitmentsPool"></param>
        /// <param name="actualAssetPos"></param>
        /// <param name="commitments"></param>
        private void GetAssetCommitmentsRing(byte[] prevCommitment, IKey[] commitmentsPool, out int actualAssetPos, out IKey[] commitments)
        {
            Random random = new Random(BitConverter.ToInt32(prevCommitment, 0));
            actualAssetPos = random.Next(commitmentsPool.Length);
            commitments = new IKey[commitmentsPool.Length];
            List<int> pickedPositions = new List<int>();

            for (int i = 0; i < commitmentsPool.Length; i++)
            {
                if (i == actualAssetPos)
                {
                    commitments[i] = _identityKeyProvider.GetKey(prevCommitment);
                }
                else
                {
                    bool found = false;
                    do
                    {
                        int randomPos = random.Next(commitmentsPool.Length);
                        if (pickedPositions.Contains(randomPos))
                        {
                            continue;
                        }

                        if (commitmentsPool[randomPos].Equals(prevCommitment))
                        {
                            continue;
                        }

                        commitments[i] = commitmentsPool[randomPos];
                        pickedPositions.Add(randomPos);
                        found = true;
                    } while (!found);
                }
            }
        }
    }
}
