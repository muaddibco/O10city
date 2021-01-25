using System.Threading.Tasks.Dataflow;
using O10.Client.DataLayer.Services;
using O10.Core.Models;

namespace O10.Client.Web.Saml.Common.Services
{
    public class SamlIdpWitnessPackageUpdater
	{
		private readonly long _accountId;
		private readonly IDataAccessService _dataAccessService;

		public SamlIdpWitnessPackageUpdater(long accountId, IDataAccessService dataAccessService)
		{
			WitnessPackagePipeIn = new ActionBlock<WitnessPackage>(w => 
			{
				_dataAccessService.StoreLastUpdatedCombinedBlockHeight(_accountId, w.CombinedBlockHeight);
			});
			_accountId = accountId;
			_dataAccessService = dataAccessService;
		}

		public ActionBlock<WitnessPackage> WitnessPackagePipeIn { get; }
	}
}
