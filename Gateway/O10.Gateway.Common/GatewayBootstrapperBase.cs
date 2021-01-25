using System.Collections.Generic;
using System.IO;
using System.Linq;
using O10.Core.Architecture;

namespace O10.Gateway.Common
{
    public class GatewayBootstrapperBase : Bootstrapper
    {
        public GatewayBootstrapperBase() : base()
        {
        }

        protected override IEnumerable<string> EnumerateCatalogItems(string rootFolder)
        {
            return base.EnumerateCatalogItems(rootFolder)
				.Union(new string[] { "O10.Crypto.dll", "O10.Transactions.*.dll" })
				.Concat(Directory.EnumerateFiles(rootFolder, "O10.Gateway.*.dll").Select(f => new FileInfo(f).Name));
        }
    }
}
