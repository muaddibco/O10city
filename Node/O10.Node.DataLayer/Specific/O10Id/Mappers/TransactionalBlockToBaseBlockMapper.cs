using O10.Node.DataLayer.Specific.O10Id.Model;
using O10.Core.Architecture;
using O10.Core.Translators;
using O10.Transactions.Core.Ledgers;

namespace O10.Node.DataLayer.Specific.O10Id.Mappers
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionalBlockToBaseBlockMapper : TranslatorBase<O10Transaction, IPacketBase>
    {
        public override IPacketBase Translate(O10Transaction transactionalBlock)
        {
            if(transactionalBlock == null)
            {
                return null;
            }

            return PacketBase.Create(transactionalBlock.Content);
        }
    }
}
