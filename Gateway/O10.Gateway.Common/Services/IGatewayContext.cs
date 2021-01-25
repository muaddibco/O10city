using O10.Core.Cryptography;
using O10.Core.Identity;
using O10.Core.States;
using O10.Transactions.Core.DataModel;
using System;
using System.Threading.Tasks;

namespace O10.Gateway.Common.Services
{
    public interface IGatewayContext : IState, IDisposable
    {
        IKey AccountKey { get; }

        void Initialize(ISigningService signingService);

        Task<StatePacketInfo> GetLastPacketInfo();

        ISigningService SigningService { get; }
    }
}
