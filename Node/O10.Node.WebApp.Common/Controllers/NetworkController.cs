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
using O10.Crypto.Models;
using System.Threading.Tasks;
using System.Threading;

namespace O10.Node.WebApp.Common.Controllers
{
    [Route("api/[controller]")]
	[ApiController]
	public class NetworkController : ControllerBase
	{
		private readonly IPacketsHandler _packetsHandler;
        private readonly IChainDataServicesRepository _chainDataServicesManager;
        private readonly IChainDataService _transactionalDataService;
		private readonly IStealthDataService _stealthDataService;
		private readonly IChainDataService _synchronizationDataService;
		private readonly IIdentityKeyProvider _identityKeyProvider;
		private readonly IHashCalculation _hashCalculation;
		private readonly ILogger _logger;
		private readonly ISynchronizationContext _synchronizationContext;

		public NetworkController(IPacketsHandler packetsHandler,
                           IChainDataServicesRepository chainDataServicesManager,
                           IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
                           IHashCalculationsRepository hashCalculationsRepository,
                           IStatesRepository statesRepository,
						   ILoggerService loggerService)
		{
			_packetsHandler = packetsHandler;
            _chainDataServicesManager = chainDataServicesManager;
            _transactionalDataService = chainDataServicesManager.GetInstance(LedgerType.O10State);
			_stealthDataService = (IStealthDataService)chainDataServicesManager.GetInstance(LedgerType.Stealth);
			_synchronizationDataService = chainDataServicesManager.GetInstance(LedgerType.Synchronization);
			_identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
			_hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
			_synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
			_logger = loggerService.GetLogger(nameof(NetworkController));
		}

		// GET api/values
		[HttpGet]
		public ActionResult<List<InfoMessage>> Get()
		{
			string version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
			return new List<InfoMessage> { new InfoMessage { Context = "Node", InfoType="Version", Message = version } };
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
			return new SyncInfoDTO(
				_synchronizationContext.LastBlockDescriptor?.BlockHeight ?? 0, 
				_synchronizationContext.LastBlockDescriptor?.Hash);
		}

		[HttpGet("LastAggregatedRegistrations")]
		public async Task<ActionResult<AggregatedRegistrationsTransactionDTO>> LastAggregatedRegistrations(CancellationToken cancellationToken)
		{
			var packet = await _synchronizationDataService.Single<SynchronizationPacket>(new SingleByBlockTypeKey(TransactionTypes.Synchronization_RegistryCombinationBlock), cancellationToken);

			return Ok(new AggregatedRegistrationsTransactionDTO { Height = packet?.Payload?.Height ?? 0 });
		}

		[HttpGet("GetLastStatePacketInfo")]
		public async Task<ActionResult<StatePacketInfo>> GetLastStatePacketInfo([FromQuery] string publicKey, CancellationToken cancellationToken)
		{
			byte[] keyBytes = publicKey.HexStringToByteArray();

			if (keyBytes.Length != Globals.NODE_PUBLIC_KEY_SIZE)
			{
				throw new ArgumentException($"Public key size must be of {Globals.NODE_PUBLIC_KEY_SIZE} bytes");
			}

			IKey key = _identityKeyProvider.GetKey(keyBytes);
			O10StatePacket transactionalBlockBase = await _transactionalDataService.Single<O10StatePacket>(new UniqueKey(key), cancellationToken);

			return new StatePacketInfo
			{
				Height = transactionalBlockBase?.Payload.Height ?? 0,
				//TODO: !!! need to reconsider hash calculation here since it is potential point of DoS attack
				Hash = (transactionalBlockBase != null ? _hashCalculation.CalculateHash(transactionalBlockBase.ToByteArray()) : new byte[Globals.DEFAULT_HASH_SIZE]).ToHexString(),
			};
		}

		[HttpGet("Ledger/{ledgerType}/Transaction")]
		public async Task<ActionResult<TransactionBase>> GetTransaction([FromRoute]LedgerType ledgerType, [FromQuery] string combinedBlockHeight, [FromQuery] string hash, CancellationToken cancellationToken)
		{
			_logger.LogIfDebug(() => $"{nameof(GetTransaction)}({ledgerType}, {combinedBlockHeight}, {hash})");

			IKey hashKey = _identityKeyProvider.GetKey(hash.HexStringToByteArray());
			try
			{
				var dataService = _chainDataServicesManager.GetInstance(ledgerType);
				if(dataService == null)
                {
					return BadRequest($"Ledger Type {ledgerType} is not supported");
                }

				var blockBase = (await dataService.Get(string.IsNullOrEmpty(combinedBlockHeight) ? (IDataKey)(new HashKey(hashKey)) : new CombinedHashKey(long.Parse(combinedBlockHeight), hashKey), cancellationToken)).Single();

				if (blockBase == null && !string.IsNullOrEmpty(combinedBlockHeight))
				{
					blockBase = (await dataService.Get(new CombinedHashKey(long.Parse(combinedBlockHeight) - 1, hashKey), cancellationToken)).Single();
				}

				if (blockBase != null)
				{
					return Ok(blockBase.Transaction);
				}
				else
				{
					_logger.Error($"{nameof(GetTransaction)}({ledgerType}, {combinedBlockHeight}, {hash}) didn't find any packet");
				}

				return NotFound();
			}
			catch (Exception ex)
			{
				_logger.Error($"Failed to retrieve block for CombinedBlockHeight {combinedBlockHeight} and ReferencedHash {hash}", ex);
				throw;
			}
		}

		[HttpGet("O10StateTransaction")]
		public async Task<ActionResult<IPacketBase>> GetO10StateTransaction([FromQuery] string combinedBlockHeight, [FromQuery] string hash, CancellationToken cancellationToken)
		{
			_logger.LogIfDebug(() => $"{nameof(GetO10StateTransaction)}({combinedBlockHeight}, {hash})");

			IKey hashKey = _identityKeyProvider.GetKey(hash.HexStringToByteArray());
			try
			{
				var blockBase = (await _transactionalDataService.Get(new CombinedHashKey(long.Parse(combinedBlockHeight), hashKey), cancellationToken)).Single();

				if(blockBase == null)
				{
					blockBase = (await _transactionalDataService.Get(new CombinedHashKey(long.Parse(combinedBlockHeight) - 1, hashKey), cancellationToken)).Single();
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
		public async Task<ActionResult<IPacketBase>> GetStealthTransaction([FromQuery] string combinedBlockHeight, [FromQuery] string hash, CancellationToken cancellationToken)
		{
			byte[] hashBytes = hash.HexStringToByteArray();
			IKey hashKey = _identityKeyProvider.GetKey(hash.HexStringToByteArray());
			try
			{
				IPacketBase blockBase = (await _stealthDataService.Get(new CombinedHashKey(long.Parse(combinedBlockHeight), hashKey), cancellationToken)).Single();

				if(blockBase == null)
				{
					blockBase = (await _stealthDataService.Get(new CombinedHashKey(long.Parse(combinedBlockHeight) - 1, hashKey), cancellationToken)).Single();
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
		public async Task<IActionResult> GetHashByKeyImage(string keyImage, CancellationToken cancellationToken)
		{
			try
			{
				var hash = await _stealthDataService.GetPacketHash(new KeyImageKey(keyImage.HexStringToByteArray()), cancellationToken);

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
