using O10.Client.Common.Dtos;
using O10.Core.Architecture;
using System.Threading.Tasks;

namespace O10.Client.Common.Integration
{
    [ExtensionPoint]
    public interface IIntegrationIdP : IIntegration
    {
        Task<ActionStatus> Register(long accountId);

        Task<ActionStatus> StoreScheme(long accountId, AttributeDefinitionDTO[] attributeDefinitions);

        Task<ActionStatus> IssueAttributes(long accountId, IssuanceDetails issuanceDetails);
    }
}
