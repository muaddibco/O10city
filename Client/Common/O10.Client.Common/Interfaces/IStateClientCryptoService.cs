using O10.Core.Architecture;
using O10.Core.Identity;
using O10.Crypto.Models;

namespace O10.Client.Common.Interfaces
{
    [ServiceContract]
    public interface IStateClientCryptoService : IClientCryptoService
    {
        EcdhTupleCA EncodeEcdhTuple(byte[] blindingFactor, byte[] assetId);

        byte[] DecodeCommitment(byte[] encodedCommitment, byte[] transactionKey);

        void GetBoundedCommitment(byte[] assetId, out byte[] assetCommitment, out byte[] keyImage, out RingSignature ringSignature);

        IKey GetPublicKey();
    }
}
