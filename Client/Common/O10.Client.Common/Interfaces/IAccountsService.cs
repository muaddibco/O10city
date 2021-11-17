using System.Collections.Generic;
using O10.Client.Common.Dtos;
using O10.Core.Architecture;

namespace O10.Client.Common.Interfaces
{
    [ServiceContract]
    public interface IAccountsService
    {
        AccountDescriptorDTO Authenticate(long accountId, string password);
        List<AccountDescriptorDTO> GetAll();
        AccountDescriptorDTO? FindByAlias(string alias);
        AccountDescriptorDTO GetById(long id);
        long Create(AccountTypeDTO accountType, string accountInfo = null, string password = null, bool isPrivate = false);
        void Update(long accountId, string accountInfo = null, string password = null);
        void Override(AccountTypeDTO accountType, long accountId, byte[] secretSpendKey, byte[] secretViewKey, string password, long lastRegistryCombinedBlockHeight);
        void Update(AccountDescriptorDTO user, string password = null);
        void Delete(long id);
        void ResetAccount(long accountId, string password);
    }
}
