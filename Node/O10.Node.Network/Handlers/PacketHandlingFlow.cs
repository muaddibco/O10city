using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Interfaces;
using O10.Core.Logging;
using O10.Core.Tracking;
using O10.Transactions.Core.Ledgers;

namespace O10.Network.Handlers
{
    internal class PacketHandlingFlow
    {
        private readonly IEnumerable<ICoreVerifier> _coreVerifiers;
        private readonly IPacketVerifiersRepository _chainTypeValidationHandlersFactory;
        private readonly IPacketsHandlersRegistry _blocksHandlersRegistry;
        private readonly ITrackingService _trackingService;
        private readonly ILogger _log;

        private readonly ActionBlock<IPacketBase> _processBlock;

        public PacketHandlingFlow(int iteration,
                                  ICoreVerifiersBulkFactory coreVerifiersBulkFactory,
                                  IPacketVerifiersRepository packetTypeHandlersFactory,
                                  IPacketsHandlersRegistry blocksProcessorFactory,
                                  ITrackingService trackingService,
                                  ILoggerService loggerService)
        {
            _coreVerifiers = coreVerifiersBulkFactory.Create();
            _log = loggerService.GetLogger($"{nameof(PacketHandlingFlow)}#{iteration}");
            _chainTypeValidationHandlersFactory = packetTypeHandlersFactory;
            _blocksHandlersRegistry = blocksProcessorFactory;
            _trackingService = trackingService;
            _processBlock = new ActionBlock<IPacketBase>(DispatchBlock, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1, BoundedCapacity = 1000000 });
        }

        public void PostPacket(IPacketBase packet)
        {
            _log.Debug(() => $"Posting to processing packet {packet.GetType().Name} [{packet.LedgerType}:{packet.Body?.TransactionType}]");

            _processBlock.Post(packet);
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

                IPacketVerifier packetVerifier = _chainTypeValidationHandlersFactory.GetInstance(packet.LedgerType);

                bool res = packetVerifier?.ValidatePacket(packet) ?? true;

                return res;
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to validate packet {packet}", ex);
                return false;
            }
        }

        private void DispatchBlock(IPacketBase packet)
        {
            if (packet != null)
            {
                _log.Debug($"Packet being dispatched is {packet.GetType().FullName}");

				//TODO: !!! need to find proper solution for the problem of checking mandatory conditions
                if (!ValidateBlock(packet))
                {
                    return;
                }

                try
                {
                    _log.Debug(() => $"Dispatching block {packet.GetType().Name}");

                    foreach (Transactions.Core.Interfaces.IPacketsHandler blocksHandler in _blocksHandlersRegistry.GetBulkInstances((LedgerType)packet.LedgerType))
					{
                        _log.Debug(() => $"Dispatching block {packet.GetType().Name} to {blocksHandler.GetType().Name}");
						blocksHandler.ProcessBlock(packet);
					}
                }
                catch (Exception ex)
                {
                    _log.Error($"Failed to dispatch packet {packet.ToString()}", ex);
                }
            }
        }
    }
}
