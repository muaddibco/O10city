using System.Collections.Generic;
using O10.Client.DataLayer.Model;
using O10.Core.Architecture;

namespace O10.Client.Mobile.Base.Interfaces
{
    [ServiceContract]
    public interface IDataAccessServiceWrapper
    {
        IEnumerable<UserRootAttribute> GetUserAttributes(long accountId);

        void AddOrUpdateUserIdentityIsser(string key, string issuerAlias, string description);

        string GetUserIdentityIsserAlias(string key);

        IEnumerable<(string schemeName, string content)> GetUserAssociatedAttributes(long accountId, string issuer);
    }
}
