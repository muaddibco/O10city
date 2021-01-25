using O10.Core.Architecture;

namespace O10.Core.HashCalculations
{
    [ServiceContract]
    public interface IHashCalculationsRepository : IFactory<IHashCalculation, HashType>
    {
    }
}
