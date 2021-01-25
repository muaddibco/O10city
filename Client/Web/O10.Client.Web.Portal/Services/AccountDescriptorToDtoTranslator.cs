using O10.Client.Common.Entities;
using O10.Client.Web.Portal.Dtos;
using O10.Core.Translators;
using O10.Core.ExtensionMethods;
using O10.Core.Architecture;


namespace O10.Client.Web.Portal.Services
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class AccountDescriptorToDtoTranslator : TranslatorBase<AccountDescriptor, AccountDto>
    {
        public override AccountDto Translate(AccountDescriptor obj)
        {
            return new AccountDto
            {
                AccountId = obj.AccountId,
                AccountType = (byte)obj.AccountType,
                AccountInfo = obj.AccountInfo,
                PublicSpendKey = obj.PublicSpendKey.ToHexString(),
                PublicViewKey = obj.PublicViewKey.ToHexString()
            };
        }
    }
}
