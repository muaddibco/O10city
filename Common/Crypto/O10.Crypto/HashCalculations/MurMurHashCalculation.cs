using HashLib;
using O10.Core.Architecture;

using O10.Core.HashCalculations;

namespace O10.Crypto.HashCalculations
{
    [RegisterExtension(typeof(IHashCalculation), Lifetime = LifetimeManagement.Transient)]
    public class MurMurHashCalculation : HashCalculationBase
    {
        public override HashType HashType => HashType.MurMur;

        public MurMurHashCalculation() 
            : base(HashFactory.Hash128.CreateMurmur3_128())
        {
        }
    }
}
