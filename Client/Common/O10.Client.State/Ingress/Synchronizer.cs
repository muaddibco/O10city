using O10.Client.Common.Communication;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Services;
using O10.Core.Architecture;
using O10.Core.Logging;

namespace O10.Client.State.Ingress
{
    [RegisterExtension(typeof(ISynchronizer), Lifetime = LifetimeManagement.Scoped)]
    public class Synchronizer : SynchronizerBase
    {
        public Synchronizer(IDataAccessService dataAccessService,
                            IStateClientCryptoService clientCryptoService,
                            ILoggerService loggerService)
            : base(dataAccessService, clientCryptoService, loggerService)
        {
        }

        public override string Name => "State";
    }
}
