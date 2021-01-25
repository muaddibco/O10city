using O10.Client.Common.Entities;
using O10.Core.Architecture;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace O10.Client.Common.Integration
{
    [ExtensionPoint]
    public interface IIntegrationIdP : IIntegration
    {
        Task<ActionStatus> Register(long accountId);

        Task<ActionStatus> StoreScheme(long accountId, AttributeDefinition[] attributeDefinitions);

        Task<ActionStatus> IssueAttributes(long accountId, IssuanceDetails issuanceDetails);
    }
}
