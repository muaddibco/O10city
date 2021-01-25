using O10.Core.Architecture;

namespace O10.Client.Common.Interfaces
{
    [ServiceContract]
    public interface IStealthWalletPacketsExtractor : IPacketsExtractor
    {
        long AccountId { get; set; }
        void Initialize(IClientCryptoService clientCryptoService);
    }
}
