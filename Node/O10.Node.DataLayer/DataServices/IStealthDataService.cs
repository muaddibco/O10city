using O10.Node.DataLayer.DataServices.Keys;
using System.Threading;
using System.Threading.Tasks;

namespace O10.Node.DataLayer.DataServices
{
    public interface IStealthDataService : IChainDataService
    {
        Task<byte[]> GetPacketHash(IDataKey dataKey, CancellationToken cancellationToken);
    }
}
