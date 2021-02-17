using O10.Transactions.Core.Ledgers.Synchronization;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Parsers;
using O10.Node.DataLayer.Specific.Synchronization.Model;
using O10.Core.Architecture;
using O10.Core.Models;
using O10.Core.Translators;

namespace O10.Node.DataLayer.Specific.Synchronization.Mappers
{

    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationBlockToBlockBaseMapper : TranslatorBase<SynchronizationBlock, PacketBase>
    {
        private readonly IBlockParsersRepository _blockParsersRepository;

        public SynchronizationBlockToBlockBaseMapper(IBlockParsersRepositoriesRepository blockParsersFactoriesRepository)
        {
            if (blockParsersFactoriesRepository is null)
            {
                throw new System.ArgumentNullException(nameof(blockParsersFactoriesRepository));
            }

            _blockParsersRepository = blockParsersFactoriesRepository.GetBlockParsersRepository(LedgerType.Synchronization);
        }

        public override PacketBase Translate(SynchronizationBlock synchronizationBlock)
        {
            if(synchronizationBlock == null)
            {
                return null;
            }

            IBlockParser blockParser = _blockParsersRepository.GetInstance(PacketTypes.Synchronization_ConfirmedBlock);

            return blockParser.Parse(synchronizationBlock.BlockContent) as SynchronizationConfirmedBlock;
        }
    }
}
