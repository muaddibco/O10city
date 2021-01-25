using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using O10.Client.DataLayer.Model.Scenarios;
using O10.Client.DataLayer.Services;
using O10.Core.Logging;
using O10.Client.Web.Portal.Dtos.Scenarios;
using O10.Client.Web.Portal.Scenarios.Models;
using O10.Client.Web.Portal.Scenarios.Services;

namespace O10.Client.Web.Portal.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ScenariosController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IScenarioRunner _scenarioRunner;
        private readonly IDataAccessService _dataAccessService;

        public ScenariosController(ILoggerService loggerService, IScenarioRunner scenarioRunner, IDataAccessService dataAccessService)
        {
            _logger = loggerService.GetLogger(nameof(ScenariosController));
            _scenarioRunner = scenarioRunner;
            _dataAccessService = dataAccessService;
        }

        [HttpGet("ActiveScenario")]
        public ActionResult<ScenarioDto> GetActiveScenario()
        {
            try
            {
                _logger.Info($"{nameof(GetActiveScenario)}()");
                IEnumerable<ScenarioDefinition> scenarioDefinitions = _scenarioRunner.GetScenarioDefinitions();

                ScenarioSession scenarioSession = _scenarioRunner.GetActiveScenarioSession(User.Identity.Name);

                if (scenarioSession != null)
                {
                    ScenarioDefinition scenarioDefinition = scenarioDefinitions.FirstOrDefault(s => s.Id == scenarioSession.ScenarioId);
                    return new ScenarioDto
                    {
                        Name = scenarioDefinition.Name,
                        Id = scenarioDefinition.Id.ToString(),
                        IsActive = scenarioSession != null,
                        CurrentStep = scenarioSession?.CurrentStep ?? 0,
                        SessionId = scenarioSession?.ScenarioSessionId ?? 0,
                        StartTime = scenarioSession?.StartTime ?? DateTime.MinValue,
                        Steps = scenarioDefinition.Steps.Select(s => new ScenarioStepDto { Id = s.Id, Caption = s.Caption }).ToArray()
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed {nameof(GetActiveScenario)}()", ex);

                throw;
            }
        }

        [HttpGet]
        public ActionResult<IEnumerable<ScenarioDto>> GetScenarios()
        {
            IEnumerable<ScenarioDefinition> scenarioDefinitions = _scenarioRunner.GetScenarioDefinitions();
            IEnumerable<ScenarioSession> scenarioSessions = _dataAccessService.GetScenarioSessions(User.Identity.Name);

            IEnumerable<ScenarioDto> scenarios = scenarioDefinitions.Select(s =>
            {
                var scenarioSession = scenarioSessions.FirstOrDefault(ss => ss.ScenarioId == s.Id);
                return new ScenarioDto
                {
                    Name = s.Name,
                    Id = s.Id.ToString(),
                    IsActive = scenarioSession != null,
                    CurrentStep = scenarioSession?.CurrentStep ?? 0,
                    SessionId = scenarioSession?.ScenarioSessionId ?? 0,
                    StartTime = scenarioSession?.StartTime ?? DateTime.MinValue,
                    Steps = s.Steps.Select(s => new ScenarioStepDto { Id = s.Id, Caption = s.Caption }).ToArray()
                };
            });

            return Ok(scenarios);
        }

        [HttpGet("{scenarioId}")]
        public ActionResult<ScenarioDto> GetScenario(int scenarioId)
        {
            IEnumerable<ScenarioDefinition> scenarioDefinitions = _scenarioRunner.GetScenarioDefinitions();
            IEnumerable<ScenarioSession> scenarioSessions = _dataAccessService.GetScenarioSessions(User.Identity.Name);

            ScenarioDefinition scenarioDefinition = scenarioDefinitions.FirstOrDefault(s => s.Id == scenarioId);
            if (scenarioDefinition != null)
            {
                var scenarioSession = scenarioSessions.FirstOrDefault(ss => ss.ScenarioId == scenarioDefinition.Id);
                var scenario = new ScenarioDto
                {
                    Name = scenarioDefinition.Name,
                    Id = scenarioId.ToString(),
                    IsActive = scenarioSession != null,
                    CurrentStep = scenarioSession?.CurrentStep ?? 0,
                    SessionId = scenarioSession?.ScenarioSessionId ?? 0,
                    StartTime = scenarioSession?.StartTime ?? DateTime.MinValue,
                    Steps = scenarioDefinition.Steps.Select(s => new ScenarioStepDto { Id = s.Id, Caption = s.Caption }).ToArray()
                };

                return Ok(scenario);
            }

            return BadRequest();
        }

        [HttpDelete("{scenarioId}")]
        public IActionResult AbandonScenarioSession(int scenarioId)
        {
            _scenarioRunner.AbandonScenario(User.Identity.Name, scenarioId);
            _dataAccessService.RemoveScenarioSession(User.Identity.Name, scenarioId);

            return Ok();
        }

        [HttpPut("{scenarioId}")]
        public ActionResult<ScenarioDto> StartNewScenario(int scenarioId)
        {
            IEnumerable<ScenarioDefinition> scenarioDefinitions = _scenarioRunner.GetScenarioDefinitions();
            ScenarioDefinition scenarioDefinition = scenarioDefinitions.FirstOrDefault(s => s.Id == scenarioId);

            ScenarioSession scenarioSession = _scenarioRunner.SetupScenario(User.Identity.Name, scenarioId);

            return Ok(new ScenarioDto
            {
                Name = scenarioDefinition.Name,
                Id = scenarioId.ToString(),
                IsActive = scenarioSession != null,
                CurrentStep = scenarioSession?.CurrentStep ?? 0,
                SessionId = scenarioSession?.ScenarioSessionId ?? 0,
                StartTime = scenarioSession?.StartTime ?? DateTime.MinValue,
                Steps = scenarioDefinition.Steps.Select(s => new ScenarioStepDto { Id = s.Id, Caption = s.Caption }).ToArray()
            });
        }

        [HttpPost("{scenarioId}")]
        public ActionResult<ScenarioDto> ResumeScenario(int scenarioId)
        {
            IEnumerable<ScenarioDefinition> scenarioDefinitions = _scenarioRunner.GetScenarioDefinitions();
            ScenarioDefinition scenarioDefinition = scenarioDefinitions.FirstOrDefault(s => s.Id == scenarioId);

            ScenarioSession scenarioSession = _scenarioRunner.ResumeScenario(User.Identity.Name, scenarioId);

            return Ok(new ScenarioDto
            {
                Name = scenarioDefinition.Name,
                Id = scenarioId.ToString(),
                IsActive = scenarioSession != null,
                CurrentStep = scenarioSession?.CurrentStep ?? 0,
                SessionId = scenarioSession?.ScenarioSessionId ?? 0,
                StartTime = scenarioSession?.StartTime ?? DateTime.MinValue,
                Steps = scenarioDefinition.Steps.Select(s => new ScenarioStepDto { Id = s.Id, Caption = s.Caption }).ToArray()
            });
        }

        [HttpGet("{scenarioId}/Step")]
        public IActionResult GetStepContent(int scenarioId)
        {
            return Ok(new { content = _scenarioRunner.GetScenarioCurrentStepContent(User.Identity.Name, scenarioId) });
        }

        [HttpPost("{scenarioId}/Step")]
        public IActionResult ChangeStep(int scenarioId, [FromQuery] bool forward = true)
        {
            _scenarioRunner.ProgressScenario(User.Identity.Name, scenarioId, forward);

            return Ok(new { content = _scenarioRunner.GetScenarioCurrentStepContent(User.Identity.Name, scenarioId) });
        }
    }
}
