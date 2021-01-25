using O10.Transactions.Core.Enums;
using O10.Core.Architecture;

namespace O10.Transactions.Core.Parsers
{
    [ServiceContract]
    public interface IBlockParsersRepositoriesRepository
    {
        IBlockParsersRepository GetBlockParsersRepository(PacketType packetType);
    }
}
