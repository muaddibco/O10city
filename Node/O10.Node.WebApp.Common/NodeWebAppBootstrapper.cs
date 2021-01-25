using System.Collections.Generic;
using System.Linq;
using O10.Node.Core.Common;

namespace O10.Node.WebApp.Common
{
	public class NodeWebAppBootstrapper : NodeBootstrapper
	{
		private readonly string[] _catalogItems = new string[] { "O10.Node.WebApp.Common.dll" };

		protected override IEnumerable<string> EnumerateCatalogItems(string rootFolder)
		{
			return base.EnumerateCatalogItems(rootFolder).Concat(_catalogItems);
		}
	}
}
