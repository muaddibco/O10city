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

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        protected override async Task OnStop()
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
        }
    }
}
