using O10.Core.Architecture;
using O10.Transactions.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace O10.Gateway.Common.Services.LedgerSynchronizers
{
    [RegisterDefaultImplementation(typeof(ILedgerSynchronizersRepository), Lifetime = LifetimeManagement.Singleton)]
    public class LedgerSynchronizersRepository : ILedgerSynchronizersRepository
    {
        private readonly IEnumerable<ILedgerSynchronizer> _ledgerSynchronizers;

        public LedgerSynchronizersRepository(IEnumerable<ILedgerSynchronizer> ledgerSynchronizers)
        {
            _ledgerSynchronizers = ledgerSynchronizers;
        }

        public ILedgerSynchronizer GetInstance(LedgerType key)
        {
            var ledgerSynchronizer = _ledgerSynchronizers.FirstOrDefault(s => s.LedgerType == key);
            if(ledgerSynchronizer == null)
            {
                throw new ArgumentOutOfRangeException(nameof(key));
            }

            return ledgerSynchronizer;
        }
    }
}
