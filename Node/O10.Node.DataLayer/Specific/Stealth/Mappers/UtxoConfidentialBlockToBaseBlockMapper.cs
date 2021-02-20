using O10.Node.DataLayer.Specific.Stealth.Model;
using O10.Core.Architecture;
using O10.Core.Models;
using O10.Core.Translators;

namespace O10.Node.DataLayer.Specific.Stealth.Mappers
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
	public class StealthBlockToBaseBlockMapper : TranslatorBase<StealthTransaction, PacketBase>
	{
		public override PacketBase Translate(StealthTransaction block)
		{
            if (block is null)
            {
                throw new System.ArgumentNullException(nameof(block));
            }

            return PacketBase.Create<PacketBase>(block.Content);
		}
	}
}
