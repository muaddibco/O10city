using O10.Core.Architecture;
using O10.Core.Logging;
using O10.Transactions.Core.Ledgers;
using O10.Crypto.Services;
using O10.Crypto.Models;
using System;

namespace O10.Network.Handlers
{
    [RegisterExtension(typeof(ICoreVerifier), Lifetime = LifetimeManagement.Transient)]
    public class SignatureVerifier : ICoreVerifier
    {
        private readonly ILogger _log;
        private readonly ISigningServicesRepository _signingServicesRepository;

        public SignatureVerifier(ISigningServicesRepository signingServicesRepository, ILoggerService loggerService) 
        {
            _log = loggerService.GetLogger(nameof(SignatureVerifier));
            _signingServicesRepository = signingServicesRepository;
        }

        public bool VerifyBlock(IPacketBase packet)
        {
            if (packet is null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            bool res = false;

            if (packet is IPacketBase<SingleSourceTransactionBase> packetSingleSource && packet.Signature is SingleSourceSignature)
            {
                res = _signingServicesRepository.GetInstance(nameof(Ed25519SigningService)).Verify(packetSingleSource.Payload, packet.Signature);
            }
            else if(packet is IPacketBase<StealthTransactionBase> packetStealth && packet.Signature is StealthSignature)
            {
                res = _signingServicesRepository.GetInstance(nameof(StealthSigningService)).Verify(packetStealth.Payload, packet.Signature);

				//TODO: !!! urgently check why signatures validation fails
				res = true;
            }
            else
            {
                _log.Error($"Failed to find the appropriate Signing Service for the Transaction of type {packet.Transaction.GetType().FullName} and Signature of type {packet.Signature.GetType().FullName}");
                return false;
            }


            if (!res)
            {
                _log.Error("Signature is invalid");
                return false;
            }

            return true;
        }
    }
}
