# O10
>Faber est suae quisque fortunae - Each man is the maker of his own fortune

(Guy Sallust Crisp)

## Solutions composition
1. O10.Node - solution that holds all projects relevant for network node
2. O10.Gateway - solution that holds all projects relevant for gateway to the network
3. O10.Portal - solution that holds all projects relevant for demo portal (that uses Gateway for accessing network Nodes)
4. O10.MobileWallet - solution that holds all projects relevant for mobile wallet (that uses Gateway for accessing network Nodes)
5. O10All - solution that holds all projects of Node, Gateway, Demo Portal and DB Setup

## How to make local run

### 1. Database initialization
Before running a Node, Gateway and a Portal it is needed to initialize database. 
Launch O10.Setup.Simulation.exe "As Administrator" with argument --WipeAll. 
This executable located at "O10.Node\NodeSetup\O10.Setup.Simulation". 
Do not close window when it will finish - copy aside key printed in console window and only then close window.

### 2. Running a Node
In order to run a Node launch O10.Node.Console.exe and provide it with key copied aside during database initialization step.

### 3. Running a Gateway
Prior to running a Gateway locally it is required to publish it to a local folder.
Once Gateway is published successfully, launch it with following command (from the folder that Gateway was published to):

    > dotnet O10.Server.Gateway.dll

### 4. Running a Portal
Prior to running a Portal locally it is required to publish it to a local folder.
Once Portal is published successfully, launch it with following command (from the folder that Portal was published to):

    > dotnet O10.Client.Web.Portal.dll --server.urls="http://0.0.0.0:5050"

When Portal is up and running it is available for browing with URL http://localhost:5050