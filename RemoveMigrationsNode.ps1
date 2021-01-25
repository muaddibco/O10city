Remove-Migration -StartupProject "O10.Node.ServiceFabric.WebApp" -Project "O10.Node.DataLayer" -Context "O10.Node.DataLayer.Specific.Synchronization.DataContext.SqlServer.DataContext"
Remove-Migration -StartupProject "O10.Node.ServiceFabric.WebApp" -Project "O10.Node.DataLayer" -Context "O10.Node.DataLayer.Specific.Synchronization.DataContext.SQLite.DataContext"

Remove-Migration -StartupProject "O10.Node.ServiceFabric.WebApp" -Project "O10.Node.DataLayer" -Context "O10.Node.DataLayer.Specific.Registry.DataContext.SqlServer.DataContext"
Remove-Migration -StartupProject "O10.Node.ServiceFabric.WebApp" -Project "O10.Node.DataLayer" -Context "O10.Node.DataLayer.Specific.Registry.DataContext.SQLite.DataContext"

Remove-Migration -StartupProject "O10.Node.ServiceFabric.WebApp" -Project "O10.Node.DataLayer" -Context "O10.Node.DataLayer.Specific.O10Id.DataContext.SqlServer.DataContext"
Remove-Migration -StartupProject "O10.Node.ServiceFabric.WebApp" -Project "O10.Node.DataLayer" -Context "O10.Node.DataLayer.Specific.O10Id.DataContext.SQLite.DataContext"

Remove-Migration -StartupProject "O10.Node.ServiceFabric.WebApp" -Project "O10.Node.DataLayer" -Context "O10.Node.DataLayer.Specific.Ephemeral.DataContext.SqlServer.DataContext"
Remove-Migration -StartupProject "O10.Node.ServiceFabric.WebApp" -Project "O10.Node.DataLayer" -Context "O10.Node.DataLayer.Specific.Ephemeral.DataContext.SQLite.DataContext"

Remove-Migration -StartupProject "O10.Node.ServiceFabric.WebApp" -Project "O10.Node.Core" -Context "O10.Node.Core.DataLayer.DataContext.SqlServer.DataContext"
Remove-Migration -StartupProject "O10.Node.ServiceFabric.WebApp" -Project "O10.Node.Core" -Context "O10.Node.Core.DataLayer.DataContext.SQLite.DataContext"