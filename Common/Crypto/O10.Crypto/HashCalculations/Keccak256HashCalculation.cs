using HashLib;
using O10.Core.Architecture;

using O10.Core.HashCalculations;

namespace O10.Crypto.HashCalculations
{
    [RegisterExtension(typeof(IHashCalculation), Lifetime = LifetimeManagement.Transient)]
    public class Keccak256HashCalculation : HashCalculationBase
    {
        public override HashType HashType => HashType.Keccak256;

        public Keccak256HashCalculation()
            : base(HashFactory.Crypto.SHA3.CreateKeccak256())
        {
        }
    }
}
