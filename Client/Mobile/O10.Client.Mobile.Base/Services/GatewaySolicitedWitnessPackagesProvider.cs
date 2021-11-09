using System.Threading.Tasks;
using O10.Client.Common.Interfaces;
using O10.Client.Common.Services;
using O10.Client.DataLayer.Services;
using O10.Core.Architecture;
using O10.Core.Logging;

namespace O10.Client.Mobile.Base.Services
{
    [RegisterExtension(typeof(IWitnessPackagesProvider), Lifetime = LifetimeManagement.Scoped)]
    public class GatewaySolicitedWitnessPackagesProvider : WitnessPackageProviderBase
    {
        public GatewaySolicitedWitnessPackagesProvider(IGatewayService gatewayService, IDataAccessService dataAccessService, ILoggerService loggerService)
            : base(gatewayService, dataAccessService, loggerService)
        {
        }

        public override string Name => "GatewaySolicited";

        public override Task Restart()
        {
            throw new System.NotImplementedException();
        }

        public override async Task Start()
        {
            await AscertainAccountIsUpToDate().ConfigureAwait(false);
        }

        protected override Task InitializeInner()
        {
            return Task.CompletedTask;
        }

        protected override async Task OnStop()
        {
        }
    }
}
