using O10.Core.Architecture;

namespace O10.Core.Cryptography
{
    [ServiceContract]
    public interface ISigningServicesRepository : IRepository<ISigningService, string>
    {
    }
}
