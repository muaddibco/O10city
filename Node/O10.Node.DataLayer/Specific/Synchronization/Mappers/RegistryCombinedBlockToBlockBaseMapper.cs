using O10.Transactions.Core.DataModel.Synchronization;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Parsers;
using O10.Node.DataLayer.Specific.Synchronization.Model;
using O10.Core.Architecture;
using O10.Core.Models;
using O10.Core.Translators;

namespace O10.Node.DataLayer.Specific.Synchronization.Mappers
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class RegistryCombinedBlockToBlockBaseMapper : TranslatorBase<RegistryCombinedBlock, PacketBase>
    {
        private readonly IBlockParsersRepository _blockParsersRepository;

        public RegistryCombinedBlockToBlockBaseMapper(IBlockParsersRepositoriesRepository blockParsersFactoriesRepository)
        {
            if (blockParsersFactoriesRepository is null)
            {
                throw new System.ArgumentNullException(nameof(blockParsersFactoriesRepository));
            }

            _blockParsersRepository = blockParsersFactoriesRepository.GetBlockParsersRepository(PacketType.Synchronization);
        }

        public override PacketBase Translate(RegistryCombinedBlock registryCombinedBlock)
        {
            if (registryCombinedBlock == null)
            {
                return null;
            }

            IBlockParser blockParser = _blockParsersRepository.GetInstance(ActionTypes.Synchronization_RegistryCombinationBlock);

            SynchronizationRegistryCombinedBlock block = blockParser.Parse(registryCombinedBlock.Content) as SynchronizationRegistryCombinedBlock;
            block.SyncBlockHeight = registryCombinedBlock.SyncBlockHeight;

            return block;
        }
    }
}
