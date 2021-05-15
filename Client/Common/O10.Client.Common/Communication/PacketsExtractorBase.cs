using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Services;
using O10.Core.Logging;
using O10.Core.Models;
using O10.Core.Serialization;
using O10.Crypto.Models;

namespace O10.Client.Common.Communication
{
    public abstract class PacketsExtractorBase : IPacketsExtractor
    {
        protected readonly ILogger _logger;
        private readonly IGatewayService _syncStateProvider;
        private readonly IDataAccessService _dataAccessService;
        private readonly IPropagatorBlock<TaskCompletionWrapper<TransactionBase>, TaskCompletionWrapper<TransactionBase>> _propagator;
        private readonly IPropagatorBlock<WitnessPackage, WitnessPackage> _propagatorProcessed;
        private readonly ITargetBlock<WitnessPackageWrapper> _pipeIn;
        protected readonly IClientCryptoService _clientCryptoService;

        public PacketsExtractorBase(
            IGatewayService syncStateProvider,
            IClientCryptoService clientCryptoService,
            IDataAccessService dataAccessService,
            ILoggerService loggerService)
        {
            _logger = loggerService.GetLogger(GetType().Name);
            _propagator = new TransformBlock<TaskCompletionWrapper<TransactionBase>, TaskCompletionWrapper<TransactionBase>>(p => p);
            _propagatorProcessed = new TransformBlock<WitnessPackage, WitnessPackage>(p => p);
            _clientCryptoService = clientCryptoService;
            _syncStateProvider = syncStateProvider;
            _dataAccessService = dataAccessService;

            _pipeIn = new ActionBlock<WitnessPackageWrapper>(async wrapper =>
            {
                WitnessPackage witnessPackage = wrapper.WitnessPackage;

                _logger.LogIfDebug(() => $"[{AccountId}]: {GetType().Name} of witnessPackage at CombinedBlockHeight {witnessPackage.CombinedBlockHeight}");

                try
                {
                    List<Task> allPacketsProcessedTasks = new List<Task>();
                    List<PacketWitness> witnessesToMe = new List<PacketWitness>();

                    foreach (var item in witnessPackage.Witnesses)
                    {
                        if (_dataAccessService.WitnessAsProcessed(AccountId, item.WitnessId))
                        {
                            _logger.Warning($"[{AccountId}]: witness {item.WitnessId} already processed");
                            continue;
                        }
                        else
                        {
                            _logger.Debug($"[{AccountId}]: Processing witness {item.WitnessId}");
                        }

                        if (CheckPacketWitness(item))
                        {
                            witnessesToMe.Add(item);
                        }
                    }

                    if (witnessesToMe.Count > 0)
                    {
                        _logger.LogIfDebug(() => $"[{AccountId}]: Obtaining packets for witnesses [{string.Join(',', witnessesToMe.Select(w => w.WitnessId).ToArray())}]");
                        var transactions = await _syncStateProvider.GetTransactions(witnessesToMe.Select(w => w.WitnessId)).ConfigureAwait(false);

                        if (transactions?.Any() != true)
                        {
                            _logger.Error($"[{AccountId}]: no packet infos obtained from the Gateway");
                            wrapper.CompletionSource.SetResult(true);
                            return;
                        }

                        foreach (var transaction in transactions)
                        {
                            _logger.LogIfDebug(() => $"[{AccountId}]: processing transaction {transaction.GetType().Name}");

                            if (transaction != null)
                            {
                                if (transaction is StealthTransactionBase stealthTransaction)
                                {
                                    _logger.LogIfDebug(() => $"[{AccountId}]: Obtained transaction {stealthTransaction.GetType().Name} with {nameof(stealthTransaction.KeyImage)}={stealthTransaction.KeyImage}");
                                }
                                else
                                {
                                    _logger.LogIfDebug(() => $"[{AccountId}]: Obtained transaction {JsonConvert.SerializeObject(transaction, new ByteArrayJsonConverter())}");
                                }

                                var packetWrapper = new TaskCompletionWrapper<TransactionBase>(transaction);
                                allPacketsProcessedTasks.Add(packetWrapper.TaskCompletion.Task);

                                await _propagator.SendAsync(packetWrapper).ConfigureAwait(false);
                                _logger.Debug($"[{AccountId}]: Passing transaction {transaction.GetType().Name} to WalletSynchronizer");
                            }
                        }

                        await Task.WhenAll(allPacketsProcessedTasks).ConfigureAwait(false);
                        wrapper.CompletionSource.SetResult(true);
                    }
                    else
                    {
                        wrapper.CompletionSource.SetResult(false);
                    }

                    await _propagatorProcessed.SendAsync(witnessPackage).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.Error($"[{AccountId}]: Failure during packet extraction", ex);
                    wrapper.CompletionSource.SetResult(false);
                }
            });
        }

        public virtual void Initialize(long accountId)
        {
            AccountId = accountId;
        }

        public abstract string Name { get; }
        protected long AccountId { get; set; }

        protected abstract bool CheckPacketWitness(PacketWitness packetWitness);

        public virtual ISourceBlock<T> GetSourcePipe<T>(string name = null)
        {
            if (typeof(T) == typeof(TaskCompletionWrapper<TransactionBase>))
            {
                return (ISourceBlock<T>)_propagator;
            }
            else if (typeof(T) == typeof(WitnessPackage))
            {
                return (ISourceBlock<T>)_propagatorProcessed;
            }

            throw new InvalidOperationException($"No source blocks are available for type {typeof(T).FullName}");
        }

        public virtual ITargetBlock<T> GetTargetPipe<T>(string name = null)
        {
            if (typeof(T) == typeof(WitnessPackageWrapper))
            {
                return (ITargetBlock<T>)_pipeIn;
            }

            throw new InvalidOperationException($"No target blocks are available for type {typeof(T).FullName}");
        }
    }
}
