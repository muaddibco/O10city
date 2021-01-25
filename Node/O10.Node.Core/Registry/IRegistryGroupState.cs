using O10.Core.Communication;
using O10.Core.Identity;

namespace O10.Node.Core.Registry
{
    public interface IRegistryGroupState : INeighborhoodState
    {
        IKey SyncLayerNode { get; set; }

        int Round { get; set; }

        void ToggleLastBlockConfirmationReceived();

        void WaitLastBlockConfirmationReceived();
    }
}
