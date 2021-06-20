using O10.Core.Architecture;
using O10.Core.Translators;
using O10.Core.Models;
using O10.Transactions.Core.Ledgers.Synchronization;

namespace O10.Node.DataLayer.Specific.Synchronization.Mappers
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class RegistryCombinedBlockToBlockBaseMapper : TranslatorBase<Model.AggregatedRegistrationsTransaction, SynchronizationPacket>
    {
        public override SynchronizationPacket Translate(Model.AggregatedRegistrationsTransaction registryCombinedBlock)
        {
            if (registryCombinedBlock == null)
            {
                return null;
            }

            return SerializableEntity.Create<SynchronizationPacket>(registryCombinedBlock.Content);
        }
    }
}
