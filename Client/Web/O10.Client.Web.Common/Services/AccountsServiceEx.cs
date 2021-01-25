using O10.Client.DataLayer.Services;
using O10.Client.Common.Interfaces;
using O10.Core.Architecture;

using O10.Client.Common.Services;
using O10.Core.HashCalculations;
using O10.Client.Common.Entities;
using O10.Core.Translators;

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

        public AccountDescriptor GetByPublicKey(byte[] publicKey)
        {
            return TranslateToAccountDescriptor(_dataAccessService.GetAccount(publicKey));
        }

		public long DuplicateAccount(long id, string accountInfo)
		{
			long accountIdNew = _dataAccessService.DuplicateUserAccount(id, accountInfo);
            _dataAccessService.DuplicateAssociatedAttributes(id, accountIdNew);
            return accountIdNew;
		}
    }
}
