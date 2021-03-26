using O10.Node.DataLayer.Specific.Stealth.Model;
using O10.Core.Architecture;
using O10.Core.Translators;
using O10.Transactions.Core.Ledgers;
using O10.Core.Models;

namespace O10.Node.DataLayer.Specific.Stealth.Mappers
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
	public class StealthBlockToBaseBlockMapper : TranslatorBase<StealthTransaction, IPacketBase>
	{
		public override IPacketBase Translate(StealthTransaction block)
		{
            if (block is null)
            {
                throw new System.ArgumentNullException(nameof(block));
            }

            return SerializableEntity<IPacketBase>.Create(block.Content);
		}
	}
}
