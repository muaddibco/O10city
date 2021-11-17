using O10.Client.DataLayer.Services;
using O10.Client.Common.Interfaces;
using O10.Core.Architecture;

using O10.Client.Common.Services;
using O10.Core.HashCalculations;
using O10.Core.Translators;
using LanguageExt;
using O10.Client.Common.Dtos;

namespace O10.Client.Web.Common.Services
{
    [RegisterDefaultImplementation(typeof(IAccountsServiceEx), Lifetime = LifetimeManagement.Singleton)]
    public class AccountsServiceEx : AccountsService, IAccountsServiceEx
    {
		private readonly IDataAccessService _dataAccessService;

		public AccountsServiceEx(IDataAccessService dataAccessService,
                           IGatewayService gatewayService,
                           IHashCalculationsRepository hashCalculationsRepository,
                           ITranslatorsRepository translatorsRepository)
            : base(dataAccessService, gatewayService, hashCalculationsRepository, translatorsRepository)
		{
			_dataAccessService = dataAccessService;
		}

        public AccountDescriptorDTO GetByPublicKey(byte[] publicKey)
        {
            return TranslateToAccountDescriptor(_dataAccessService.GetAccount(publicKey));
        }

        public AccountDescriptorDTO? GetBySecrets(byte[] secretSpendKey, byte[] secretViewKey, string password)
        {
            GeneratePasswordKeys(AccountTypeDTO.User, password, secretSpendKey, secretViewKey, out var publicSpendKey, out var publicViewKey);
            return TranslateToAccountDescriptor(_dataAccessService.FindUserAccountByKeys(publicSpendKey, publicViewKey));
        }


        public Option<AccountDescriptorDTO> DuplicateAccount(long sourceAccountId, long targetAccountId)
		{
			var account = _dataAccessService.DuplicateUserAccount(sourceAccountId, targetAccountId);
            _dataAccessService.DuplicateAssociatedAttributes(sourceAccountId, account.AccountId);
            return TranslateToAccountDescriptor(account);
		}
    }
}
