using System;
using System.Collections.Generic;
using O10.Core.Logging;
using O10.Core.Tracking;
using O10.Transactions.Core.Ledgers;
using O10.Network.Interfaces;
using System.Threading.Tasks;

namespace O10.Network.Handlers
{
    internal class PacketHandlingFlow
    {
        private readonly IEnumerable<ICoreVerifier> _coreVerifiers;
        private readonly IPacketVerifiersRepository _chainTypeValidationHandlersFactory;
        private readonly IPacketsHandlersRegistry _packetsHandlersRegistry;
        private readonly ITrackingService _trackingService;
        private readonly ILogger _log;

        public PacketHandlingFlow(int iteration,
                                  ICoreVerifiersBulkFactory coreVerifiersBulkFactory,
                                  IPacketVerifiersRepository packetTypeHandlersFactory,
                                  IPacketsHandlersRegistry packetsHandlersRegistry,
                                  ITrackingService trackingService,
                                  ILoggerService loggerService)
        {
            _coreVerifiers = coreVerifiersBulkFactory.Create();
            _log = loggerService.GetLogger($"{nameof(PacketHandlingFlow)}#{iteration}");
            _chainTypeValidationHandlersFactory = packetTypeHandlersFactory;
            _packetsHandlersRegistry = packetsHandlersRegistry;
            _trackingService = trackingService;
        }

        public async Task PostPacket(IPacketBase packet)
        {
            if (packet != null)
            {
                _log.Debug(() => $"Posting to processing packet {packet.GetType().FullName} [{packet.LedgerType}:{packet.Transaction?.TransactionType}]");

                //TODO: !!! need to find proper solution for the problem of checking mandatory conditions
                if (!ValidateBlock(packet))
                {
                    return;
                }

                try
                {
                    _log.Debug(() => $"Dispatching block {packet.GetType().Name}");

                    foreach (ILedgerPacketsHandler handler in _packetsHandlersRegistry.GetBulkInstances(packet.LedgerType))
                    {
                        _log.Debug(() => $"Dispatching block {packet.GetType().Name} to {handler.GetType().Name}");
                        await handler.ProcessPacket(packet);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error($"Failed to dispatch packet {packet.ToString()}", ex);
                }
            }
        }

        private bool ValidateBlock(IPacketBase packet)
        {
            if (packet == null)
            {
                return false;
            }

            _log.Debug(() => $"Validating block {packet.GetType().Name}");

            try
            {
                foreach (ICoreVerifier coreVerifier in _coreVerifiers)
                {
                    if (!coreVerifier.VerifyBlock(packet))
                    {
                        _log.Error($"Verifier {coreVerifier.GetType().Name} found packet {packet.GetType().Name} is invalid");
                        return false;
                    }
                }

                IPacketVerifier packetVerifier = null; // _chainTypeValidationHandlersFactory.GetInstance(packet.LedgerType);

                bool res = packetVerifier?.ValidatePacket(packet) ?? true;

                return res;
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to validate packet {packet}", ex);
                return false;
            }
        }
    }
}
