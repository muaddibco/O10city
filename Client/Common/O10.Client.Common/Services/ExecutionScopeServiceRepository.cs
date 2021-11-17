using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Enums;
using O10.Core.Architecture;
using System.Collections.Generic;
using System.Linq;

namespace O10.Client.Common.Services
{
    [RegisterDefaultImplementation(typeof(IExecutionScopeServiceRepository), Lifetime = LifetimeManagement.Scoped)]
    public class ExecutionScopeServiceRepository : IExecutionScopeServiceRepository
    {
        private readonly IEnumerable<IExecutionScopeService> _executionScopeServices;

        public ExecutionScopeServiceRepository(IEnumerable<IExecutionScopeService> executionScopeServices)
        {
            _executionScopeServices = executionScopeServices;
        }

        public IExecutionScopeService? GetInstance(AccountType key)
        {
            return _executionScopeServices.FirstOrDefault(s => s.AccountType == key);
        }
    }
}
