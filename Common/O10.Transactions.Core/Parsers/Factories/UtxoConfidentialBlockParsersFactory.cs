using System.Collections.Generic;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;


namespace O10.Transactions.Core.Parsers.Factories
{
    [RegisterExtension(typeof(IBlockParsersRepository), Lifetime = LifetimeManagement.Singleton)]
    public class StealthBlockParsersFactory : BlockParsersRepositoryBase
    {
        public StealthBlockParsersFactory(IEnumerable<IBlockParser> blockParsers) : base(blockParsers)
        {
        }

        public override PacketType PacketType => PacketType.Stealth;
    }
}
