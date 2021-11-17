using LanguageExt;
using O10.Client.Common.Dtos;
using O10.Client.Common.Interfaces;
using O10.Core.Architecture;

namespace O10.Client.Web.Common.Services
{
    [ServiceContract]
    public interface IAccountsServiceEx : IAccountsService
    {
        AccountDescriptorDTO GetByPublicKey(byte[] publicKey);
        AccountDescriptorDTO? GetBySecrets(byte[] secretSpendKey, byte[] secretViewKey, string password);

        Option<AccountDescriptorDTO> DuplicateAccount(long sourceAccountId, long targetAccountId);
	}
}
