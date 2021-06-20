using O10.Core.Architecture;
using O10.Core.Translators;
using O10.Transactions.Core.Ledgers;
using O10.Core.Models;
using O10.Transactions.Core.Ledgers.Stealth;

namespace O10.Node.DataLayer.Specific.Stealth.Mappers
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
	public class StealthBlockToBaseBlockMapper : TranslatorBase<Model.StealthTransaction, StealthPacket>
	{
		public override StealthPacket Translate(Model.StealthTransaction block)
		{
            if (block is null)
            {
                throw new System.ArgumentNullException(nameof(block));
            }

            return SerializableEntity.Create<StealthPacket>(block.Content);
		}
	}
}
