using Chaos.NaCl;
using System;
using O10.Core;
using O10.Core.Architecture;

using O10.Core.Cryptography;
using O10.Core.ExtensionMethods;
using O10.Core.HashCalculations;
using O10.Core.Identity;
using O10.Core.Models;
using O10.Crypto.Exceptions;
using O10.Crypto.Properties;

namespace O10.Crypto
{
    [RegisterExtension(typeof(ISigningService), Lifetime = LifetimeManagement.Transient)]
    public class AccountSigningService : ISigningService
    {
        protected byte[] _secretKey;
        protected byte[] _expandedPrivateKey;

        public AccountSigningService(IHashCalculationsRepository hashCalculationRepository, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry)
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

            if(secretKeys.Length != 1)
            {
                throw new WrongSecretKeysNumberException(nameof(AccountSigningService), 1);
            }

			_secretKey = Ed25519.SecretKeyFromSeed(secretKeys[0]);
			Ed25519.KeyPairFromSeed(out byte[] publicKey, out _expandedPrivateKey, secretKeys[0]);

            PublicKeys[0] = IdentityKeyProvider.GetKey(publicKey);
        }

        public void Sign(IPacket packet, object args = null)
        {
            if (!(packet is SignedPacketBase packetBase))
            {
                throw new ArgumentOutOfRangeException(nameof(packet), string.Format(Resources.ERR_WRONG_PACKET_BASE_TYPE, nameof(AccountSigningService), typeof(SignedPacketBase).FullName));
            }

            byte[] signature = Sign(packet.BodyBytes);

            packetBase.Source = PublicKeys[0];
            packetBase.Signature = signature;
        }

        public byte[] Sign(Memory<byte> msg, object args = null)
        {
            byte[] message = msg.ToArray();
            byte[] signature = Ed25519.Sign(message, _expandedPrivateKey);
            return signature;
        }

        public bool Verify(IPacket packet)
        {
            if (!(packet is SignedPacketBase packetBase))
            {
                throw new ArgumentOutOfRangeException(nameof(packet), string.Format(Resources.ERR_WRONG_PACKET_BASE_TYPE, nameof(AccountSigningService), typeof(SignedPacketBase).FullName));
            }

            byte[] signature = packetBase.Signature.ToArray();
            byte[] message = packetBase.BodyBytes.ToArray();
            byte[] publickKey = packetBase.Source.Value.ToArray();

            return Ed25519.Verify(signature, message, publickKey);
        }

        public bool CheckTarget(params byte[][] targetValues)
        {
            if (targetValues == null)
            {
                throw new ArgumentNullException(nameof(targetValues));
            }

            if(targetValues.Length != 1)
            {
                throw new ArgumentOutOfRangeException(nameof(targetValues));
            }

            return targetValues[0].Equals32(PublicKeys[0].Value.ToArray());
        }
    }

}
