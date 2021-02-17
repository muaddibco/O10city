using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using O10.Transactions.Core.Ledgers.Synchronization;
using O10.Transactions.Core.Ledgers.O10State;
using O10.Transactions.Core.Enums;
using O10.Node.DataLayer.DataServices;
using O10.Node.DataLayer.DataServices.Keys;
using O10.Core;
using O10.Core.ExtensionMethods;
using O10.Core.HashCalculations;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Core.Models;
using O10.Core.States;
using O10.Core.Synchronization;
using O10.Network.Handlers;
using O10.Transactions.Core.DTOs;

namespace O10.Node.WebApp.Common.Controllers
{
    [Route("api/[controller]")]
	[ApiController]
	public class NetworkController : ControllerBase
	{
		private readonly IPacketsHandler _packetsHandler;
        private readonly IChainDataService _transactionalDataService;
		private readonly IStealthDataService _stealthDataService;
		private readonly IChainDataService _synchronizationDataService;
		private readonly IIdentityKeyProvider _identityKeyProvider;
		private readonly IHashCalculation _hashCalculation;
		private readonly ILogger _logger;
		private readonly ISynchronizationContext _synchronizationContext;

		public NetworkController(IPacketsHandler packetsHandler,
                           IChainDataServicesManager chainDataServicesManager,
                           IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
                           IHashCalculationsRepository hashCalculationsRepository,
                           IStatesRepository statesRepository,
						   ILoggerService loggerService)
		{
			_packetsHandler = packetsHandler;
            _transactionalDataService = chainDataServicesManager.GetChainDataService(LedgerType.O10State);
			_stealthDataService = (IStealthDataService)chainDataServicesManager.GetChainDataService(LedgerType.Stealth);
			_synchronizationDataService = chainDataServicesManager.GetChainDataService(LedgerType.Synchronization);
			_identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
			_hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
			_synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
			_logger = loggerService.GetLogger(nameof(NetworkController));
		}

		// GET api/values
		[HttpGet]
		public ActionResult<IEnumerable<InfoMessage>> Get()
		{
			string version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
			return new List<InfoMessage> { new InfoMessage { Context = "Node", InfoType="Version", Message = version } };
		}

		// POST api/values
		[HttpPost]
		public async void Post()
		{
			string packet = await Request.GetRawBodyStringAsync().ConfigureAwait(false);
			byte[] packetBytes = packet.HexStringToByteArray();

			_packetsHandler.Push(packetBytes);
		}

		[HttpPost("Packet")]
		public IActionResult Post([FromBody] PacketBase packet)
		{
			_packetsHandler.Push(packet);

			return Ok();
		}

		[HttpGet("GetLastSyncBlock")]
		public ActionResult<SyncBlockModel> GetLastSyncBlock()
		{
			return new SyncBlockModel(_synchronizationContext.LastBlockDescriptor?.BlockHeight ?? 0, _synchronizationContext.LastBlockDescriptor?.Hash ?? new byte[Globals.DEFAULT_HASH_SIZE]);
		}

		[HttpGet("LastRegistryCombinedBlock")]
		public ActionResult<RegistryCombinedBlockModel> GetLastRegistryCombinedBlock()
		{
			SynchronizationRegistryCombinedBlock combinedBlock = _synchronizationDataService.Single<SynchronizationRegistryCombinedBlock>(new SingleByBlockTypeKey(PacketTypes.Synchronization_RegistryCombinationBlock));

			byte[] content = combinedBlock?.RawData.ToArray() ?? new byte[] { };
			return Ok(new RegistryCombinedBlockModel(combinedBlock?.Height??0, content, _hashCalculation.CalculateHash(content)));
		}

		[HttpGet("GetLastStatePacketInfo")]
		public ActionResult<StatePacketInfo> GetLastStatePacketInfo([FromQuery] string publicKey)
		{
			byte[] keyBytes = publicKey.HexStringToByteArray();

			if (keyBytes.Length != Globals.NODE_PUBLIC_KEY_SIZE)
			{
				throw new ArgumentException($"Public key size must be of {Globals.NODE_PUBLIC_KEY_SIZE} bytes");
			}

			IKey key = _identityKeyProvider.GetKey(keyBytes);
			TransactionalPacketBase transactionalBlockBase = _transactionalDataService.Single<TransactionalPacketBase>(new UniqueKey(key));

			return new StatePacketInfo
			{
				Height = transactionalBlockBase?.Height ?? 0,
				//TODO: !!! need to reconsider hash calculation here since it is potential point of DoS attack
				Hash = (transactionalBlockBase != null ? _hashCalculation.CalculateHash(transactionalBlockBase.RawData) : new byte[Globals.DEFAULT_HASH_SIZE]).ToHexString(),
			};
		}

		[HttpGet("GetTransactionInfoState")]
		public ActionResult<TransactionInfo> GetTransactionInfoState([FromQuery] string combinedBlockHeight, [FromQuery] string hash)
		{
			_logger.LogIfDebug(() => $"{nameof(GetTransactionInfoState)}({combinedBlockHeight}, {hash})");

			byte[] hashBytes = hash.HexStringToByteArray();
			try
			{
				PacketBase blockBase = _transactionalDataService.Get(new CombinedHashKey(ulong.Parse(combinedBlockHeight), hashBytes)).Single();

				if (blockBase != null)
				{
					return new TransactionInfo
					{
						SyncBlockHeight = blockBase.SyncHeight,
						PacketType = (LedgerType)blockBase.LedgerType,
						BlockType = blockBase.PacketType,
						Content = blockBase.RawData.ToArray()
					};
				}
				else
				{
					blockBase = _transactionalDataService.Get(new CombinedHashKey(ulong.Parse(combinedBlockHeight) - 1, hashBytes)).Single();

					if (blockBase != null)
					{
						return new TransactionInfo
						{
							SyncBlockHeight = blockBase.SyncHeight,
							PacketType = (LedgerType)blockBase.LedgerType,
							BlockType = blockBase.PacketType,
							Content = blockBase.RawData.ToArray()
						};
					}
				}
			}
			catch (Exception ex)
			{
				_logger.Error($"Failed to retrieve block for CombinedBlockHeight {combinedBlockHeight} and ReferencedHash {hash}", ex);
			}

			_logger.Error($"{nameof(GetTransactionInfoState)}({combinedBlockHeight}, {hash}) not found");

			return new TransactionInfo();
		}

		[HttpGet("StealthTransactionInfo")]
		public ActionResult<TransactionInfo> GetStealthTransactionInfo([FromQuery] string combinedBlockHeight, [FromQuery] string hash)
		{
			byte[] hashBytes = hash.HexStringToByteArray();
			try
			{
				PacketBase blockBase = _stealthDataService.Get(new CombinedHashKey(ulong.Parse(combinedBlockHeight), hashBytes)).Single();

				if (blockBase != null)
				{
					return new TransactionInfo
					{
						SyncBlockHeight = blockBase.SyncHeight,
						PacketType = (LedgerType)blockBase.LedgerType,
						BlockType = blockBase.PacketType,
						Content = blockBase.RawData.ToArray()
					};
				}
				else
				{
					blockBase = _stealthDataService.Get(new CombinedHashKey(ulong.Parse(combinedBlockHeight) - 1, hashBytes)).Single();

					if (blockBase != null)
					{
						return new TransactionInfo
						{
							SyncBlockHeight = blockBase.SyncHeight,
							PacketType = (LedgerType)blockBase.LedgerType,
							BlockType = blockBase.PacketType,
							Content = blockBase.RawData.ToArray()
						};
					}
				}
			}
			catch (Exception ex)
			{
				_logger.Error($"Failed to retrieve block for CombinedBlockHeight {combinedBlockHeight} and ReferencedHash {hash}", ex);
			}

			return new TransactionInfo();
		}

		[HttpGet("HashByKeyImage/{keyImage}")]
		public IActionResult GetHashByKeyImage(string keyImage)
		{
			try
			{
				string hash = _stealthDataService.GetPacketHash(new KeyImageKey(keyImage));

				var response = new PacketHashResponse { Hash = hash };

				return Ok(response);
			}
			catch (Exception ex)
			{
				_logger.Error($"Failed to obtain hash by KeyImage {keyImage}", ex);
				throw;
			}
		}
	}
}
