using System.Collections.Generic;
using O10.Client.Common.Entities;
using O10.Client.DataLayer.Enums;
using O10.Client.DataLayer.Model;
using O10.Core.Architecture;

namespace O10.Client.Common.Interfaces
{
    [ServiceContract]
    public interface IAccountsService
    {
        AccountDescriptor Authenticate(long accountId, string password);
        List<AccountDescriptor> GetAll();
        AccountDescriptor GetById(long id);
        long Create(AccountType accountType, string accountInfo = null, string password = null, bool isPrivate = false);
        void Update(long accountId, string accountInfo = null, string password = null);
        void Override(AccountType accountType, long accountId, byte[] secretSpendKey, byte[] secretViewKey, string password, long lastRegistryCombinedBlockHeight);
        void Update(AccountDescriptor user, string password = null);
        void Delete(long id);
        void ResetAccount(long accountId, string password);
    }
}
