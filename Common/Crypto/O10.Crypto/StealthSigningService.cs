using System;
using System.Linq;
using O10.Core.Architecture;
using O10.Core.Cryptography;
using O10.Core.ExtensionMethods;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Core.Models;
using O10.Crypto.ConfidentialAssets;
using O10.Crypto.Exceptions;
using O10.Crypto.Properties;

namespace O10.Crypto
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
            PublicKeys[0] = _identityKeyProvider.GetKey(ConfidentialAssetsHelper.GetPublicKey(_secretSpendKey));
            PublicKeys[1] = _identityKeyProvider.GetKey(ConfidentialAssetsHelper.GetPublicKey(_secretViewKey));
        }

        public byte[] Sign(Memory<byte> msg, object args = null)
        {
            if (!(args is StealthSignatureInput signatureInput))
            {
                throw new ArgumentNullException(nameof(args), $"{nameof(StealthSigningService)} expects argument args of type {nameof(StealthSignatureInput)}");
            }

            byte[][] publicKeys = signatureInput.PublicKeys;
            int index = signatureInput.KeyPosition;
            byte[] otsk = ConfidentialAssetsHelper.GetOTSK(signatureInput.SourceTransactionKey, _secretViewKey, _secretSpendKey);

            byte[] keyImage = ConfidentialAssetsHelper.GenerateKeyImage(otsk);

            byte[] msg2 = new byte[msg.Length + keyImage.Length];

            Array.Copy(msg.ToArray(), 0, msg2, 0, msg.Length);
            Array.Copy(keyImage, 0, msg2, msg.Length, keyImage.Length);

            RingSignature[] ringSignatures = ConfidentialAssetsHelper.GenerateRingSignature(msg2, keyImage, publicKeys, otsk, index);

            byte[] signature = new byte[64 * ringSignatures.Length];
            for (int i = 0; i < ringSignatures.Length; i++)
            {
                byte[] sig = ringSignatures[i].ToByteArray();
                Array.Copy(sig, 0, signature, 64 * i, 64);
            }

            return signature;
        }

        public void Sign(IPacket packet, object args = null)
        {
            if (!(packet is StealthSignedPacketBase packetBase))
            {
                throw new ArgumentOutOfRangeException(nameof(packet), string.Format(Resources.ERR_WRONG_PACKET_BASE_TYPE, nameof(StealthSigningService), typeof(StealthSignedPacketBase).FullName));
            }

            if (!(args is StealthSignatureInput signatureInput))
            {
                throw new ArgumentNullException(nameof(args), $"{nameof(StealthSigningService)} expects argument args of type {nameof(StealthSignatureInput)}");
            }

            byte[][] publicKeys = signatureInput.PublicKeys;
            int index = signatureInput.KeyPosition;
            byte[] otsk = ConfidentialAssetsHelper.GetOTSK(signatureInput.SourceTransactionKey, _secretViewKey, _secretSpendKey);

            byte[] keyImage = ConfidentialAssetsHelper.GenerateKeyImage(otsk);
            packetBase.KeyImage = _identityKeyProvider.GetKey(keyImage);

            signatureInput.UpdatePacketAction?.Invoke(packetBase);

            byte[] msg = new byte[packet.BodyBytes.Length + keyImage.Length];

            Array.Copy(packet.BodyBytes.ToArray(), 0, msg, 0, packet.BodyBytes.Length);
            Array.Copy(keyImage, 0, msg, packet.BodyBytes.Length, keyImage.Length);

            RingSignature[] ringSignatures = ConfidentialAssetsHelper.GenerateRingSignature(msg, keyImage, publicKeys, otsk, index);

            packetBase.PublicKeys = signatureInput.PublicKeys.Select(p => _identityKeyProvider.GetKey(p)).ToArray();
            packetBase.Signatures = ringSignatures;
        }

        public bool Verify(IPacket packet)
        {
            if (!(packet is StealthSignedPacketBase packetBase))
            {
                throw new ArgumentOutOfRangeException(nameof(packet), string.Format(Resources.ERR_WRONG_PACKET_BASE_TYPE, nameof(StealthSigningService), typeof(StealthSignedPacketBase).FullName));
            }

            byte[] msg = packetBase.BodyBytes.ToArray();
            byte[] keyImage = packetBase.KeyImage.Value.ToArray();
            IKey[] publicKeys = packetBase.PublicKeys;
            RingSignature[] signatures = packetBase.Signatures;

            return ConfidentialAssetsHelper.VerifyRingSignature(msg, keyImage, publicKeys.Select(p => p.Value.ToArray()).ToArray(), signatures);
        }

		public bool CheckTarget(params byte[][] targetValues)
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
                _logger.LogIfDebug(() => $"{nameof(ConfidentialAssetsHelper)}.{nameof(ConfidentialAssetsHelper.IsDestinationKeyMine)}({targetValues[0].ToHexString()}, {targetValues[1].ToHexString()}, {_secretViewKey.ToHexString()}, {PublicKeys[0].Value.ToArray().ToHexString()})");
                bool res = ConfidentialAssetsHelper.IsDestinationKeyMine(targetValues[0], targetValues[1], _secretViewKey, PublicKeys[0].Value.ToArray());
				return res;
			}
			catch (Exception ex)
			{
				_logger.Error($"Failure at {nameof(ConfidentialAssetsHelper)}.{nameof(ConfidentialAssetsHelper.IsDestinationKeyMine)}", ex);
				_logger.Error($"args: {nameof(ConfidentialAssetsHelper)}.{nameof(ConfidentialAssetsHelper.IsDestinationKeyMine)}({targetValues[0].ToHexString()}, {targetValues[1].ToHexString()}, {_secretViewKey.ToHexString()}, {PublicKeys[0].Value.ToArray().ToHexString()})");
				throw;
			}
		}
    }
}

