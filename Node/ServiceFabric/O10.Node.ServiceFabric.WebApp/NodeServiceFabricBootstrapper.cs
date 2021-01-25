using System.Collections.Generic;
using System.Linq;
using O10.Node.WebApp.Common;

namespace O10.Node.ServiceFabric.WebApp
{
	public class NodeServiceFabricBootstrapper : NodeWebAppBootstrapper
	{
		private readonly string[] _catalogItems = new string[] { "O10.Node.ServiceFabric.WebApp.dll" };

		public NodeServiceFabricBootstrapper()
		{
		}

		protected override IEnumerable<string> EnumerateCatalogItems(string rootFolder)
		{
			return base.EnumerateCatalogItems(rootFolder).Concat(_catalogItems);
		}
	}
}
