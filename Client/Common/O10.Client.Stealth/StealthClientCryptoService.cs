﻿using System;
using O10.Core.Architecture;
using O10.Core.ExtensionMethods;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Crypto.ConfidentialAssets;
using O10.Crypto.Exceptions;
using O10.Crypto.Models;
using O10.Crypto.Services;

namespace O10.Client.Stealth
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

        public void DecodeEcdhTuple(EcdhTupleCA ecdhTupleCA, IKey transactionPublicKey, out byte[] blindingFactor, out byte[] assetId)
        {
            //byte[] otsk = ConfidentialAssetsHelper.GetOTSK(transactionPublicKey, _secretViewKey, _secretSpendKey);
            CryptoHelper.DecodeEcdhTuple(ecdhTupleCA, transactionPublicKey.ToByteArray(), _secretViewKey, out blindingFactor, out assetId);
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

        public byte[] GetKeyImage(IKey transactionPublicKey)
        {
            if (transactionPublicKey is null)
            {
                throw new ArgumentNullException(nameof(transactionPublicKey));
            }

            byte[] otsk = CryptoHelper.GetOTSK(transactionPublicKey.Value, _secretViewKey, _secretSpendKey);
            byte[] keyImage = CryptoHelper.GenerateKeyImage(otsk);

            return keyImage;
        }

        public byte[] GetBlindingFactor(IKey transactionPublicKey)
        {
            if (transactionPublicKey is null)
            {
                throw new ArgumentNullException(nameof(transactionPublicKey));
            }

            return GetBlindingFactor(transactionPublicKey.Value);
        }

        public byte[] GetBlindingFactor(Memory<byte> transactionPublicKey) => CryptoHelper.GetOTSK(transactionPublicKey, _secretViewKey);
    }
}
