using O10.Client.Common.Interfaces;
using O10.Client.Web.Common.Services;
using O10.Core.Architecture;
using O10.Transactions.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace O10.Client.Common.Communication.LedgerWriters
{
    [RegisterDefaultImplementation(typeof(ILedgerWriterRepository), Lifetime = LifetimeManagement.Scoped)]
    public class LedgerWriterRepository : ILedgerWriterRepository
    {
        private readonly IEnumerable<ILedgerWriter> _ledgerWriters;

        public LedgerWriterRepository(IEnumerable<ILedgerWriter> ledgerWriters)
        {
            _ledgerWriters = ledgerWriters;
        }

        public ILedgerWriter GetInstance(LedgerType key)
        {
            return _ledgerWriters.FirstOrDefault(l => l.LedgerType == key) ?? throw new ArgumentOutOfRangeException(nameof(key));
        }
    }
}
