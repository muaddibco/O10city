using System.Threading;
using O10.Core.Logging;
using O10.Network.Handlers;

namespace O10.Node.Core
{
    /// <summary>
    /// Main class with business logic of Node.
    /// 
    /// Process of start-up:
    ///  1. Initialize - it creates, initializes and launches listeners of other nodes and wallet accounts
    ///  2. EnterGroup - before Node can start to function it must to connect to some consensus group of Nodes for consensus decisions accepting
    ///  3. Start - after Node entered to any consensus group it starts to work
    /// </summary>
    public class NodeMain
    {
        private readonly ILogger _log;
        private readonly IPacketsHandler _packetsHandler;

        public NodeMain(IPacketsHandler packetsHandler, ILoggerService loggerService)
        {
            _log = loggerService.GetLogger(GetType().Name);
            _packetsHandler = packetsHandler;
        }

        public void Initialize(CancellationToken ct)
        {
            _packetsHandler.Initialize(ct);
        }

        internal void Start()
        {
            _packetsHandler.Start();
        }
    }
}
