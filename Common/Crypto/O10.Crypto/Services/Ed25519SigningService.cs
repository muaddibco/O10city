using Chaos.NaCl;
using System;
using O10.Core;
using O10.Core.Architecture;
using O10.Core.Cryptography;
using O10.Core.ExtensionMethods;
using O10.Core.HashCalculations;
using O10.Core.Identity;
using O10.Crypto.Exceptions;
using O10.Crypto.Properties;
using System.Text;
using O10.Crypto.Models;

namespace O10.Crypto.Services
{
    [RegisterExtension(typeof(ISigningService), Lifetime = LifetimeManagement.Transient)]
    public class Ed25519SigningService : ISigningService
    {
        protected byte[] _secretKey;
        protected byte[] _expandedPrivateKey;

        public Ed25519SigningService(IHashCalculationsRepository hashCalculationRepository, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry)
        {
            DefaultHashCalculation = hashCalculationRepository.Create(Globals.DEFAULT_HASH);
            IdentityKeyProvider = identityKeyProvidersRegistry.GetInstance();
            PublicKeys = new IKey[1];
        }

        public IKey[] PublicKeys { get; private set; }

        public string Name => GetType().Name;

        protected IIdentityKeyProvider IdentityKeyProvider { get; }

        protected IHashCalculation DefaultHashCalculation { get; }

        public virtual void Initialize(params byte[][] secretKeys)
        {
            if (secretKeys == null)
            {
                throw new ArgumentNullException(nameof(secretKeys));
            }

            if (secretKeys.Length != 1)
            {
                throw new WrongSecretKeysNumberException(nameof(Ed25519SigningService), 1);
            }

            _secretKey = Ed25519.SecretKeyFromSeed(secretKeys[0]);
            Ed25519.KeyPairFromSeed(out byte[] publicKey, out _expandedPrivateKey, secretKeys[0]);

            PublicKeys[0] = IdentityKeyProvider.GetKey(publicKey);
        }

        public SignatureBase Sign(TransactionBase msg, object args = null)
        {
            if (!(msg is SingleSourceTransactionBase singleSourceTransaction))
            {
                throw new ArgumentOutOfRangeException(nameof(msg), string.Format(Resources.ERR_WRONG_BODY_TYPE, nameof(Ed25519SigningService), typeof(SingleSourceTransactionBase).FullName));
            }

            singleSourceTransaction.Source = PublicKeys[0];

            return new SingleSourceSignature
            {
                Signature = Sign(msg?.ToString() ?? throw new ArgumentNullException(nameof(msg))),
            };
        }

        public byte[] Sign(Memory<byte> msg, object args = null)
        {
            var signature = new byte[Ed25519.SignatureSizeInBytes];
            Ed25519.Sign(new ArraySegment<byte>(signature), msg.ToArraySegment(), new ArraySegment<byte>(_expandedPrivateKey));
            return signature;
        }

        public byte[] Sign(string msg, object args = null)
        {
            byte[] message = Encoding.UTF8.GetBytes(msg);
            byte[] signature = Sign(message, args);
            return signature;
        }

        public bool Verify(TransactionBase msg, SignatureBase signatureBase)
        {
            if (msg is null)
            {
                throw new ArgumentNullException(nameof(msg));
            }

            if (signatureBase is null)
            {
                throw new ArgumentNullException(nameof(signatureBase));
            }

            if (!(msg is SingleSourceTransactionBase singleSourceTransaction))
            {
                throw new ArgumentOutOfRangeException(nameof(msg), string.Format(Resources.ERR_WRONG_BODY_TYPE, nameof(Ed25519SigningService), typeof(SingleSourceTransactionBase).FullName));
            }

            if (!(signatureBase is SingleSourceSignature signature))
            {
                throw new ArgumentOutOfRangeException(nameof(signatureBase), string.Format(Resources.ERR_WRONG_SIGNATURE_TYPE, nameof(Ed25519SigningService), typeof(SingleSourceSignature).FullName));
            }

            byte[] message = Encoding.UTF8.GetBytes(msg.ToString());

            return Ed25519.Verify(signature.Signature.ToArray(), message, singleSourceTransaction.Source.ToByteArray());
        }

        public bool CheckTarget(params IKey[] targetValues)
        {
            if (targetValues == null)
            {
                throw new ArgumentNullException(nameof(targetValues));
            }

            if (targetValues.Length != 1)
            {
                throw new ArgumentOutOfRangeException(nameof(targetValues));
            }

            return targetValues[0].Equals(PublicKeys[0].Value);
        }
    }

}
