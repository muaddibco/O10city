using Microsoft.AspNetCore.Mvc;
using O10.Client.Common.Interfaces;
using O10.Client.Web.Portal.Services;
using O10.Core.ExtensionMethods;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using O10.Client.Common.Entities;

namespace O10.Client.Web.Portal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IAssetsService _assetsService;
        private readonly IExecutionContextManager _executionContextManager;

        public TestController(IAssetsService assetsService, IExecutionContextManager executionContextManager)
        {
            _assetsService = assetsService;
            _executionContextManager = executionContextManager;
        }

        [HttpGet("Asset")]
        public IActionResult GetAssetId(long schemeId, string assetValue)
        {
            return Ok(_assetsService.GenerateAssetId(schemeId, assetValue).ToHexString());
        }

        [HttpPost("Asset/{accountId}")]
        public async Task<IActionResult> SendAsset(long accountId, string assetId, string targetPublicSpendKey, string targetPublicViewKey)
        {
            var persistency = _executionContextManager.ResolveExecutionServices(accountId);
            var serviceProvider = persistency.Scope.ServiceProvider;
            var transactionsService = serviceProvider.GetService<IStateTransactionsService>();
            ConfidentialAccount targetAccount = new ConfidentialAccount
            {
                PublicSpendKey = targetPublicSpendKey.HexStringToByteArray(),
                PublicViewKey = targetPublicViewKey.HexStringToByteArray()
            };

            var packet = await transactionsService.TransferAssetToStealth(assetId.HexStringToByteArray(), targetAccount).ConfigureAwait(false);

            return Ok(packet);
        }
    }
}
