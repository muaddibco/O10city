using O10.Core;
using O10.Core.Architecture;
using System.Collections.Generic;

namespace O10.Client.Common.Integration
{
    [ServiceContract]
    public interface IIntegrationIdPRepository : IRepository<IIntegrationIdP, string>
    {
        IEnumerable<IIntegrationIdP> GetIntegrationIdPs();

        string IntegrationKeyName { get; }
    }
}
