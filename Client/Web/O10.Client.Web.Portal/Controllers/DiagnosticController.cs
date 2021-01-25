using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using O10.Client.Common.Interfaces;
using O10.Client.Web.Common.Hubs;
using O10.Client.Web.Portal.Dtos;
using O10.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace O10.Client.Web.Portal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiagnosticController : ControllerBase
    {
        private readonly IGatewayService _gatewayService;
        private readonly IHubContext<IdentitiesHub> _idenitiesHubContext;

        public DiagnosticController(IGatewayService gatewayService, IHubContext<IdentitiesHub> idenitiesHubContext)
        {
            _gatewayService = gatewayService;
            _idenitiesHubContext = idenitiesHubContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InfoMessage>>> GetInfo()
        {
            string version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
            List<InfoMessage> portalInfo = new List<InfoMessage> { new InfoMessage { Context = "Portal", InfoType = "Version", Message = version } };
            IEnumerable<InfoMessage> gatewayInfo;
            try
            {
                gatewayInfo = await _gatewayService.GetInfo().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                gatewayInfo = new List<InfoMessage> { new InfoMessage { Context = "Gateway", InfoType = "Error", Message = $"Failed to connect due to the error '{ex.Message}'" } };
            }

            return Ok(portalInfo.Concat(gatewayInfo));
        }

        [HttpPost("SignalR")]
        public IActionResult TestSignalR(long accountId = 0, string method = "Test", string msg = "Test Message")
        {
            if(accountId != 0)
            {
                _idenitiesHubContext.Clients.Group(accountId.ToString()).SendAsync(method, new HubDiagnosticMessage { Message = msg });
            }
            else
            {
                _idenitiesHubContext.Clients.All.SendAsync(method, new HubDiagnosticMessage { Message = msg });
            }

            return Ok();
        }
    }
}
