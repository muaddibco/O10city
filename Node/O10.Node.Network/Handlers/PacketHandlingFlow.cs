using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Interfaces;
using O10.Transactions.Core.Parsers;
using O10.Core;
using O10.Core.ExtensionMethods;
using O10.Core.Logging;
using O10.Core.Tracking;
using O10.Transactions.Core.Serializers;
using O10.Transactions.Core.Ledgers;

namespace O10.Network.Handlers
{
    internal class PacketHandlingFlow
    {
        private readonly IEnumerable<ICoreVerifier> _coreVerifiers;
        private readonly IPacketVerifiersRepository _chainTypeValidationHandlersFactory;
        private readonly IBlockParsersRepositoriesRepository _blockParsersFactoriesRepository;
        private readonly IPacketsHandlersRegistry _blocksHandlersRegistry;
        private readonly ISerializersFactory _serializersFactory;
        private readonly ITrackingService _trackingService;
        private readonly ILogger _log;

        private readonly TransformBlock<byte[], byte[]> _decodeBlock;
        private readonly TransformBlock<byte[], PacketBase> _parseBlock;
        private readonly ActionBlock<PacketBase> _processBlock;

        public PacketHandlingFlow(int iteration,
                                  ICoreVerifiersBulkFactory coreVerifiersBulkFactory,
                                  IPacketVerifiersRepository packetTypeHandlersFactory,
                                  IBlockParsersRepositoriesRepository blockParsersFactoriesRepository,
                                  IPacketsHandlersRegistry blocksProcessorFactory,
                                  ISerializersFactory serializersFactory,
                                  ITrackingService trackingService,
                                  ILoggerService loggerService)
        {
            _coreVerifiers = coreVerifiersBulkFactory.Create();
            _log = loggerService.GetLogger($"{nameof(PacketHandlingFlow)}#{iteration}");
            _blockParsersFactoriesRepository = blockParsersFactoriesRepository;
            _chainTypeValidationHandlersFactory = packetTypeHandlersFactory;
            _blocksHandlersRegistry = blocksProcessorFactory;
            _serializersFactory = serializersFactory;
            _trackingService = trackingService;
            _decodeBlock = new TransformBlock<byte[], byte[]>(DecodeMessage, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1, BoundedCapacity = 1000000});
            _parseBlock = new TransformBlock<byte[], PacketBase>(ParseMessagePacket, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1, BoundedCapacity = 1000000 });
            _processBlock = new ActionBlock<PacketBase>(DispatchBlock, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1, BoundedCapacity = 1000000 });

            _decodeBlock.LinkTo(_parseBlock);
            _parseBlock.LinkTo(_processBlock);
        }

        public void PostPacket(PacketBase packet)
        {
            _log.Debug(() => $"Posting to processing packet {packet.GetType().Name} [{packet.LedgerType}:{packet.PacketType}]");

            if(packet.RawData.Length == 0)
            {
                using var serializer = _serializersFactory.Create(packet);
                serializer.SerializeFully();
            }

            _processBlock.Post(packet);
        }

        public void PostMessage(byte[] messagePacket)
        {
            _log.Debug(() => $"Posting to decoding packet {messagePacket.ToHexString()}");

            _decodeBlock.Post(messagePacket);
        }

        private byte[] DecodeMessage(byte[] messagePacket)
        {
            _log.LogIfDebug(() => $"Packet being decoded {messagePacket.ToHexString()}");

            using MemoryStream memoryStream = new MemoryStream();
            bool dleDetected = false;

            foreach (byte b in messagePacket)
            {
                if (b != Globals.DLE)
                {
                    if (dleDetected)
                    {
                        dleDetected = false;
                        memoryStream.WriteByte((byte)(b - Globals.DLE));
                    }
                    else
                    {
                        memoryStream.WriteByte(b);
                    }
                }
                else
                {
                    dleDetected = true;
                }
            }

            _trackingService.TrackMetric("DecodingThroughput", 1);

            byte[] decodedPacket = memoryStream.ToArray();

            _log.Debug(() => $"Decoded packet {decodedPacket.ToHexString()}");

            return decodedPacket;
        }

        private PacketBase ParseMessagePacket(byte[] messagePacket)
        {
            _log.Debug(() => $"Packet being parsed {messagePacket.ToHexString()}");

            PacketBase blockBase = null;
            LedgerType ledgerType = (LedgerType)BitConverter.ToUInt16(messagePacket, 0);
            const int blockTypePos = Globals.PACKET_TYPE_LENGTH + Globals.SYNC_BLOCK_HEIGHT_LENGTH + Globals.NONCE_LENGTH + Globals.POW_HASH_SIZE + Globals.VERSION_LENGTH;

            if (messagePacket.Length < blockTypePos + 2)
            {
                _log.Error($"Length of packet is insufficient for obtaining BlockType value: {messagePacket.ToHexString()}");
                return blockBase;
            }

            ushort blockType = BitConverter.ToUInt16(messagePacket, Globals.PACKET_TYPE_LENGTH + Globals.SYNC_BLOCK_HEIGHT_LENGTH + Globals.NONCE_LENGTH + Globals.POW_HASH_SIZE + Globals.VERSION_LENGTH);
            try
            {
                _log.Debug($"Parsing packet of type {ledgerType} and block type {blockType}");

                IBlockParsersRepository blockParsersFactory = _blockParsersFactoriesRepository.GetBlockParsersRepository(ledgerType);

                if (blockParsersFactory != null)
                {
                    IBlockParser blockParser = blockParsersFactory.GetInstance(blockType);

                    if (blockParser != null)
                    {
                        blockBase = blockParser.Parse(messagePacket);
                    }
                    else
                    {
                        _log.Error($"Block parser of packet type {ledgerType} and block type {blockType} not found! Message: {messagePacket.ToHexString()}");
                    }
                }
                else
                {
                    _log.Error($"Block parser factory of packet type {ledgerType} not found! Message: {messagePacket.ToHexString()}");
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to parse message of packet type {ledgerType} and block type {blockType}: {messagePacket.ToHexString()}", ex);
            }

            _trackingService.TrackMetric("ParsingThroughput", 1);

            if (blockBase != null)
            {
                _log.Debug(() => $"Parsed block {blockBase.RawData.ToHexString()}");
            }
            else
            {
                _log.Error($"Failed to parse block from message {messagePacket.ToHexString()}");
            }

            return blockBase;
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
                    _log.Error($"Failed to dispatch block {packet.RawData.ToHexString()}", ex);
                }
            }
        }
    }
}
