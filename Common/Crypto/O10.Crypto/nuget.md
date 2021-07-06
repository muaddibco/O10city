## How to create a nuget package
1. Download the latest nuget.exe into the root folder of the project
2. Run the following CLI command:

`.\nuget.exe pack -Build -IncludeReferencedProjects -Properties Configuration=Release O10.Crypto.csproj`