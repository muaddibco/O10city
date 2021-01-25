using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Parsers;
using O10.Core.Translators;

namespace O10.Node.DataLayer.Specific.O10Id.Mappers
{
    public abstract class TransactionalMapperBase<TFrom, TTo> : TranslatorBase<TFrom, TTo>
    {
        protected TransactionalMapperBase(IBlockParsersRepositoriesRepository blockParsersFactoriesRepository)
        {
            if (blockParsersFactoriesRepository is null)
            {
                throw new System.ArgumentNullException(nameof(blockParsersFactoriesRepository));
            }

            BlockParsersRepository = blockParsersFactoriesRepository.GetBlockParsersRepository(PacketType.Transactional);
        }

        protected IBlockParsersRepository BlockParsersRepository { get; }
    }
}
