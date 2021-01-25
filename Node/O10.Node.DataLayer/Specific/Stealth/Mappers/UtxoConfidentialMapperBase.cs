using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Parsers;
using O10.Core.Translators;

namespace O10.Node.DataLayer.Specific.Stealth.Mappers
{
	public abstract class StealthMapperBase<TFrom, TTo> : TranslatorBase<TFrom, TTo>
	{
        public StealthMapperBase(IBlockParsersRepositoriesRepository blockParsersFactoriesRepository)
		{
            if (blockParsersFactoriesRepository is null)
            {
                throw new System.ArgumentNullException(nameof(blockParsersFactoriesRepository));
            }

            BlockParsersRepository = blockParsersFactoriesRepository.GetBlockParsersRepository(PacketType.Stealth);
		}

        protected IBlockParsersRepository BlockParsersRepository { get; }
    }
}
