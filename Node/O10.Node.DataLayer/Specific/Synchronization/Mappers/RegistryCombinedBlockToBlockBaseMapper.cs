using O10.Node.DataLayer.Specific.Synchronization.Model;
using O10.Core.Architecture;
using O10.Core.Translators;
using O10.Transactions.Core.Ledgers;
using O10.Core.Models;

namespace O10.Node.DataLayer.Specific.Synchronization.Mappers
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class RegistryCombinedBlockToBlockBaseMapper : TranslatorBase<AggregatedRegistrationsTransaction, IPacketBase>
    {
        public override IPacketBase Translate(AggregatedRegistrationsTransaction registryCombinedBlock)
        {
            if (registryCombinedBlock == null)
            {
                throw new System.ArgumentNullException(nameof(registryCombinedBlock));
            }

            return SerializableEntity<IPacketBase>.Create(registryCombinedBlock.Content);
        }
    }
}
