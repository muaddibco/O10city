using System.Collections.Generic;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Exceptions;
using O10.Transactions.Core.Interfaces;
using O10.Core.Architecture;


namespace O10.Transactions.Core
{
    [RegisterDefaultImplementation(typeof(IPacketsHandlersRegistry), Lifetime = LifetimeManagement.Singleton)]
    public class PacketsHandlersRegistry : IPacketsHandlersRegistry
    {
        private readonly Dictionary<string, IPacketsHandler> _packetHandlers;
        private readonly Dictionary<LedgerType, HashSet<IPacketsHandler>> _packetHandlersRegistered;

        public PacketsHandlersRegistry(IEnumerable<IPacketsHandler> packetHandlers)
        {
            _packetHandlersRegistered = new Dictionary<LedgerType, HashSet<IPacketsHandler>>();
            _packetHandlers = new Dictionary<string, IPacketsHandler>();

			if (packetHandlers != null)
			{
				foreach (IPacketsHandler packetHandler in packetHandlers)
				{
					if (!_packetHandlers.ContainsKey(packetHandler.Name))
					{
						_packetHandlers.Add(packetHandler.Name, packetHandler);
					}
				}
			}
        }

        public IPacketsHandler GetInstance(string blocksProcessorName)
        {
            if (!_packetHandlers.ContainsKey(blocksProcessorName))
            {
                throw new BlocksProcessorNotRegisteredException(blocksProcessorName);
            }

            return _packetHandlers[blocksProcessorName];
        }

        public IEnumerable<IPacketsHandler> GetBulkInstances(LedgerType key)
        {
            if(!_packetHandlersRegistered.ContainsKey(key))
            {
                throw new BlockHandlerNotSupportedException(key);
            }

            return _packetHandlersRegistered[key];
        }

        public void RegisterInstance(IPacketsHandler obj)
        {
            if(!_packetHandlersRegistered.ContainsKey(obj.LedgerType))
            {
                _packetHandlersRegistered.Add(obj.LedgerType, new HashSet<IPacketsHandler>());
            }

            _packetHandlersRegistered[obj.LedgerType].Add(obj);
        }
    }
}
