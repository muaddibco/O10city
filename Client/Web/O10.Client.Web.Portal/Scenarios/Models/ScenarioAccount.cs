using System.Collections.Generic;
using O10.Client.DataLayer.Enums;

namespace O10.Client.Web.Portal.Scenarios.Models
{
    public class ScenarioAccount
    {
        public AccountType AccountType { get; set; }

        public string AccountInfo { get; set; }

        #region Identity Provider

        public IEnumerable<ScenarioAttributeScheme> IdentityScheme { get; set; }

        public IEnumerable<ScenarionIdentity> Identities { get; set; }

        #endregion Identity Provider

        #region Service Provider

        public IEnumerable<ScenarioRelationGroup> RelationGroups { get; set; }

        #endregion Service Provider
    }
}
