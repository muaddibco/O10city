using O10.Transactions.Core.Enums;
using O10.Core;
using O10.Core.Architecture;

namespace O10.Transactions.Core.Parsers
{
    [ExtensionPoint]
    public interface IBlockParsersRepository : IRepository<IBlockParser, ushort>
    {
        LedgerType LedgerType { get; }
    }
}
