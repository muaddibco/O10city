using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using O10.Core.Models;
using O10.Core.Persistency;
using O10.Gateway.Api;
using O10.Node.Core.Centralized;
using O10.Node.Core.DataLayer;

namespace O10.Node.WebApp.Common.Controllers
{
    [Route("api/[controller]")]
	[ApiController]
	public class ServiceController : ControllerBase
	{
		private readonly INotificationsService _notificationsService;
		private readonly DataAccessService _dataAccessService;

		public ServiceController(INotificationsService notificationsService, IDataAccessServiceRepository dataAccessServiceRepository)
		{
            if (dataAccessServiceRepository is null)
            {
                throw new System.ArgumentNullException(nameof(dataAccessServiceRepository));
            }

            _notificationsService = notificationsService ?? throw new System.ArgumentNullException(nameof(notificationsService));
            _dataAccessService = dataAccessServiceRepository.GetInstance<DataAccessService>();
		}

		[HttpGet("Gateways")]
		public async Task<ActionResult<List<GatewayDto>>> GetGateways(CancellationToken cancellationToken)
		{
			return (await _dataAccessService.GetGateways(cancellationToken))
				.Select(g => new GatewayDto { GatewayId = g.GatewayId, Alias = g.Alias, Uri = g.BaseUri})
				.ToList();
		}

		[HttpDelete("Gateways")]
		public async Task<IActionResult> DeleteGateway(long gatewayId)
        {
			var gateway = await _dataAccessService.RemoveGateway(gatewayId);

			if(gateway != null)
            {
				return Ok(gateway);
            }

			return BadRequest();
        }

		[HttpPost("Gateways")]
		public async Task<ActionResult<bool>> AddGateway([FromBody] GatewayDto gateway, CancellationToken cancellationToken)
		{
            if (gateway is null)
            {
                throw new System.ArgumentNullException(nameof(gateway));
            }

            bool res = await _dataAccessService.AddGateway(gateway.Alias, gateway.Uri, cancellationToken);
			if(res)
			{
				_notificationsService.UpdateGateways(cancellationToken);
			}

			return res;
		}

		[HttpPost("ConnectivityCheck")]
		public async Task<IActionResult> ConnectivityCheck([FromBody] InfoMessage message)
		{
			await _notificationsService.GatewaysConnectivityCheck(message).ConfigureAwait(false);

			return Ok();
		}
	}
}
