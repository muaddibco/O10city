using System.Collections.Generic;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;


namespace O10.Transactions.Core.Parsers.Factories
{
    [RegisterExtension(typeof(IBlockParsersRepository), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationBlockParsersFactory : BlockParsersRepositoryBase
    {
        public SynchronizationBlockParsersFactory(IEnumerable<IBlockParser> blockParsers) : base(blockParsers)
        {
        }

        public override PacketType PacketType => PacketType.Synchronization;
    }
}
