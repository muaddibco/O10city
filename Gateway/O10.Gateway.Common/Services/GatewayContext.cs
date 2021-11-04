using O10.Core.Architecture;
using O10.Core.Cryptography;
using O10.Core.Identity;
using O10.Transactions.Core.DTOs;
using System.Linq;
using System.Threading.Tasks;

namespace O10.Gateway.Common.Services
{
    [RegisterDefaultImplementation(typeof(IGatewayContext), Lifetime = LifetimeManagement.Singleton)]
    public class GatewayContext : IGatewayContext
    {
        private readonly INetworkSynchronizer _networkSynchronizer;

        public GatewayContext(INetworkSynchronizer networkSynchronizer)
        {
            _networkSynchronizer = networkSynchronizer;
        }

        public IKey AccountKey { get; private set; }

        public ISigningService SigningService { get; private set; }

        public void Initialize(ISigningService signingService)
        {
            SigningService = signingService;
            AccountKey = SigningService.PublicKeys[0];
        }

        // TODO: Need to modify so last packet info won't be taken every time from a Node
        public async Task<StatePacketInfo> GetLastPacketInfo()
        {
            return await _networkSynchronizer.GetLastPacketInfo(SigningService.PublicKeys.First()).ConfigureAwait(false);
        }
    }
}
