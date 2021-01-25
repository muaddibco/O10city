using HashLib;
using O10.Core.Architecture;

using O10.Core.HashCalculations;

namespace O10.Crypto.HashCalculations
{
    [RegisterExtension(typeof(IHashCalculation), Lifetime = LifetimeManagement.Transient)]
    public class Tiger4HashCalculation : HashCalculationBase
    {
        public override HashType HashType => HashType.Tiger4;

        public Tiger4HashCalculation()
            : base(HashFactory.Crypto.CreateTiger_4_192())
        {
        }
    }
}
