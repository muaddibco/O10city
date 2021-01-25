using O10.Core.Cryptography;

namespace O10.Client.Common.Interfaces
{
    public interface IClientCryptoService : ISigningService
    {
        bool CheckTarget(params byte[][] targetValues);
        void DecodeEcdhTuple(EcdhTupleCA ecdhTupleCA, byte[] transactionPublicKey, out byte[] blindingFactor, out byte[] assetId);

        void DecodeEcdhTuple(EcdhTupleIP ecdhTuple, byte[] transactionKey, out byte[] issuer, out byte[] payload);

		void DecodeEcdhTuple(EcdhTupleProofs ecdhTuple, byte[] transactionKey, out byte[] blindingFactor, out byte[] assetId, out byte[] issuer, out byte[] payload);
	}
}
