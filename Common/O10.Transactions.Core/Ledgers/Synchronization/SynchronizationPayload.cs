using Newtonsoft.Json;
using O10.Core.Identity;
using O10.Core.Serialization;
using O10.Crypto.Models;
using O10.Transactions.Core.Ledgers.Synchronization.Transactions;
using System;

namespace O10.Transactions.Core.Ledgers.Synchronization
{
    public class SynchronizationPayload : PayloadBase<SynchronizationTransactionBase>
    {
        public long Height { get; set; }

        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey? HashPrev { get; set; }

        public DateTime ReportedTime { get; set; }
    }
}
