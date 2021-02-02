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
using O10.Core.Models;
using O10.Core.Tracking;
using O10.Transactions.Core.Serializers;

namespace O10.Network.Handlers
{
    internal class PacketHandlingFlow
    {
        private readonly IEnumerable<ICoreVerifier> _coreVerifiers;
        private readonly IPacketVerifiersRepository _chainTypeValidationHandlersFactory;
        private readonly IBlockParsersRepositoriesRepository _blockParsersFactoriesRepository;
        private readonly IBlocksHandlersRegistry _blocksHandlersRegistry;
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
                                  IBlocksHandlersRegistry blocksProcessorFactory,
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
            _log.Debug(() => $"Posting to processing packet {packet.GetType().Name} [{packet.PacketType}:{packet.BlockType}]");

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
            PacketType packetType = (PacketType)BitConverter.ToUInt16(messagePacket, 0);
            const int blockTypePos = Globals.PACKET_TYPE_LENGTH + Globals.SYNC_BLOCK_HEIGHT_LENGTH + Globals.NONCE_LENGTH + Globals.POW_HASH_SIZE + Globals.VERSION_LENGTH;

            if (messagePacket.Length < blockTypePos + 2)
            {
                _log.Error($"Length of packet is insufficient for obtaining BlockType value: {messagePacket.ToHexString()}");
                return blockBase;
            }

            ushort blockType = BitConverter.ToUInt16(messagePacket, Globals.PACKET_TYPE_LENGTH + Globals.SYNC_BLOCK_HEIGHT_LENGTH + Globals.NONCE_LENGTH + Globals.POW_HASH_SIZE + Globals.VERSION_LENGTH);
            try
            {
                _log.Debug($"Parsing packet of type {packetType} and block type {blockType}");

                IBlockParsersRepository blockParsersFactory = _blockParsersFactoriesRepository.GetBlockParsersRepository(packetType);

                if (blockParsersFactory != null)
                {
                    IBlockParser blockParser = blockParsersFactory.GetInstance(blockType);

                    if (blockParser != null)
                    {
                        blockBase = blockParser.Parse(messagePacket);
                    }
                    else
                    {
                        _log.Error($"Block parser of packet type {packetType} and block type {blockType} not found! Message: {messagePacket.ToHexString()}");
                    }
                }
                else
                {
                    _log.Error($"Block parser factory of packet type {packetType} not found! Message: {messagePacket.ToHexString()}");
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to parse message of packet type {packetType} and block type {blockType}: {messagePacket.ToHexString()}", ex);
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

        private bool ValidateBlock(PacketBase block)
        {
            if (block == null)
            {
                return false;
            }

            _log.Debug(() => $"Validating block {block.GetType().Name}");

            try
            {
				//TODO: !!! need to find proper solution for the problem of checking mandatory conditions
                if (!(block.PacketType == (ushort)PacketType.Registry && block.BlockType == ActionTypes.Registry_RegisterStealth))
                {
                    foreach (ICoreVerifier coreVerifier in _coreVerifiers)
                    {
                        if (!coreVerifier.VerifyBlock(block))
                        {
                            _log.Error($"Verifier {coreVerifier.GetType().Name} found block {block.GetType().Name} is invalid");
                            return false;
                        }
                    }
                }

                IPacketVerifier packetVerifier = _chainTypeValidationHandlersFactory.GetInstance((PacketType)block.PacketType);

                bool res = packetVerifier?.ValidatePacket(block) ?? true;

                return res;
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to validate block {block.RawData.ToHexString()}", ex);
                return false;
            }
        }

        private void DispatchBlock(PacketBase block)
        {
            if (block != null)
            {
                _log.Debug($"Block being dispatched PacketType = {(PacketType)block.PacketType}, BlockType = {block.BlockType}");

				//TODO: !!! need to find proper solution for the problem of checking mandatory conditions
                if (!ValidateBlock(block))
                {
                    return;
                }

                try
                {
                    _log.Debug(() => $"Dispatching block {block.GetType().Name}");

                    foreach (IBlocksHandler blocksHandler in _blocksHandlersRegistry.GetBulkInstances((PacketType)block.PacketType))
					{
						_log.Debug(() => $"Dispatching block {block.GetType().Name} to {blocksHandler.GetType().Name}");
						blocksHandler.ProcessBlock(block);
					}
                }
                catch (Exception ex)
                {
                    _log.Error($"Failed to dispatch block {block.RawData.ToHexString()}", ex);
                }
            }
        }
    }
}
