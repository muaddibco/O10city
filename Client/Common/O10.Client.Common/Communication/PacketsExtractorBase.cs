using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.DataModel;
using O10.Transactions.Core.Parsers;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Services;
using O10.Core.Logging;
using O10.Core.Models;

namespace O10.Client.Common.Communication
{
    public abstract class PacketsExtractorBase : IPacketsExtractor
    {
        protected readonly ILogger _logger;
        private readonly IGatewayService _syncStateProvider;
        private readonly IDataAccessService _dataAccessService;
        private readonly IPropagatorBlock<PacketWrapper, PacketWrapper> _propagator;
        private readonly IPropagatorBlock<WitnessPackage, WitnessPackage> _propagatorProcessed;
        private readonly IBlockParsersRepositoriesRepository _blockParsersRepositoriesRepository;
        private readonly ITargetBlock<WitnessPackageWrapper> _pipeIn;
        protected readonly IClientCryptoService _clientCryptoService;
        protected long _accountId;

        public PacketsExtractorBase(
            IBlockParsersRepositoriesRepository blockParsersRepositoriesRepository,
            IGatewayService syncStateProvider,
            IClientCryptoService clientCryptoService,
            IDataAccessService dataAccessService,
            ILoggerService loggerService)
        {
            _logger = loggerService.GetLogger(GetType().Name);
            _propagator = new TransformBlock<PacketWrapper, PacketWrapper>(p => p);
            _propagatorProcessed = new TransformBlock<WitnessPackage, WitnessPackage>(p => p);
            _clientCryptoService = clientCryptoService;
            _syncStateProvider = syncStateProvider;
            _dataAccessService = dataAccessService;

            _pipeIn = new ActionBlock<WitnessPackageWrapper>(async wrapper =>
            {
                WitnessPackage witnessPackage = wrapper.WitnessPackage;

                _logger.LogIfDebug(() => $"[{_accountId}]: {GetType().Name} of witnessPackage at CombinedBlockHeight {witnessPackage.CombinedBlockHeight}");

                try
                {
                    List<Task> allPacketsProcessedTasks = new List<Task>();
                    List<PacketWitness> witnessesToMe = new List<PacketWitness>();

                    foreach (var item in witnessPackage.StateWitnesses)
                    {
                        if (_dataAccessService.WitnessAsProcessed(_accountId, item.WitnessId))
                        {
                            _logger.Warning($"[{_accountId}]: witness {item.WitnessId} already processed");
                            continue;
                        }
                        else
                        {
                            _logger.Debug($"[{_accountId}]: Processing witness {item.WitnessId}");
                        }

                        if (CheckPacketWitness(item))
                        {
                            witnessesToMe.Add(item);
                        }
                    }

                    foreach (var item in witnessPackage.StealthWitnesses)
                    {
                        if (_dataAccessService.WitnessAsProcessed(_accountId, item.WitnessId))
                        {
                            _logger.Warning($"[{_accountId}]: witness {item.WitnessId} already processed");
                            continue;
                        }
                        else
                        {
                            _logger.Debug($"[{_accountId}]: Processing witness {item.WitnessId}");
                        }

                        if (CheckPacketWitness(item))
                        {
                            witnessesToMe.Add(item);
                        }
                    }

                    if (witnessesToMe.Count > 0)
                    {
                        _logger.LogIfDebug(() => $"[{_accountId}]: Obtaining packets for witnesses [{string.Join(',', witnessesToMe.Select(w => w.WitnessId).ToArray())}]");
                        IEnumerable<PacketInfo> packetInfos = await _syncStateProvider.GetPacketInfos(witnessesToMe.Select(w => w.WitnessId)).ConfigureAwait(false);

                        if (packetInfos?.Any() != true)
                        {
                            _logger.Error($"[{_accountId}]: no packet infos obtained from the Gateway");
                            wrapper.CompletionSource.SetResult(true);
                            return;
                        }

                        foreach (var packetInfo in packetInfos)
                        {
                            _logger.LogIfDebug(() => $"[{_accountId}]: processing packet PacketType={packetInfo.PacketType}, BlockType={packetInfo.BlockType}");
                            IBlockParsersRepository blockParsersRepository = _blockParsersRepositoriesRepository.GetBlockParsersRepository(packetInfo.PacketType);
                            IBlockParser blockParser = blockParsersRepository?.GetInstance(packetInfo.BlockType);
                            PacketBase packetBase = blockParser?.Parse(packetInfo.Content);

                            if (packetBase != null)
                            {
                                if (packetBase is StealthSignedPacketBase packet)
                                {
                                    _logger.LogIfDebug(() => $"[{_accountId}]: Obtained packet {packetBase.GetType().Name} with {nameof(packet.KeyImage)}={packet.KeyImage}");
                                }
                                else
                                {
                                    _logger.LogIfDebug(() => $"[{_accountId}]: Obtained packet {JsonConvert.SerializeObject(packetBase, new ByteArrayJsonConverter())}");
                                }

                                PacketWrapper packetWrapper = new PacketWrapper(packetBase);
                                allPacketsProcessedTasks.Add(packetWrapper.TaskCompletion.Task);

                                await _propagator.SendAsync(packetWrapper).ConfigureAwait(false);
                                _logger.Debug($"[{_accountId}]: Passing packet {packetBase.GetType().Name} to WalletSynchronizer");
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
                    _logger.Error($"[{_accountId}]: Failure during packet extraction", ex);
                    wrapper.CompletionSource.SetResult(false);
                }
            });
            _blockParsersRepositoriesRepository = blockParsersRepositoriesRepository;
        }

        public virtual void Initialize(long accountId)
        {
            _accountId = accountId;
        }

        public abstract string Name { get; }

        protected abstract bool CheckPacketWitness(PacketWitness packetWitness);

        public virtual ISourceBlock<T> GetSourcePipe<T>(string name = null)
        {
            if (typeof(T) == typeof(PacketWrapper))
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
