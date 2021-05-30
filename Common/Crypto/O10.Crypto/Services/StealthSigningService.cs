using System;
using System.Linq;
using System.Text;
using O10.Core.Architecture;
using O10.Core.Cryptography;
using O10.Core.ExtensionMethods;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Crypto.ConfidentialAssets;
using O10.Crypto.Exceptions;
using O10.Crypto.Models;
using O10.Crypto.Properties;

namespace O10.Crypto.Services
{
    [RegisterExtension(typeof(ISigningService), Lifetime = LifetimeManagement.Transient)]
    public class StealthSigningService : ISigningService
    {
        protected byte[] _secretSpendKey;
        protected byte[] _secretViewKey;
        private readonly IIdentityKeyProvider _identityKeyProvider;
        private readonly ILogger _logger;

        public StealthSigningService(
            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
            ILoggerService loggerService)
        {
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
            PublicKeys = new IKey[2];
            _logger = loggerService.GetLogger(GetType().Name);
        }

        public IKey[] PublicKeys { get; private set; }

        public string Name => GetType().Name;

        public virtual void Initialize(params byte[][] secretKeys)
        {
            if (secretKeys == null)
            {
                throw new ArgumentNullException(nameof(secretKeys));
            }

            if (secretKeys.Length != 2)
            {
                throw new WrongSecretKeysNumberException(nameof(StealthSigningService), 2);
            }

            _secretSpendKey = secretKeys[0];
            _secretViewKey = secretKeys[1];
            PublicKeys[0] = _identityKeyProvider.GetKey(CryptoHelper.GetPublicKey(_secretSpendKey));
            PublicKeys[1] = _identityKeyProvider.GetKey(CryptoHelper.GetPublicKey(_secretViewKey));
        }

        public byte[] Sign(string msg, object? args = null)
        {
            byte[] message = Encoding.UTF8.GetBytes(msg);
            return Sign(message, args);
        }

        public byte[] Sign(Memory<byte> msg, object? args = null)
        {
            if (!(args is StealthSignatureInput signatureInput))
            {
                throw new ArgumentNullException(nameof(args), $"{nameof(StealthSigningService)} expects argument args of type {nameof(StealthSignatureInput)}");
            }

            int index = signatureInput.KeyPosition;
            byte[] otsk = CryptoHelper.GetOTSK(signatureInput.SourceTransactionKey.Value, _secretViewKey, _secretSpendKey);

            byte[] keyImage = CryptoHelper.GenerateKeyImage(otsk);

            byte[] msg2 = new byte[msg.Length + keyImage.Length];

            Array.Copy(msg.ToArray(), 0, msg2, 0, msg.Length);
            Array.Copy(keyImage, 0, msg2, msg.Length, keyImage.Length);

            RingSignature[] ringSignatures = CryptoHelper.GenerateRingSignature(msg2, keyImage, signatureInput.PublicKeys, otsk, index);

            byte[] signature = new byte[64 * ringSignatures.Length];
            for (int i = 0; i < ringSignatures.Length; i++)
            {
                byte[] sig = ringSignatures[i].ToByteArray();
                Array.Copy(sig, 0, signature, 64 * i, 64);
            }

            return signature;
        }

        public SignatureBase Sign<T>(PayloadBase<T> payload, object? args = null) where T: TransactionBase
        {
            if (payload is null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if (!(payload.Transaction is StealthTransactionBase transaction))
            {
                throw new ArgumentOutOfRangeException(nameof(payload));
            }

            if (!(args is StealthSignatureInput signatureInput))
            {
                throw new ArgumentException($"{nameof(StealthSigningService)} expects argument args of type {nameof(StealthSignatureInput)}", nameof(args));
            }

            int index = signatureInput.KeyPosition;
            byte[] otsk = CryptoHelper.GetOTSK(signatureInput.SourceTransactionKey.Value, _secretViewKey, _secretSpendKey);

            byte[] keyImage = CryptoHelper.GenerateKeyImage(otsk);
            transaction.KeyImage = _identityKeyProvider.GetKey(keyImage);

            signatureInput.PreSigningAction?.Invoke(transaction);

            RingSignature[] ringSignatures = CryptoHelper.GenerateRingSignature(payload.ToByteArray(), keyImage, signatureInput.PublicKeys, otsk, index);

            return new StealthSignature
            {
                Sources = signatureInput.PublicKeys,
                Signature = ringSignatures
            };
        }

        public bool Verify<T>(PayloadBase<T> payload, SignatureBase signatureBase) where T: TransactionBase
        {
            if (payload is null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if (!(payload.Transaction is StealthTransactionBase transaction))
            {
                throw new ArgumentOutOfRangeException(nameof(payload), string.Format(Resources.ERR_WRONG_BODY_TYPE, nameof(StealthSigningService), typeof(StealthTransactionBase).FullName));
            }

            if (!(signatureBase is StealthSignature stealthSignature))
            {
                throw new ArgumentOutOfRangeException(nameof(signatureBase), string.Format(Resources.ERR_WRONG_SIGNATURE_TYPE, nameof(StealthSigningService), typeof(StealthSignature).FullName));
            }

            if(transaction.KeyImage == null)
            {
                throw new ArgumentException($"payload's {nameof(transaction.KeyImage)} is null");
            }

            byte[] msg = transaction.ToByteArray();
            byte[] keyImage = transaction.KeyImage.ToByteArray();

            return CryptoHelper.VerifyRingSignature(msg, keyImage, stealthSignature.Sources.Select(p => p.Value.ToArray()).ToArray(), stealthSignature.Signature.ToArray());
        }

        public bool CheckTarget(params IKey[] targetValues)
        {
            if (targetValues == null)
            {
                throw new ArgumentNullException(nameof(targetValues));
            }

            if (targetValues.Length != 2)
            {
                throw new ArgumentOutOfRangeException(nameof(targetValues));
            }

            try
            {
                _logger.LogIfDebug(() => $"{nameof(CryptoHelper)}.{nameof(CryptoHelper.IsDestinationKeyMine)}({targetValues[0]}, {targetValues[1]}, {_secretViewKey.ToHexString()}, {PublicKeys[0].Value.ToArray().ToHexString()})");
                bool res = CryptoHelper.IsDestinationKeyMine(targetValues[0].ToByteArray(), targetValues[1].ToByteArray(), _secretViewKey, PublicKeys[0].Value.ToArray());
                return res;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failure at {nameof(CryptoHelper)}.{nameof(CryptoHelper.IsDestinationKeyMine)}", ex);
                _logger.Error($"args: {nameof(CryptoHelper)}.{nameof(CryptoHelper.IsDestinationKeyMine)}({targetValues[0]}, {targetValues[1]}, {_secretViewKey.ToHexString()}, {PublicKeys[0].Value.ToArray().ToHexString()})");
                throw;
            }
        }
    }
}

