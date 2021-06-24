using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using O10.Gateway.DataLayer.Services;
using O10.Core.ExtensionMethods;
using O10.Gateway.Common.Services;
using O10.Gateway.WebApp.Common.Models;
using O10.Gateway.WebApp.Common.Services;
using O10.Core.Configuration;
using Flurl;
using Flurl.Http;
using System.Threading.Tasks;
using O10.Gateway.Common.Configuration;
using System.Reflection;
using O10.Core.Logging;
using System;
using O10.Gateway.WebApp.Common.Exceptions;
using O10.Core.Models;
using O10.Gateway.Common.Services.Results;
using O10.Core.Notifications;
using O10.Transactions.Core.DTOs;
using O10.Transactions.Core.Ledgers;
using O10.Core.Identity;

namespace O10.Gateway.WebApp.Common.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SynchronizationController : ControllerBase
    {
        private readonly INetworkSynchronizer _networkSynchronizer;
        private readonly IDataAccessService _dataAccessService;
        private readonly ITransactionsHandler _transactionsHandler;
        private readonly IRelationProofSessionsStorage _relationProofSessionsStorage;
        private readonly ISynchronizerConfiguration _synchronizerConfiguration;
        private readonly ILogger _logger;
        private readonly IIdentityKeyProvider _identityKeyProvider;

        public SynchronizationController(INetworkSynchronizer networkSynchronizer,
                                         IDataAccessService dataAccessService,
                                         IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
                                         ITransactionsHandler transactionsHandler,
                                         IRelationProofSessionsStorage relationProofSessionsStorage,
                                         IConfigurationService configurationService,
                                         ILoggerService loggerService)
        {
            _networkSynchronizer = networkSynchronizer;
            _dataAccessService = dataAccessService;
            _transactionsHandler = transactionsHandler;
            _relationProofSessionsStorage = relationProofSessionsStorage;
            _synchronizerConfiguration = configurationService.Get<ISynchronizerConfiguration>();
            _logger = loggerService.GetLogger(nameof(SynchronizationController));
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
        }

        [HttpGet("EnvironmentVariables")]
        public IActionResult GetEnvironmentVariables()
        {
            return Ok(Environment.GetEnvironmentVariables());
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InfoMessage>>> Get()
        {
            string version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
            _logger.Info(version);

            IEnumerable<InfoMessage> nodeInfo;
            try
            {
                nodeInfo = await _networkSynchronizer.GetConnectedNodesInfo().ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                nodeInfo = new List<InfoMessage> { new InfoMessage { Context = "Gateway", InfoType = "Error", Message = $"Failed to connect due to the error '{ex.Message}'" } };
            }

            InfoMessage msgUpdaterConnectivity = new InfoMessage
            {
                Context = "Gateway",
                InfoType = "UpdaterConnectivity"
            };

            Random r = new Random();
            int nonce = r.Next();

            var awaiter = _networkSynchronizer.GetConnectivityCheckAwaiter(nonce);
            await _synchronizerConfiguration.NodeServiceApiUri.AppendPathSegment("ConnectivityCheck").PostJsonAsync(new InfoMessage
            {
                Context = "Gateway",
                InfoType = "UpdaterConnectivity",
                Message = nonce.ToString()
            }).ConfigureAwait(false);

            try
            {
                await awaiter.Task.TimeoutAfter(3000).ConfigureAwait(false);
                msgUpdaterConnectivity.Message = "Succeeded";
            }
            catch (TimeoutException)
            {
                msgUpdaterConnectivity.Message = "Failed";
            }


            List<InfoMessage> gatewayInfo = new List<InfoMessage> { new InfoMessage { Context = "Gateway", InfoType = "Version", Message = version }, msgUpdaterConnectivity };

            return Ok(gatewayInfo.Concat(nodeInfo));
        }

        [HttpGet("GetLastRegistryCombinedBlock")]
        public async Task<IActionResult> GetLastRegistryCombinedBlock()
        {
            return Ok(await _networkSynchronizer.GetLastRegistryCombinedBlock().ConfigureAwait(false));
        }

        [HttpGet("GetLastSyncBlock")]
        public async Task<IActionResult> GetLastSyncBlock()
        {
            return Ok(await _networkSynchronizer.GetLastSyncBlock().ConfigureAwait(false));
        }

        [HttpGet("GetLastSyncBlockOnNode")]
        public async Task<IActionResult> GetLastSyncBlockOnNode()
        {
            return Ok(await _synchronizerConfiguration.NodeApiUri.AppendPathSegment("GetLastSyncBlock").GetJsonAsync<SyncInfoDTO>().ConfigureAwait(false));
        }

        [HttpGet("GetWitnessesRange/{combinedBlockHeightStart}/{combinedBlockHeightEnd}")]
        public ActionResult<List<WitnessPackage>> GetWitnessesRange(long combinedBlockHeightStart, long combinedBlockHeightEnd = 0)
        {
            return Ok(_networkSynchronizer.GetWitnessRange(combinedBlockHeightStart, combinedBlockHeightEnd).ToList());
        }

        [HttpGet("GetLastPacketInfo/{accountPublicKey}")]
        public async Task<IActionResult> GetLastPacketInfo(string accountPublicKey)
        {
            StatePacketInfo statePacketInfo = await _networkSynchronizer.GetLastPacketInfo(accountPublicKey).ConfigureAwait(false);
            return Ok(statePacketInfo);
        }

        [HttpGet("GetCombinedBlockByTransactionHash/{accountPublicKey}/{transactionHash}")]
        public IActionResult GetCombinedBlockByAccountHeight(string accountPublicKey, string transactionHash)
        {
            var transactionalIncomingBlock = _dataAccessService.GetStateTransaction(accountPublicKey, transactionHash);

            return Ok(transactionalIncomingBlock?.Hash?.AggregatedTransactionsHeight ?? 0);
        }

        [HttpGet("GetIssuanceCommitments/{issuer}/{amount}")]
        public IActionResult GetIssuanceCommitments(string issuer, int amount)
        {
            byte[][] commitments = _dataAccessService.GetRootAttributeCommitments(issuer.HexStringToByteArray(), amount);

            return Ok(commitments);
        }

        [HttpGet("GetOutputs/{amount}")]
        public IActionResult GetOutputs(int amount)
        {
            OutputSources[] outputModels = _dataAccessService.GetOutputs(amount).Select(o => new OutputSources { DestinationKey = _identityKeyProvider.GetKey(o.DestinationKey.HexStringToByteArray()) }).ToArray();

            return Ok(outputModels);
        }

        /// <summary>
        /// This endpoint is used for sending Stealth and Transactional packets only
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        [HttpPost("SendPacket")]
        public async Task<IActionResult> SendPacket([FromBody] IPacketBase packet)
        {
            SendDataResponse response = new SendDataResponse();

            var completion = _transactionsHandler.SendPacket(packet);
            
            var res = await completion.Task.ConfigureAwait(false);

            response.Status = res is SucceededNotification;
            if (res is KeyImageViolatedNotification result)
            {
                response.ExistingHash = result.ExistingHash;
            }

            return Ok(response);
        }

        [HttpGet("Packets")]
        public async Task<IActionResult> GetPackets([FromQuery(Name = "wid")] List<long> witnessIds)
        {
            return Ok(await _networkSynchronizer.GetTransactions(witnessIds));
        }

        [HttpGet("IsRootAttributeValid/{issuer}/{commitment}")]
        public IActionResult IsRootAttributeValid(string issuer, string commitment)
        {
            _logger.LogIfDebug(() => $"{nameof(IsRootAttributeValid)}({issuer}, {commitment})");
            bool res = _dataAccessService.CheckRootAttributeExist(issuer.HexStringToByteArray(), commitment.HexStringToByteArray());
            _logger.LogIfDebug(() => $"{nameof(IsRootAttributeValid)}({issuer}, {commitment}): {res}");

            if(!res)
            {
                throw new NotValidRootAttributeException(commitment, issuer);
            }

            return Ok();
        }

        [HttpPost("AreRootAttributesValid/{issuer}")]
        public IActionResult AreRootAttributesValid(string issuer, [FromBody] List<string> rootAttributes)
        {
            foreach (var commitment in rootAttributes)
            {
                _logger.LogIfDebug(() => $"{nameof(AreRootAttributesValid)}({issuer}, {commitment})");
                bool res = _dataAccessService.CheckRootAttributeExist(issuer.HexStringToByteArray(), commitment.HexStringToByteArray());
                _logger.LogIfDebug(() => $"{nameof(AreRootAttributesValid)}({issuer}, {commitment}): {res}");

                if(!res)
                {
                    throw new NotValidRootAttributeException(commitment, issuer);
                }
            }

            return Ok();
        }

        [HttpPost("AreAssociatedAttributesExist/{issuer}")]
        public IActionResult AreAssociatedAttributesExist(string issuer, [FromBody] Dictionary<string, string> rootAttributes)
        {
            byte[] issuerBytes = issuer.HexStringToByteArray();
            foreach (var issuanceCommitment in rootAttributes.Keys)
            {
                string commitmentToRoot = rootAttributes[issuanceCommitment];

                _logger.LogIfDebug(() => $"{nameof(AreAssociatedAttributesExist)}({issuer}, {issuanceCommitment}, {commitmentToRoot})");
                bool res = _dataAccessService.CheckAssociatedAtributeExist(issuerBytes, issuanceCommitment.HexStringToByteArray(), commitmentToRoot.HexStringToByteArray());
                _logger.LogIfDebug(() => $"{nameof(AreAssociatedAttributesExist)}({issuer}, {issuanceCommitment}, {commitmentToRoot}): {res}");

                if (!res)
                {
                    throw new NotValidAssociatedAttributeException(issuanceCommitment, commitmentToRoot, issuer);
                }
            }

            return Ok();
        }

        [HttpGet("WasRootAttributeValid/{issuer}/{commitment}/{combinedBlockHeight}")]
        public IActionResult WasRootAttributeValid(string issuer, string commitment, long combinedBlockHeight)
        {
            return Ok(_dataAccessService.CheckRootAttributeWasValid(issuer.HexStringToByteArray(), commitment.HexStringToByteArray(), combinedBlockHeight));
        }

        [HttpGet("GetEmployeeRecordGroup/{issuer}/{registrationCommitment}")]
        public IActionResult GetEmployeeRecordGroup(string issuer, string registrationCommitment)
        {
            return Ok(_dataAccessService.GetRelationRecordGroup(issuer, registrationCommitment).ToHexString());
        }

        [HttpGet("Transaction")]
        public IActionResult GetTransactionBySourceAndHeight([FromQuery] string source, [FromQuery] string transactionHash)
        {
            var transaction = _dataAccessService.GetStateTransaction(source, transactionHash);

            return Ok(transaction);
        }

        [HttpPost("PushRelationProofSession")]
        public IActionResult PushRelationProofSession([FromBody] RelationProofSession relationProofSession)
        {
            return Ok(new { sessionKey = _relationProofSessionsStorage.Push(relationProofSession) });
        }

        [HttpGet("PopRelationProofSession/{sessionKey}")]
        public IActionResult PopRelationProofSession(string sessionKey)
        {
            var relationProofSession = _relationProofSessionsStorage.Pop(sessionKey);

            if (relationProofSession != null)
            {
                return Ok(relationProofSession);
            }

            return BadRequest();
        }

        [HttpGet("HashByKeyImage/{keyImage}")]
        public async Task<IActionResult> GetHashByKeyImage(string keyImage)
        {
            var response = await _synchronizerConfiguration.NodeApiUri.AppendPathSegments("HashByKeyImage", keyImage).GetJsonAsync<PacketHashResponse>().ConfigureAwait(false);

            return Ok(response);
        }

        [HttpGet("IsKeyImageCompomised")]
        public ActionResult<bool> GetIsKeyImageCompomised(string keyImage)
        {
            return Ok(_dataAccessService.GetIsKeyImageCompomised(_identityKeyProvider.GetKey(keyImage.HexStringToByteArray())));
        }
    }
}
