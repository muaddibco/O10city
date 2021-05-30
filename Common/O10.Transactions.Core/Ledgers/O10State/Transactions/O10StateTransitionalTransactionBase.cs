using Newtonsoft.Json;
using O10.Core.Identity;
using O10.Core.Serialization;

namespace O10.Transactions.Core.Ledgers.O10State.Transactions
{
    public abstract class O10StateTransitionalTransactionBase : O10StateTransactionBase
    {
        [JsonConverter(typeof(KeyJsonConverter))]
        /// <summary>
        /// P = Hs(r * A) * G + B where A is receiver's Public View Key and B is receiver's Public Spend Key
        /// </summary>
        public IKey? DestinationKey { get; set; }

        [JsonConverter(typeof(KeyJsonConverter))]
        /// <summary>
        /// R = r * G. 'r' can be erased after transaction sent unless sender wants to proof he sent funds to particular destination address.
        /// </summary>
        public IKey? TransactionPublicKey { get; set; }
    }
}
