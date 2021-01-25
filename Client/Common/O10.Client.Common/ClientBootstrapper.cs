using System.Collections.Generic;
using System.IO;
using System.Linq;
using O10.Core.Architecture;

namespace O10.Client.Common
{
    public class ClientBootstrapper : Bootstrapper
    {
        protected override IEnumerable<string> EnumerateCatalogItems(string rootFolder)
        {
            return base.EnumerateCatalogItems(rootFolder)
				.Union(new string[] { "O10.Crypto.dll", "O10.Transactions.*.dll" })
				.Concat(Directory.EnumerateFiles(rootFolder, "O10.Client.*.dll").Select(f => new FileInfo(f).Name));
        }
    }
}
