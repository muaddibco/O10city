using System;
using O10.Client.Common.Interfaces;
using O10.Core.Architecture;

using O10.Core.Cryptography;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Crypto.ConfidentialAssets;
using O10.Crypto.Exceptions;
using O10.Crypto.Services;

namespace O10.Client.Common.Crypto
{
    [RegisterDefaultImplementation(typeof(IStealthClientCryptoService), Lifetime = LifetimeManagement.Scoped)]
	public class StealthClientCryptoService : StealthSigningService, IStealthClientCryptoService
	{
		public StealthClientCryptoService(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ILoggerService loggerService) 
			: base(identityKeyProvidersRegistry, loggerService)
		{
		}

		public override void Initialize(params byte[][] secretKeys)
		{
			if (secretKeys == null)
			{
				throw new ArgumentNullException(nameof(secretKeys));
			}

			if (secretKeys.Length != 2)
			{
				throw new WrongSecretKeysNumberException(nameof(StealthClientCryptoService), 2);
			}

			base.Initialize(secretKeys[0], secretKeys[1]);
		}

		public void DecodeEcdhTuple(EcdhTupleCA ecdhTupleCA, byte[] transactionPublicKey, out byte[] blindingFactor, out byte[] assetId)
		{
			//byte[] otsk = ConfidentialAssetsHelper.GetOTSK(transactionPublicKey, _secretViewKey, _secretSpendKey);
			ConfidentialAssetsHelper.DecodeEcdhTuple(ecdhTupleCA, transactionPublicKey, _secretViewKey, out blindingFactor, out assetId);
		}

		public void DecodeEcdhTuple(EcdhTupleIP ecdhTuple, byte[] transactionKey, out byte[] issuer, out byte[] payload)
		{
			//ConfidentialAssetsHelper.DecodeEcdhTuple(ecdhTupleCA, transactionKey, _secretKey, out blindingFactor, out assetId);

			issuer = ecdhTuple.Issuer;
			payload = ecdhTuple.Payload;
		}

		public void DecodeEcdhTuple(EcdhTupleProofs ecdhTuple, byte[] transactionKey, out byte[] blindingFactor, out byte[] assetId, out byte[] issuer, out byte[] payload)
		{
			//ConfidentialAssetsHelper.DecodeEcdhTuple(ecdhTuple, transactionKey, _secretKey, out blindingFactor, out assetId, out issuer, out payload);
			blindingFactor = ecdhTuple.Mask;
			assetId = ecdhTuple.AssetId;
			issuer = ecdhTuple.AssetIssuer;
			payload = ecdhTuple.Payload;
		}

        public byte[] GetKeyImage(byte[] transactionPublicKey)
		{
			byte[] otsk = ConfidentialAssetsHelper.GetOTSK(transactionPublicKey, _secretViewKey, _secretSpendKey);
			byte[] keyImage = ConfidentialAssetsHelper.GenerateKeyImage(otsk);

			return keyImage;
		}
	}
}
