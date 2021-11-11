using System;
using System.Collections.Generic;
using O10.Core.Logging;
using O10.Tracking.Core;
using O10.Transactions.Core.Ledgers;
using O10.Network.Interfaces;
using System.Threading.Tasks;
using O10.Core.Architecture;

namespace O10.Network.Handlers
{
    [RegisterDefaultImplementation(typeof(IPacketHandlingFlow), Lifetime = LifetimeManagement.Scoped)]
    public class PacketHandlingFlow : IPacketHandlingFlow, IDisposable
    {
        private readonly IEnumerable<ICoreVerifier> _coreVerifiers;
        private readonly IPacketVerifiersRepository _chainTypeValidationHandlersFactory;
        private readonly IPacketsHandlersRegistry _packetsHandlersRegistry;
        private readonly ITrackingService _trackingService;
        private readonly IHandlingFlowContext _handlingFlowContext;
        private readonly ILogger _logger;
        private bool _disposedValue;

        public PacketHandlingFlow(ICoreVerifiersBulkFactory coreVerifiersBulkFactory,
                                  IPacketVerifiersRepository packetTypeHandlersFactory,
                                  IPacketsHandlersRegistry packetsHandlersRegistry,
                                  ITrackingService trackingService,
                                  IHandlingFlowContext handlingFlowContext,
                                  ILoggerService loggerService)
        {
            _handlingFlowContext = handlingFlowContext;
            _logger = loggerService.GetLogger($"{nameof(PacketHandlingFlow)}#{handlingFlowContext.Index}");
            _logger.Debug(() => $"Creating {nameof(PacketHandlingFlow)}...");

            _coreVerifiers = coreVerifiersBulkFactory.Create();
            _chainTypeValidationHandlersFactory = packetTypeHandlersFactory;
            _packetsHandlersRegistry = packetsHandlersRegistry;
            _trackingService = trackingService;
        }


        public async Task PostPacket(IPacketBase packet)
        {
            _logger.Debug(() => $"Picked for handling flow #{_handlingFlowContext.Index}");

            if (packet != null)
            {
                _logger.Debug(() => $"Posting for processing packet {packet.GetType().FullName} [{packet.LedgerType}:{packet.Transaction?.TransactionType}]");

                //TODO: !!! need to find proper solution for the problem of checking mandatory conditions
                if (!ValidateBlock(packet))
                {
                    return;
                }

                try
                {
                    _logger.Debug(() => $"Dispatching packet {packet.GetType().Name} [{packet.LedgerType}:{packet.Transaction?.TransactionType}]");

                    foreach (ILedgerPacketsHandler handler in _packetsHandlersRegistry.GetBulkInstances(packet.LedgerType))
                    {
                        _logger.Debug(() => $"Dispatching packet {packet.GetType().Name} [{packet.LedgerType}:{packet.Transaction?.TransactionType}] to {handler.GetType().Name}");
                        await handler.ProcessPacket(packet);
                        _logger.Debug(() => $"Dispatching packet {packet.GetType().Name} [{packet.LedgerType}:{packet.Transaction?.TransactionType}] to {handler.GetType().Name} completed");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to dispatch packet {packet.ToString()}", ex);
                }
            }
            else
            {
                _logger.Error("Packet is NULL!!!");
            }
        }

        private bool ValidateBlock(IPacketBase packet)
        {
            if (packet == null)
            {
                return false;
            }

            _logger.Debug(() => $"Validating packet {packet.GetType().Name} [{packet.LedgerType}:{packet.Transaction?.TransactionType}]");

            try
            {
                foreach (ICoreVerifier coreVerifier in _coreVerifiers)
                {
                    if (!coreVerifier.VerifyBlock(packet))
                    {
                        _logger.Error($"Verifier {coreVerifier.GetType().Name} found packet {packet.GetType().Name} is invalid");
                        return false;
                    }
                }

                IPacketVerifier packetVerifier = null; // _chainTypeValidationHandlersFactory.GetInstance(packet.LedgerType);

                bool res = packetVerifier?.ValidatePacket(packet) ?? true;

                return res;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to validate packet {packet}", ex);
                return false;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _logger.Debug(() => $"Stopping {nameof(PacketHandlingFlow)}...");
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~PacketHandlingFlow()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
