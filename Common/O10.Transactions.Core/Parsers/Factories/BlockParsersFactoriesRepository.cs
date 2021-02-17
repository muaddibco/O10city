using System.Collections.Generic;
using System.Linq;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;

namespace O10.Transactions.Core.Parsers.Factories
{
    [RegisterDefaultImplementation(typeof(IBlockParsersRepositoriesRepository), Lifetime = LifetimeManagement.Singleton)]
    public class BlockParsersFactoriesRepository : IBlockParsersRepositoriesRepository
    {
        private readonly IEnumerable<IBlockParsersRepository> _blockParsersFactories;

        public BlockParsersFactoriesRepository(IEnumerable<IBlockParsersRepository> blockParsersFactories)
        {
            _blockParsersFactories = blockParsersFactories;
        }

        public IBlockParsersRepository GetBlockParsersRepository(LedgerType packetType)
        {
            return _blockParsersFactories.FirstOrDefault(f => f.PacketType == packetType);
        }
    }
}
