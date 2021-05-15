using O10.Core.Cryptography;
using O10.Core.Identity;

namespace O10.Client.Common.Interfaces
{
    public interface IClientCryptoService : ISigningService
    {
        bool CheckTarget(params IKey[] targetValues);
        void DecodeEcdhTuple(EcdhTupleCA ecdhTupleCA, IKey transactionPublicKey, out byte[] blindingFactor, out byte[] assetId);

        void DecodeEcdhTuple(EcdhTupleIP ecdhTuple, byte[] transactionKey, out byte[] issuer, out byte[] payload);

		void DecodeEcdhTuple(EcdhTupleProofs ecdhTuple, byte[] transactionKey, out byte[] blindingFactor, out byte[] assetId, out byte[] issuer, out byte[] payload);
	}
}
