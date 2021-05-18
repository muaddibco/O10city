using O10.Client.Common.Interfaces;
using O10.Core.Architecture;
using O10.Transactions.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace O10.Client.Common.Communication.PacketProducers
{
    [RegisterDefaultImplementation(typeof(IPacketsProducerRepository), Lifetime = LifetimeManagement.Scoped)]
    public class PacketsProducerRepository : IPacketsProducerRepository
    {
        private readonly IEnumerable<IPacketsProducer> _ledgerWriters;

        public PacketsProducerRepository(IEnumerable<IPacketsProducer> ledgerWriters)
        {
            _ledgerWriters = ledgerWriters;
        }

        public IPacketsProducer GetInstance(LedgerType key)
        {
            return _ledgerWriters.FirstOrDefault(l => l.LedgerTypes.Contains(key)) ?? throw new ArgumentOutOfRangeException(nameof(key));
        }
    }
}
