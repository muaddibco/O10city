using O10.Core.Translators;
using O10.Core.ExtensionMethods;
using O10.Core.Architecture;
using O10.Client.Web.DataContracts;
using O10.Client.Common.Dtos;

namespace O10.Client.Web.Portal.Services
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class AccountDescriptorToDtoTranslator : TranslatorBase<AccountDescriptorDTO?, AccountDto?>
    {
        public override AccountDto? Translate(AccountDescriptorDTO? obj)
        {
            if(obj == null)
            {
                return null;
            }

            return new AccountDto
            {
                AccountId = obj.AccountId,
                AccountType = obj.AccountType,
                AccountInfo = obj.AccountInfo,
                PublicSpendKey = obj.PublicSpendKey.ToHexString(),
                PublicViewKey = obj.PublicViewKey.ToHexString()
            };
        }
    }
}
