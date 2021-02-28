using O10.Core.Identity;
using System.Collections.Generic;

namespace O10.Crypto.Models
{
    public abstract class StealthTransactionBase : TransactionBase
    {
        public IEnumerable<IKey> Sources { get; set; }
        public IKey KeyImage { get; set; }
    }
}
