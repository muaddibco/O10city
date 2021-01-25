using HashLib;
using O10.Core.Architecture;

using O10.Core.HashCalculations;

namespace O10.Crypto.HashCalculations
{
	[RegisterExtension(typeof(IHashCalculation), Lifetime = LifetimeManagement.Transient)]
	public class Sha224HashCalculation : HashCalculationBase
	{
		public override HashType HashType => HashType.Sha224;

		public Sha224HashCalculation()
			: base(HashFactory.Crypto.CreateSHA224())
		{
		}
	}
}
