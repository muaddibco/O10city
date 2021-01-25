using System.Collections.Generic;
using System.Linq;
using O10.Core.Architecture;


namespace O10.Client.Web.Portal.ExternalIdps.Validators
{
    [RegisterDefaultImplementation(typeof(IExternalIdpDataValidatorsRepository), Lifetime = LifetimeManagement.Singleton)]
    public class ExternalIdpDataValidatorsRepository : IExternalIdpDataValidatorsRepository
    {
        private readonly IEnumerable<IExternalIdpDataValidator> _validators;

        public ExternalIdpDataValidatorsRepository(IEnumerable<IExternalIdpDataValidator> validators)
        {
            _validators = validators;
        }

        public IExternalIdpDataValidator GetInstance(string key)
        {
            return _validators.FirstOrDefault(v => v.Name == key);
        }
    }
}
