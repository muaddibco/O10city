using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Parsers;
using O10.Node.DataLayer.Specific.Registry.Model;
using O10.Core.Architecture;
using O10.Core.Models;
using O10.Core.Translators;

namespace O10.Node.DataLayer.Mappers.Registry
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionsRegistryBlockToBlockBaseMapper : TranslatorBase<RegistryFullBlock, PacketBase>
    {
        private readonly IBlockParsersRepository _blockParsersRepository;

        public TransactionsRegistryBlockToBlockBaseMapper(IBlockParsersRepositoriesRepository blockParsersFactoriesRepository)
        {
            if (blockParsersFactoriesRepository is null)
            {
                throw new System.ArgumentNullException(nameof(blockParsersFactoriesRepository));
            }

            _blockParsersRepository = blockParsersFactoriesRepository.GetBlockParsersRepository(LedgerType.Registry);
        }

        public override PacketBase Translate(RegistryFullBlock obj)
        {
            if(obj == null)
            {
                return null;
            }

            IBlockParser blockParser = _blockParsersRepository.GetInstance(PacketTypes.Registry_FullBlock);

			return (Transactions.Core.Ledgers.Registry.RegistryFullBlock)blockParser.Parse(obj.Content);
        }
    }
}
