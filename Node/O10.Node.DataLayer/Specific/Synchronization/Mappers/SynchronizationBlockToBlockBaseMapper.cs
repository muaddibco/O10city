using O10.Core.Architecture;
using O10.Core.Translators;
using O10.Transactions.Core.Ledgers;
using O10.Core.Models;

namespace O10.Node.DataLayer.Specific.Synchronization.Mappers
{

    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationBlockToBlockBaseMapper : TranslatorBase<Model.SynchronizationPacket, IPacketBase>
    {
        public override IPacketBase Translate(Model.SynchronizationPacket synchronizationBlock)
        {
            if(synchronizationBlock == null)
            {
                throw new System.ArgumentNullException(nameof(synchronizationBlock));
            }

            return SerializableEntity<IPacketBase>.Create(synchronizationBlock.Content);
        }
    }
}
