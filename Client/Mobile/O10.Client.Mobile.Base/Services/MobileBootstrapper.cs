using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using O10.Client.Common;
using O10.Client.Common.Configuration;
using O10.Client.Common.Interfaces;
using O10.Core.Configuration;
using O10.Core.Logging;

namespace O10.Client.Mobile.Base.Services
{
    public class MobileBootstrapper : ClientBootstrapper
    {
        //============================================================================
        //                                 MEMBERS
        //============================================================================

        private readonly string[] _catalogItems = new string[] { "O10.Client.Mobile.Base.dll" };

        //============================================================================
        //                                  C'TOR
        //============================================================================

        public MobileBootstrapper()
        {
        }

        //============================================================================
        //                                FUNCTIONS
        //============================================================================

        #region ============ PUBLIC FUNCTIONS =============  

        public override void RunInitializers(IServiceProvider serviceProvider, CancellationToken cancellationToken, ILogger log)
        {
            base.RunInitializers(serviceProvider, cancellationToken, log);

            IGatewayService gatewayService = serviceProvider.GetService<IGatewayService>();
            IRestApiConfiguration configuration = serviceProvider.GetService<IConfigurationService>().Get<IRestApiConfiguration>();
            gatewayService.Initialize(configuration.GatewayUri, cancellationToken);
        }

        protected

        #endregion

        #region ============ PRIVATE FUNCTIONS ============ 

        override IEnumerable<string> EnumerateCatalogItems(string rootFolder) => base.EnumerateCatalogItems(rootFolder).Concat(_catalogItems);

        #endregion

    }
}
