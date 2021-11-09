using Chaos.NaCl;
using System;
using O10.Core;
using O10.Core.Architecture;
using O10.Core.ExtensionMethods;
using O10.Core.HashCalculations;
using O10.Core.Identity;
using O10.Crypto.Exceptions;
using O10.Crypto.Properties;
using System.Text;
using O10.Crypto.Models;
using O10.Core.Logging;
using System.Linq;

namespace O10.Crypto.Services
{
    [RegisterExtension(typeof(ISigningService), Lifetime = LifetimeManagement.Transient)]
    public class Ed25519SigningService : ISigningService
    {
        protected byte[] _secretKey;
        protected byte[] _expandedPrivateKey;
        private readonly ILogger _logger;

        public Ed25519SigningService(IHashCalculationsRepository hashCalculationRepository, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ILoggerService loggerService)
        {
            DefaultHashCalculation = hashCalculationRepository.Create(Globals.DEFAULT_HASH);
            IdentityKeyProvider = identityKeyProvidersRegistry.GetInstance();
            PublicKeys = new IKey[1];
            _logger = loggerService.GetLogger(nameof(Ed25519SigningService));
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

        public SignatureBase Sign<T>(PayloadBase<T> payload, object? args = null) where T: TransactionBase
        {
            if (payload is null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if (!(payload.Transaction is SingleSourceTransactionBase singleSourceTransaction))
            {
                throw new ArgumentOutOfRangeException(nameof(payload), string.Format(Resources.ERR_WRONG_BODY_TYPE, nameof(Ed25519SigningService), typeof(SingleSourceTransactionBase).FullName));
            }

            singleSourceTransaction.Source = PublicKeys[0];

            return new SingleSourceSignature
            {
                Signature = Sign(payload?.ToString() ?? throw new ArgumentNullException(nameof(payload)), args),
            };
        }

        public byte[] Sign(Memory<byte> msg, object? args = null)
        {
            var signature = new byte[Ed25519.SignatureSizeInBytes];
            Memory<byte> memory;
            if(args != null)
            {
                var argsBytes = args.Serialize();
                var bytes = new byte[msg.Length + argsBytes?.Length ?? 0];
                if (bytes.Length > 0)
                {
                    Array.Copy(argsBytes, 0, bytes, msg.Length, argsBytes?.Length ?? 0);
                }

                memory = new Memory<byte>(bytes);
                msg.CopyTo(memory);
            }
            else
            {
                memory = msg;
            }

            Ed25519.Sign(new ArraySegment<byte>(signature), memory.ToArraySegment(), new ArraySegment<byte>(_expandedPrivateKey));
            return signature;
        }

        public byte[] Sign(string msg, object? args = null)
        {
            byte[] message = Encoding.UTF8.GetBytes(msg);
            byte[] signature = Sign(message, args);
            return signature;
        }

        public bool Verify<T>(IPayload<T> payload, SignatureBase signatureBase) where T: TransactionBase
        {
            if (payload is null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if (signatureBase is null)
            {
                throw new ArgumentNullException(nameof(signatureBase));
            }

            if (!(payload.GetTransaction() is SingleSourceTransactionBase singleSourceTransaction))
            {
                throw new ArgumentOutOfRangeException(nameof(payload), string.Format(Resources.ERR_WRONG_BODY_TYPE, nameof(Ed25519SigningService), typeof(SingleSourceTransactionBase).FullName));
            }

            if (!(signatureBase is SingleSourceSignature signature))
            {
                throw new ArgumentOutOfRangeException(nameof(signatureBase), string.Format(Resources.ERR_WRONG_SIGNATURE_TYPE, nameof(Ed25519SigningService), typeof(SingleSourceSignature).FullName));
            }

            byte[] message = Encoding.UTF8.GetBytes(payload.ToString());

            return Ed25519.Verify(signature.Signature.ToArray(), message, singleSourceTransaction.Source.ToByteArray());
        }

        public bool CheckTarget(params IKey[] targetValues)
        {
            _logger.LogIfDebug(() => $"Checking target for {string.Join(',', targetValues.Select(s => s.ToString()))} and Public Key {PublicKeys[0].Value}");

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
