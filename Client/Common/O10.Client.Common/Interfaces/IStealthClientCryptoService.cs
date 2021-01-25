using O10.Core.Architecture;

namespace O10.Client.Common.Interfaces
{
    [ServiceContract]
	public interface IStealthClientCryptoService : IClientCryptoService
    {
		byte[] GetKeyImage(byte[] transactionPublicKey);
	}
}
