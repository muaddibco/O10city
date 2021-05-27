using Chaos.NaCl;
using O10.Client.Common.Entities;
using O10.Client.DataLayer.Enums;
using O10.Client.DataLayer.Model;
using O10.Core.Architecture;

using O10.Core.Translators;
using O10.Crypto.ConfidentialAssets;

namespace O10.Client.Common.Services
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class AccountToDescriptorTranslator : TranslatorBase<Account, AccountDescriptor>
    {
        public override AccountDescriptor Translate(Account account)
        {
            if (account == null)
            {
                return null;
            }

            return new AccountDescriptor
            {
                AccountType = account.AccountType,
                SecretSpendKey = account.SecretSpendKey,
                SecretViewKey = account.SecretViewKey,
                PublicSpendKey = account.AccountType == AccountType.User ? CryptoHelper.GetPublicKey(account.SecretSpendKey) : CryptoHelper.GetPublicKey(Ed25519.SecretKeyFromSeed(account.SecretSpendKey)),
                PublicViewKey = account.AccountType == AccountType.User ? CryptoHelper.GetPublicKey(account.SecretViewKey) : null,
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
