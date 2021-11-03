using O10.Core;
using O10.Core.Architecture;

namespace O10.Network.Handlers
{
    [ServiceContract]
    public interface ICoreVerifiersBulkFactory : IBulkFactory<ICoreVerifier>
    {
    }
}
