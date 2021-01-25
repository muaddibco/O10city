using O10.Core;
using O10.Core.Architecture;
using O10.Core.Models;
using O10.Gateway.DataLayer.Services;
using O10.Transactions.Core.Accessors;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Exceptions;
using O10.Transactions.Core.Parsers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

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
        private readonly IBlockParsersRepository _blockParsersRepository;

        public O10StealthAccessor(IDataAccessService dataAccessService, IBlockParsersRepositoriesRepository blockParsersRepositoriesRepository)
        {
            _dataAccessService = dataAccessService;
            _blockParsersRepository = blockParsersRepositoriesRepository.GetBlockParsersRepository(PacketType.Stealth);
        }

        public override PacketType PacketType => PacketType.Stealth;

        protected override IEnumerable<string> GetAccessingKeys() => AccessingKeys;

        protected override PacketBase GetPacketInner(EvidenceDescriptor accessDescriptor)
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

            var stealthPacket = _dataAccessService.GetStealthPacket(syncBlockHeight, combinedRegistryBlockHeight, accessDescriptor.Parameters[Hash]);
            var parser = _blockParsersRepository.GetInstance(stealthPacket.BlockType);

            var packet = parser.Parse(stealthPacket.Content);
            return packet;
        }
    }
}
