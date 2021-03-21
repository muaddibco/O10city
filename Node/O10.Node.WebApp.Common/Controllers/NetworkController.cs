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
using O10.Transactions.Core.Ledgers;
using O10.Transactions.Core.Ledgers.Synchronization.Transactions;

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
		public IActionResult Post([FromBody] IPacketBase packet)
		{
			_packetsHandler.Push(packet);

			return Ok();
		}

		[HttpGet("GetLastSyncBlock")]
		public ActionResult<SyncInfoDTO> GetLastSyncBlock()
		{
			return new SyncInfoDTO(_synchronizationContext.LastBlockDescriptor?.BlockHeight ?? 0, _synchronizationContext.LastBlockDescriptor?.Hash ?? new byte[Globals.DEFAULT_HASH_SIZE]);
		}

		[HttpGet("LastAggregatedRegistrations")]
		public ActionResult<AggregatedRegistrationsTransactionDTO> LastAggregatedRegistrations()
		{
			AggregatedRegistrationsTransaction combinedBlock = _synchronizationDataService.Single<SynchronizationPacket>(new SingleByBlockTypeKey(TransactionTypes.Synchronization_RegistryCombinationBlock))?.As<AggregatedRegistrationsTransaction>();

			return Ok(new AggregatedRegistrationsTransactionDTO { Height = combinedBlock.Height });
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
			O10StatePacket transactionalBlockBase = _transactionalDataService.Single<O10StatePacket>(new UniqueKey(key));

			return new StatePacketInfo
			{
				Height = transactionalBlockBase.Body?.Height ?? 0,
				//TODO: !!! need to reconsider hash calculation here since it is potential point of DoS attack
				Hash = (transactionalBlockBase != null ? _hashCalculation.CalculateHash(transactionalBlockBase.ToByteArray()) : new byte[Globals.DEFAULT_HASH_SIZE]).ToHexString(),
			};
		}

		[HttpGet("O10StateTransaction")]
		public ActionResult<IPacketBase> GetO10StateTransaction([FromQuery] string combinedBlockHeight, [FromQuery] string hash)
		{
			_logger.LogIfDebug(() => $"{nameof(GetO10StateTransaction)}({combinedBlockHeight}, {hash})");

			IKey hashKey = _identityKeyProvider.GetKey(hash.HexStringToByteArray());
			try
			{
				var blockBase = _transactionalDataService.Get(new CombinedHashKey(long.Parse(combinedBlockHeight), hashKey)).Single();

				if(blockBase == null)
				{
					blockBase = _transactionalDataService.Get(new CombinedHashKey(long.Parse(combinedBlockHeight) - 1, hashKey)).Single();
				}
			
				if(blockBase != null)
                {
					return Ok(blockBase);
				}
				else
				{
					_logger.Error($"{nameof(GetO10StateTransaction)}({combinedBlockHeight}, {hash}) not found");
				}

				return NotFound();
			}
			catch (Exception ex)
			{
				_logger.Error($"Failed to retrieve block for CombinedBlockHeight {combinedBlockHeight} and ReferencedHash {hash}", ex);
				throw;
			}
		}

		[HttpGet("StealthTransaction")]
		public ActionResult<IPacketBase> GetStealthTransaction([FromQuery] string combinedBlockHeight, [FromQuery] string hash)
		{
			byte[] hashBytes = hash.HexStringToByteArray();
			IKey hashKey = _identityKeyProvider.GetKey(hash.HexStringToByteArray());
			try
			{
				IPacketBase blockBase = _stealthDataService.Get(new CombinedHashKey(long.Parse(combinedBlockHeight), hashKey)).Single();

				if(blockBase == null)
				{
					blockBase = _stealthDataService.Get(new CombinedHashKey(long.Parse(combinedBlockHeight) - 1, hashKey)).Single();
				}

				if (blockBase != null)
				{
					return Ok(blockBase);
				}
				else
				{
					_logger.Error($"{nameof(GetStealthTransaction)}({combinedBlockHeight}, {hash}) not found");
				}

				return NotFound();
			}
			catch (Exception ex)
			{
				_logger.Error($"Failed to retrieve block for CombinedBlockHeight {combinedBlockHeight} and ReferencedHash {hash}", ex);
				throw;
			}
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
