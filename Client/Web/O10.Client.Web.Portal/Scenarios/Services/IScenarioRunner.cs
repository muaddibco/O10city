using System.Collections.Generic;
using O10.Client.DataLayer.Model.Scenarios;
using O10.Core.Architecture;
using O10.Client.Web.Portal.Scenarios.Models;

namespace O10.Client.Web.Portal.Scenarios.Services
{
    [ServiceContract]
    public interface IScenarioRunner
    {
        void Initialize();

        IEnumerable<ScenarioDefinition> GetScenarioDefinitions();

        ScenarioSession SetupScenario(string userSubject, int id);

        ScenarioSession AbandonScenario(string userSubject, int id);

        ScenarioSession ResumeScenario(string userSubject, int id);

        void ProgressScenario(string userSubject, int id, bool forward = true);

        string GetScenarioCurrentStepContent(string userSubject, int id);

        ScenarioSession GetActiveScenarioSession(string userSubject);
    }
}
