using System.Collections.Generic;
using System.Linq;
using O10.Server.Gateway;

namespace O10.Gateway.ServiceFabric.WebApp
{
    public class GatewayServiceFabricBootstrapper : GatewayCommonBootstrapper
	{
		private readonly string[] _catalogItems = new string[] { "O10.Gateway.ServiceFabric.WebApp.dll" };

		public GatewayServiceFabricBootstrapper() : base()
		{
		}

		protected override IEnumerable<string> EnumerateCatalogItems(string rootFolder)
		{
			return base.EnumerateCatalogItems(rootFolder).Concat(_catalogItems);
		}
	}
}
