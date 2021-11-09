using O10.Core.Architecture;
using System;

namespace O10.Core.HealthChecks
{
    [ServiceContract]
    public interface IHealthChecksProvider : IRepository<IHealthCheckService, Type>
    {
    }
}
