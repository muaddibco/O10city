# O10
>Faber est suae quisque fortunae - Each man is the maker of his own fortune

(Guy Sallust Crisp)

## Solutions composition
1. O10.Node - solution that holds all projects relevant for network node
2. O10.Gateway - solution that holds all projects relevant for gateway to the network
3. O10.Portal - solution that holds all projects relevant for demo portal (that uses Gateway for accessing network Nodes)
4. O10.MobileWallet - solution that holds all projects relevant for mobile wallet (that uses Gateway for accessing network Nodes)
5. O10All - solution that holds all projects of Node, Gateway, Demo Portal and DB Setup

## How to make local run via Visual Studio

### 1. Database initialization
Before running a Node, Gateway and a Portal it is needed to setup a database. For this get SQL Server image and run it with the following command:

docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=p@ssword1' -e 'MSSQL_PID=Express' -p 1433:1433 -d mcr.microsoft.com/mssql/server:2017-latest-ubuntu

### 2. Running a Node
Open O10.Node.sln file using your Visual Studio and run the project O10.Node.WebApp

### 3. Running a Gateway
Open O10.Gateway.sln file using your Visual Studio and run the project O10.Gateway.WebApp

### 4. Running a Portal
Open O10.Portal.sln file using your Visual Studio and run the project O10.Client.Web.Portal
