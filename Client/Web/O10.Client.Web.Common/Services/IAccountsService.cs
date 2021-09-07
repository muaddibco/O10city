using LanguageExt;
using O10.Client.Common.Entities;
using O10.Client.Common.Interfaces;
using O10.Core.Architecture;

namespace O10.Client.Web.Common.Services
{
    [ServiceContract]
    public interface IAccountsServiceEx : IAccountsService
    {
        AccountDescriptor GetByPublicKey(byte[] publicKey);

        Option<AccountDescriptor> DuplicateAccount(long id, string accountInfo);
	}
}
