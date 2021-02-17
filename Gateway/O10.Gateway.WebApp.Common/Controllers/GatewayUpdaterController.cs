using Microsoft.AspNetCore.Mvc;
using O10.Core.Logging;
using O10.Gateway.Common.Services;
using System;
using O10.Core.ExtensionMethods;
using System.Threading.Tasks;
using O10.Core.Models;
using O10.Transactions.Core.DTOs;

namespace O10.Gateway.WebApp.Common.Controllers
{
    [Route("api/[controller]")]
	[ApiController]
	public class GatewayUpdaterController : ControllerBase
	{
		private readonly ILogger _logger;
		private readonly INetworkSynchronizer _networkSynchronizer;

		public GatewayUpdaterController(INetworkSynchronizer networkSynchronizer, ILoggerService loggerService)
		{
			_logger = loggerService.GetLogger(nameof(GatewayUpdaterController));
			_networkSynchronizer = networkSynchronizer;
		}

		[HttpPost("PackageUpdate")]
		public IActionResult PostPackageUpdate([FromBody] RtPackage rtPackage)
		{
			_logger.Debug("PostPackageUpdate invoked");

			_networkSynchronizer.ProcessRtPackage(rtPackage);

			return Ok();
		}

		[HttpPost("CheckConnectivity")]
		public IActionResult SetCheckConnectivity([FromBody] InfoMessage message)
        {
			if(int.TryParse(message.Message, out int nonce))
            {
				_networkSynchronizer.ConnectivityCheckSet(nonce);
            }

			return Ok();
		}
		
		[HttpGet]
		public async Task<IActionResult> CheckConnectivity()
        {
			Random r = new Random();
			int nonce = r.Next();

			var awaiter = _networkSynchronizer.GetConnectivityCheckAwaiter(nonce);

            try
            {
				await awaiter.Task.TimeoutAfter(3000).ConfigureAwait(false);
				return Ok(new InfoMessage { Context = "Gateway", InfoType = "ConnectivityCheck", Message = "Succeeded" });
			}
			catch (TimeoutException)
            {
				return Ok(new InfoMessage { Context = "Gateway", InfoType = "ConnectivityCheck", Message = "Failed" });
            }
        }
	}
}
