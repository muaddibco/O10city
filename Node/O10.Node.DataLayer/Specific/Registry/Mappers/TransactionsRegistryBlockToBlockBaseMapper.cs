using O10.Core.Architecture;
using O10.Core.Models;
using O10.Core.Translators;
using O10.Transactions.Core.Ledgers.Registry;
using RegistryFullBlockDb = O10.Node.DataLayer.Specific.Registry.Model.RegistryFullBlock;

namespace O10.Node.DataLayer.Mappers.Registry
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionsRegistryBlockToBlockBaseMapper : TranslatorBase<RegistryFullBlockDb, RegistryPacket>
    {
        public override RegistryPacket Translate(RegistryFullBlockDb obj)
        {
            if(obj == null)
            {
                return null;
            }

            return SerializableEntity.Create<RegistryPacket>(obj.Content);
        }
    }
}
