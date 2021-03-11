using Newtonsoft.Json;
using O10.Core.Identity;
using O10.Core.Serialization;

namespace O10.Crypto.Models
{
    public abstract class SingleSourceTransactionBase : TransactionBase
    {
        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey? Source { get; set; }
    }
}
