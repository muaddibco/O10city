using System.Collections.Generic;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;


namespace O10.Transactions.Core.Parsers.Factories
{
    [RegisterExtension(typeof(IBlockParsersRepository), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionalBlockParsersFactory : BlockParsersRepositoryBase
    {
        public TransactionalBlockParsersFactory(IEnumerable<IBlockParser> blockParsers) : base(blockParsers)
        {
        }

        public override LedgerType PacketType => LedgerType.O10State;
    }
}
