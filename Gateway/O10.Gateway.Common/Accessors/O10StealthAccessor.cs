using Flurl;
using Flurl.Http;
using O10.Core;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.Logging;
using O10.Gateway.Common.Configuration;
using O10.Gateway.DataLayer.Model;
using O10.Gateway.DataLayer.Services;
using O10.Transactions.Core.Accessors;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using O10.Transactions.Core.DTOs;
using O10.Crypto.Models;

namespace O10.Gateway.Common.Accessors
{
    /// <summary>
    /// Obtains Stealth packets using their Hash
    /// </summary>
    [RegisterExtension(typeof(IAccessor), Lifetime = LifetimeManagement.Singleton)]
    public class O10StealthAccessor : AccessorBase
    {
        public const string SyncBlockHeight = "SyncBlockHeight";
        public const string CombinedRegistryBlockHeight = "CombinedRegistryBlockHeight";
        public const string Hash = "Hash";

        public static readonly ReadOnlyCollection<string> AccessingKeys 
            = new ReadOnlyCollection<string>(new[] { "SyncBlockHeight", "CombinedRegistryBlockHeight", "Hash" });

        private readonly IDataAccessService _dataAccessService;
        private readonly ISynchronizerConfiguration _synchronizerConfiguration;
        private readonly ILogger _logger;

        public O10StealthAccessor(IDataAccessService dataAccessService,
                                  IConfigurationService configurationService,
                                  ILoggerService loggerService)
        {
            _dataAccessService = dataAccessService;
            _synchronizerConfiguration = configurationService.Get<ISynchronizerConfiguration>();
            _logger = loggerService.GetLogger(nameof(O10StealthAccessor));
        }

        public override LedgerType LedgerType => LedgerType.Stealth;

        protected override IEnumerable<string> GetAccessingKeys() => AccessingKeys;

        protected override async Task<TransactionBase> GetTransactionInner(EvidenceDescriptor accessDescriptor)
        {
            if(!long.TryParse(accessDescriptor.Parameters[SyncBlockHeight], out long syncBlockHeight))
            {
                throw new AccessorValidationFailedException($"{SyncBlockHeight} is corrupted");
            }

            if (!long.TryParse(accessDescriptor.Parameters[CombinedRegistryBlockHeight], out long combinedRegistryBlockHeight))
            {
                throw new AccessorValidationFailedException($"{CombinedRegistryBlockHeight} is corrupted");
            }

            if (accessDescriptor.Parameters[Hash]?.Length != Globals.DEFAULT_HASH_SIZE * 2)
            {
                throw new AccessorValidationFailedException($"{Hash} is corrupted");
            }

            var stealthPacket = _dataAccessService.GetStealthTransaction(combinedRegistryBlockHeight, accessDescriptor.Parameters[Hash]);
            if(stealthPacket != null)
            {
                var parser = _blockParsersRepository.GetInstance(stealthPacket.TransactionType);

                var packet = parser.Parse(stealthPacket.Content);
                return packet;
            }
            else
            {
                Url url = _synchronizerConfiguration.NodeApiUri
                    .AppendPathSegment("StealthTransaction")
                    .SetQueryParam("combinedBlockHeight", combinedRegistryBlockHeight)
                    .SetQueryParam("hash", accessDescriptor.Parameters[Hash]);

                _logger.Info($"Querying Stealth packet with URL {url}");
                var transactionInfo = await url.GetJsonAsync<TransactionInfo>().ConfigureAwait(false);
                

                if (transactionInfo.Content?.Any() ?? false)
                {
                    _logger.Info($"Stealth packet with Packet Type {transactionInfo.LedgerType} and BlockType {transactionInfo.PacketType} at SyncBlockHeight {transactionInfo.SyncBlockHeight} obtained");
                    StorePacket(tuple.Item1, t1.Result.Content);
                }
                else
                {
                    _logger.Error($"Empty Stealth packet with Packet Type {transactionInfo.LedgerType} and BlockType {transactionInfo.PacketType} at SyncBlockHeight {transactionInfo.SyncBlockHeight}  obtained");
                }

                Tuple<WitnessPacket, TaskCompletionSource<WitnessPacket>> tuple = (Tuple<WitnessPacket, TaskCompletionSource<WitnessPacket>>)o2;
                    if (t1.IsCompletedSuccessfully)
                    {

                    }
                    else
                    {
                        tuple.Item2.SetException(t1.Exception);

                        _logger.Error($"Failure during obtaining and storing Stealth packet", t1.Exception);
                        foreach (var ex in t1.Exception.InnerExceptions)
                        {
                            _logger.Error(ex.Message);
                        }
                    }
                
            }
        }
    }
}
