using O10.Core;
using O10.Core.Architecture;
using O10.Core.Cryptography;

namespace O10.Crypto.Services
{
    [ServiceContract]
    public interface ISigningServicesRepository : IRepository<ISigningService, string>
    {
    }
}
