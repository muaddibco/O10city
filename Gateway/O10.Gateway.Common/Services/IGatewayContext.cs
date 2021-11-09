using O10.Core.Architecture;
using O10.Core.Identity;
using O10.Crypto.Models;
using O10.Transactions.Core.DTOs;
using System.Threading.Tasks;

namespace O10.Gateway.Common.Services
{
    [ServiceContract]
    public interface IGatewayContext
    {
        IKey AccountKey { get; }

        void Initialize(ISigningService signingService);

        Task<StatePacketInfo> GetLastPacketInfo();

        ISigningService SigningService { get; }
    }
}
