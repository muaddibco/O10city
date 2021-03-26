$name = $args[0]

Add-Migration -Name $name -StartupProject "O10.Node.WebApp" -Project "O10.Node.DataLayer" -Context "O10.Node.DataLayer.Specific.Synchronization.DataContexts.SqlServer.DataContext" -OutputDir "Specific/Synchronization/DataContexts/SqlServer/Migrations"
Add-Migration -Name $name -StartupProject "O10.Node.WebApp" -Project "O10.Node.DataLayer" -Context "O10.Node.DataLayer.Specific.Synchronization.DataContexts.SQLite.DataContext" -OutputDir "Specific/Synchronization/DataContexts/SQLite/Migrations"

Add-Migration -Name $name -StartupProject "O10.Node.WebApp" -Project "O10.Node.DataLayer" -Context "O10.Node.DataLayer.Specific.Registry.DataContexts.SqlServer.DataContext" -OutputDir "Specific/Registry/DataContexts/SqlServer/Migrations"
Add-Migration -Name $name -StartupProject "O10.Node.WebApp" -Project "O10.Node.DataLayer" -Context "O10.Node.DataLayer.Specific.Registry.DataContexts.SQLite.DataContext" -OutputDir "Specific/Registry/DataContexts/SQLite/Migrations"

Add-Migration -Name $name -StartupProject "O10.Node.WebApp" -Project "O10.Node.DataLayer" -Context "O10.Node.DataLayer.Specific.O10Id.DataContexts.SqlServer.DataContext" -OutputDir "Specific/O10Id/DataContexts/SqlServer/Migrations"
Add-Migration -Name $name -StartupProject "O10.Node.WebApp" -Project "O10.Node.DataLayer" -Context "O10.Node.DataLayer.Specific.O10Id.DataContexts.SQLite.DataContext" -OutputDir "Specific/O10Id/DataContexts/SQLite/Migrations"

Add-Migration -Name $name -StartupProject "O10.Node.WebApp" -Project "O10.Node.DataLayer" -Context "O10.Node.DataLayer.Specific.Stealth.DataContexts.SqlServer.DataContext" -OutputDir "Specific/Stealth/DataContexts/SqlServer/Migrations"
Add-Migration -Name $name -StartupProject "O10.Node.WebApp" -Project "O10.Node.DataLayer" -Context "O10.Node.DataLayer.Specific.Stealth.DataContexts.SQLite.DataContext" -OutputDir "Specific/Stealth/DataContexts/SQLite/Migrations"

Add-Migration -Name $name -StartupProject "O10.Node.WebApp" -Project "O10.Node.Core" -Context "O10.Node.Core.DataLayer.DataContexts.SqlServer.DataContext" -OutputDir "DataLayer/DataContexts/SqlServer/Migrations"
Add-Migration -Name $name -StartupProject "O10.Node.WebApp" -Project "O10.Node.Core" -Context "O10.Node.Core.DataLayer.DataContexts.SQLite.DataContext" -OutputDir "DataLayer/DataContexts/SQLite/Migrations"