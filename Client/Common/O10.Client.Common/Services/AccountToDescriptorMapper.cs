using O10.Client.Common.Dtos;
using O10.Client.DataLayer.Model;
using O10.Core.Architecture;
using O10.Core.Translators;

namespace O10.Client.Common.Services
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class AccountToDescriptorTranslator : TranslatorBase<Account, AccountDescriptorDTO>
    {
        public override AccountDescriptorDTO Translate(Account account)
        {
            if (account == null)
            {
                return null;
            }

            return new AccountDescriptorDTO
            {
                AccountType = (AccountTypeDTO)account.AccountType,
                SecretSpendKey = account.SecretSpendKey,
                SecretViewKey = account.SecretViewKey,
                PublicSpendKey = account.PublicSpendKey,
                PublicViewKey = account.PublicViewKey,
                AccountInfo = account.AccountInfo,
                AccountId = account.AccountId,
                IsCompromised = account.IsCompromised,
                LastAggregatedRegistrations = account.LastAggregatedRegistrations,
                IsPrivate = account.IsPrivate,
                IsActive = account.PublicSpendKey != null
            };
        }
    }
}
