﻿using Microsoft.EntityFrameworkCore;
using O10.Core;
using O10.Core.Architecture;

namespace O10.Node.Core.DataLayer.DataContexts.SQLite
{
    [RegisterExtension(typeof(IDataContext), Lifetime = LifetimeManagement.Singleton)]
    public class DataContext : InternalDataContextBase
    {
        public override string DataProvider => "SQLite";
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(ConnectionString ?? "Filename=core.dat");
            ManualResetEventSlim.Set();
        }
    }
}
