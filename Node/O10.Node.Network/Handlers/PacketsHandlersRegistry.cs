using System.Collections.Generic;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Exceptions;
using O10.Core.Architecture;
using O10.Network.Interfaces;
using System;

namespace O10.Network.Handlers
{
    [RegisterDefaultImplementation(typeof(IPacketsHandlersRegistry), Lifetime = LifetimeManagement.Scoped)]
    public class PacketsHandlersRegistry : IPacketsHandlersRegistry
    {
        private readonly Dictionary<string, ILedgerPacketsHandler> _packetHandlers;
        private readonly Dictionary<LedgerType, HashSet<ILedgerPacketsHandler>> _packetHandlersRegistered;

        public PacketsHandlersRegistry(IEnumerable<ILedgerPacketsHandler> packetHandlers)
        {
            _packetHandlersRegistered = new Dictionary<LedgerType, HashSet<ILedgerPacketsHandler>>();
            _packetHandlers = new Dictionary<string, ILedgerPacketsHandler>();

            if (packetHandlers != null)
            {
                foreach (ILedgerPacketsHandler packetHandler in packetHandlers)
                {
                    if (!_packetHandlers.ContainsKey(packetHandler.Name))
                    {
                        _packetHandlers.Add(packetHandler.Name, packetHandler);
                    }
                }
            }
        }

        public ILedgerPacketsHandler GetInstance(string blocksProcessorName)
        {
            if (!_packetHandlers.ContainsKey(blocksProcessorName))
            {
                throw new BlocksProcessorNotRegisteredException(blocksProcessorName);
            }

            return _packetHandlers[blocksProcessorName];
        }

        public IEnumerable<ILedgerPacketsHandler> GetBulkInstances(LedgerType key)
        {
            if (!_packetHandlersRegistered.ContainsKey(key))
            {
                throw new BlockHandlerNotSupportedException(key);
            }

            return _packetHandlersRegistered[key];
        }

        public void RegisterInstance(ILedgerPacketsHandler obj)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (!_packetHandlersRegistered.ContainsKey(obj.LedgerType))
            {
                _packetHandlersRegistered.Add(obj.LedgerType, new HashSet<ILedgerPacketsHandler>());
            }

            _packetHandlersRegistered[obj.LedgerType].Add(obj);
        }
    }
}
