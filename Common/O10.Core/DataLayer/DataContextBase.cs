using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace O10.Core.DataLayer
{
    public abstract class DataContextBase : DbContext, IDataContext
    {
        public abstract string DataProvider { get; }
        protected string ConnectionString { get; private set; }

        protected ManualResetEventSlim ManualResetEventSlim { get; } = new ManualResetEventSlim(false);

        public void EnsureConfigurationCompleted()
        {
            ManualResetEventSlim.Wait();
        }

        public void Initialize(string connectionString) => ConnectionString = connectionString;
    }
}
