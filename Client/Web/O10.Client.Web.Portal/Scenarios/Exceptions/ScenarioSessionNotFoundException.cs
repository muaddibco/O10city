using System;
using O10.Client.Web.Portal.Properties;

namespace O10.Client.Web.Portal.Scenarios.Exceptions
{

    [Serializable]
    public class ScenarioSessionNotFoundException : Exception
    {
        public ScenarioSessionNotFoundException() { }
        public ScenarioSessionNotFoundException(string user, int sessionId) : base(string.Format(Resources.ERR_SCENARIO_SESSION_NOT_FOUND, sessionId, user)) { }
        public ScenarioSessionNotFoundException(string user, int sessionId, Exception inner) : base(string.Format(Resources.ERR_SCENARIO_SESSION_NOT_FOUND, sessionId, user), inner) { }
        protected ScenarioSessionNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
