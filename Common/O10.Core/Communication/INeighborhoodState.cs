using System.Collections.Generic;
using O10.Core.Identity;
using O10.Core.States;

namespace O10.Core.Communication
{
    public interface INeighborhoodState : IState
    {
        bool AddNeighbor(IKey key);

        bool RemoveNeighbor(IKey key);

        IEnumerable<IKey> GetAllNeighbors();
    }
}
