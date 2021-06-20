using O10.Node.DataLayer.Specific.O10Id.Model;
using O10.Core.Architecture;
using O10.Core.Translators;
using O10.Core.Models;
using O10.Transactions.Core.Ledgers.O10State;

namespace O10.Node.DataLayer.Specific.O10Id.Mappers
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionalBlockToBaseBlockMapper : TranslatorBase<O10Transaction, O10StatePacket>
    {
        public override O10StatePacket Translate(O10Transaction transactionalBlock)
        {
            if(transactionalBlock == null)
            {
                throw new System.ArgumentNullException(nameof(transactionalBlock));
            }

            return SerializableEntity.Create<O10StatePacket>(transactionalBlock.Content);
        }
    }
}
