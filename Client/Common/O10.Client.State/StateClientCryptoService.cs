﻿using Chaos.NaCl;
using O10.Client.Common.Interfaces;
using O10.Core.Architecture;
using O10.Core.ExtensionMethods;
using O10.Core.HashCalculations;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Crypto.ConfidentialAssets;
using O10.Crypto.Models;
using O10.Crypto.Services;
using System.Collections.Generic;

namespace O10.Client.State
{
    [RegisterDefaultImplementation(typeof(IStateClientCryptoService), Lifetime = LifetimeManagement.Scoped)]
    public class StateClientCryptoService : Ed25519SigningService, IStateClientCryptoService
    {
        private byte[] _blindingSecretKey;
        private IKey _publicKey;

        public StateClientCryptoService(IHashCalculationsRepository hashCalculationRepository, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ILoggerService loggerService)
            : base(hashCalculationRepository, identityKeyProvidersRegistry, loggerService)
        {
        }

        public byte[] DecodeCommitment(byte[] encodedCommitment, byte[] transactionKey)
        {
            return CryptoHelper.DecodeCommitment(encodedCommitment, transactionKey, _secretKey);
        }

        public void DecodeEcdhTuple(EcdhTupleCA ecdhTupleCA, IKey transactionKey, out byte[] blindingFactor, out byte[] assetId)
        {
            CryptoHelper.DecodeEcdhTuple(ecdhTupleCA, (transactionKey ?? PublicKeys[0]).ToByteArray(), _secretKey, out blindingFactor, out assetId);
        }

        public void DecodeEcdhTuple(EcdhTupleIP ecdhTupleCA, byte[] transactionKey, out byte[] issuer, out byte[] payload)
        {
            //ConfidentialAssetsHelper.DecodeEcdhTuple(ecdhTupleCA, transactionKey, _secretKey, out blindingFactor, out assetId);

            issuer = ecdhTupleCA.Issuer;
            payload = ecdhTupleCA.Payload;
        }

        public void DecodeEcdhTuple(EcdhTupleProofs ecdhTuple, byte[] transactionKey, out byte[] blindingFactor, out byte[] assetId, out byte[] issuer, out byte[] payload)
        {
            //ConfidentialAssetsHelper.DecodeEcdhTuple(ecdhTuple, transactionKey, _secretKey, out blindingFactor, out assetId, out issuer, out payload);
            blindingFactor = ecdhTuple.Mask;
            assetId = ecdhTuple.AssetId;
            issuer = ecdhTuple.AssetIssuer;
            payload = ecdhTuple.Payload;
        }

        public EcdhTupleCA EncodeEcdhTuple(byte[] blindingFactor, byte[] assetId)
        {
            EcdhTupleCA ecdhTupleCA = CryptoHelper.CreateEcdhTupleCA(blindingFactor, assetId, _secretKey, PublicKeys[0].ArraySegment.Array);

            return ecdhTupleCA;
        }

        /// <summary>
        /// TODO - need to rename to update while this function retuns nothing and just updates out params
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="assetCommitment"></param>
        /// <param name="keyImage"></param>
        /// <param name="ringSignature"></param>
        public void GetBoundedCommitment(byte[] assetId, out byte[] assetCommitment, out byte[] keyImage, out RingSignature ringSignature)
        {
            keyImage = CryptoHelper.GenerateKeyImage(_blindingSecretKey);
            byte[] nonBlindedCommitment = CryptoHelper.GetNonblindedAssetCommitment(assetId);
            assetCommitment = CryptoHelper.GetAssetCommitment(_blindingSecretKey, assetId);
            byte[] pk = CryptoHelper.SubCommitments(assetCommitment, nonBlindedCommitment);
            ringSignature = CryptoHelper.GenerateRingSignature(assetCommitment, keyImage, new List<IKey> { IdentityKeyProvider.GetKey(pk) }, _blindingSecretKey, 0)[0];
        }

        public IKey GetPublicKey()
        {
            return _publicKey;
        }

        public override void Initialize(params byte[][] secretKeys)
        {
            base.Initialize(secretKeys);

            _publicKey = IdentityKeyProvider.GetKey(CryptoHelper.GetPublicKey(Ed25519.SecretKeyFromSeed(_secretKey)));
            _blindingSecretKey = CryptoHelper.FastHash256(_secretKey);
            _blindingSecretKey = CryptoHelper.ReduceScalar32(_blindingSecretKey);
        }
    }
}
