using O10.Core.Architecture;
using O10.Core.Identity;

namespace O10.Client.Common.Interfaces
{
    [ServiceContract]
	public interface IStealthClientCryptoService : IClientCryptoService
    {
		byte[] GetKeyImage(IKey transactionPublicKey);
	}
}
