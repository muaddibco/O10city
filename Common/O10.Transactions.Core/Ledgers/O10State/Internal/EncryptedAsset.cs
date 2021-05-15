using Newtonsoft.Json;
using O10.Core.Cryptography;
using O10.Core.Identity;
using O10.Core.Serialization;

namespace O10.Transactions.Core.Ledgers.O10State.Internal
{
    public class EncryptedAsset
    {
        [JsonConverter(typeof(KeyJsonConverter))]
        /// <summary>
        /// C = x * G + I, where I is elliptic curve point representing assert id
        /// </summary>
        public IKey? AssetCommitment { get; set; }

        /// <summary>
        /// Contains encrypted blinding factor of AssetCommitment: x` = x ^ (r * A). To decrypt receiver makes (R * a) ^ x` = x.
        /// </summary>
        public EcdhTupleCA? EcdhTuple { get; set; }
    }
}
