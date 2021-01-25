using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Services;
using O10.Core.Architecture;
using O10.Core.Logging;

namespace O10.Client.Common.Communication
{
    [RegisterExtension(typeof(IWalletSynchronizer), Lifetime = LifetimeManagement.Scoped)]
    public class StateWalletSynchronizer : WalletSynchronizer
    {
        public StateWalletSynchronizer(IDataAccessService dataAccessService,
                                       IStateClientCryptoService clientCryptoService,
                                       ILoggerService loggerService) 
            : base(dataAccessService, clientCryptoService, loggerService)
        {
        }

        public override string Name => "State";
    }
}
