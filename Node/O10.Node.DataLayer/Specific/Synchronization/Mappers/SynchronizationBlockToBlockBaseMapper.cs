using O10.Core.Architecture;
using O10.Core.Translators;
using O10.Core.Models;
using O10.Transactions.Core.Ledgers.Synchronization;

namespace O10.Node.DataLayer.Specific.Synchronization.Mappers
{

    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationBlockToBlockBaseMapper : TranslatorBase<Model.SynchronizationPacket, SynchronizationPacket>
    {
        public override SynchronizationPacket Translate(Model.SynchronizationPacket synchronizationBlock)
        {
            if(synchronizationBlock == null)
            {
                throw new System.ArgumentNullException(nameof(synchronizationBlock));
            }

            return SerializableEntity.Create<SynchronizationPacket>(synchronizationBlock.Content);
        }
    }
}
