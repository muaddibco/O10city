using Newtonsoft.Json;
using O10.Core.Identity;
using O10.Core.Serialization;

namespace O10.Crypto.Models
{
    public abstract class StealthTransactionBase : TransactionBase
    {
        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey? KeyImage { get; set; }
    }
}
