using O10.Core.Architecture;

namespace O10.Network.Handlers
{
    [ServiceContract]
    public interface IHandlingFlowContext
    {
        int Index { get; }
    }
}
