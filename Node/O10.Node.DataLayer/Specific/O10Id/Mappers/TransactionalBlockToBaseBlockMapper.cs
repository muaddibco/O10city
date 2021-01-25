using O10.Transactions.Core.Parsers;
using O10.Node.DataLayer.Specific.O10Id.Model;
using O10.Core.Architecture;
using O10.Core.Models;
using O10.Core.Translators;

namespace O10.Node.DataLayer.Specific.O10Id.Mappers
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionalBlockToBaseBlockMapper : TransactionalMapperBase<O10Transaction, PacketBase>
    {
        public TransactionalBlockToBaseBlockMapper(IBlockParsersRepositoriesRepository blockParsersFactoriesRepository)
            : base(blockParsersFactoriesRepository)
        {
        }

        public override PacketBase Translate(O10Transaction transactionalBlock)
        {
            if(transactionalBlock == null)
            {
                return null;
            }

            IBlockParser blockParser = BlockParsersRepository.GetInstance(transactionalBlock.BlockType);

            return blockParser.Parse(transactionalBlock.BlockContent);
        }
    }
}
