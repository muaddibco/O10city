using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using O10.Client.Common.Exceptions;
using O10.Client.Web.Portal.Dtos;
using O10.Client.Web.Portal.Exceptions;
using O10.Core.Logging;

namespace O10.Client.Web.Portal.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorsController : ControllerBase
    {
        private readonly ILogger _logger;

        public ErrorsController(ILoggerService loggerService)
        {
            _logger = loggerService.GetLogger(nameof(ErrorsController));
        }

        [Route("error")]
        public ErrorResponseDto Error()
        {

            var context = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var exception = context?.Error;
            var code = exception switch
            {
                AccountAuthenticationFailedException _ => StatusCodes.Status401Unauthorized,
                AccountNotFoundException _ => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError,
            };

            Response.StatusCode = code;

            _logger.Error($"Request failed. Path: {context.Path}", exception);

            return new ErrorResponseDto { Message = exception.Message };
        }
    }
}
