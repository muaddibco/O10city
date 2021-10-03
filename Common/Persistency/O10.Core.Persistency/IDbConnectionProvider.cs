using O10.Core.Architecture;
using System.Data;

namespace O10.Core.Persistency
{
    [ExtensionPoint]
    public interface IDbConnectionProvider
    {
        string ConnectionType { get; }

        IDbConnection GetDbConnection();
    }
}
