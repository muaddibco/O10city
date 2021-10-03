using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace O10.Core.Persistency
{
    public abstract class DataContextBase : DbContext, IDataContext
    {
        public abstract string DataProvider { get; }
        protected string ConnectionString { get; private set; }

        protected ManualResetEventSlim ManualResetEventSlim { get; } = new ManualResetEventSlim(false);

        public IDataContext EnsureConfigurationCompleted()
        {
            ManualResetEventSlim.Wait();
            return this;
        }

        public IDataContext Initialize(string connectionString)
        {
            ConnectionString = connectionString;
            return this;
        }

        public DataContextBase Migrate()
        {
            Database.Migrate();
            return this;
        }
    }
}
