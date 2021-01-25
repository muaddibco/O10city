using System.Collections.Generic;
using O10.Client.DataLayer.Model;
using O10.Client.DataLayer.Services;
using O10.Core.Architecture;
using O10.Client.Mobile.Base.Interfaces;

namespace O10.Client.Mobile.Base.Services
{
    [RegisterDefaultImplementation(typeof(IDataAccessServiceWrapper), Lifetime = LifetimeManagement.Singleton)]
    public class DataAccessServiceWrapper : IDataAccessServiceWrapper
    {
        private readonly IDataAccessService _dataAccessService;

        public DataAccessServiceWrapper(IDataAccessService dataAccessService)
        {
            _dataAccessService = dataAccessService;
        }

        public void AddOrUpdateUserIdentityIsser(string key, string alias, string description) => _dataAccessService.AddOrUpdateUserIdentityIsser(key, alias, description);

        public IEnumerable<(string schemeName, string content)> GetUserAssociatedAttributes(long accountId, string issuer) => _dataAccessService.GetUserAssociatedAttributes(accountId, issuer);

        public IEnumerable<UserRootAttribute> GetUserAttributes(long accountId) => _dataAccessService.GetUserAttributes(accountId);

        public string GetUserIdentityIsserAlias(string key) => _dataAccessService.GetUserIdentityIsserAlias(key);
    }
}
