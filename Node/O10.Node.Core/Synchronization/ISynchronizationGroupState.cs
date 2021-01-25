using O10.Core.Communication;
using O10.Core.Identity;

namespace O10.Node.Core.Synchronization
{
    public interface ISynchronizationGroupState : INeighborhoodState
    {
        bool CheckParticipant(IKey key);
    }
}
