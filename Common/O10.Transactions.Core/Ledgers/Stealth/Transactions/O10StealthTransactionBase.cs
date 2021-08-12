using Newtonsoft.Json;
using O10.Core.Identity;
using O10.Core.Serialization;
using O10.Crypto.Models;
using O10.Transactions.Core.Ledgers.Stealth.Internal;
using System;

namespace O10.Transactions.Core.Ledgers.Stealth.Transactions
{
    public abstract class O10StealthTransactionBase : StealthTransactionBase
    {
        [JsonConverter(typeof(KeyJsonConverter))]
        /// <summary>
        /// P = Hs(r * A) * G + B where A is receiver's Public View Key and B is receiver's Public Spend Key
        /// </summary>
        public IKey? DestinationKey { get; set; }

        [JsonConverter(typeof(KeyJsonConverter))]
        /// <summary>
        /// This is destination key of target that sender wants to authorize with
        /// </summary>
        public IKey? DestinationKey2 { get; set; }

        [JsonConverter(typeof(KeyJsonConverter))]
        /// <summary>
        /// R = r * G. 'r' can be erased after transaction sent unless sender wants to proof he sent funds to particular destination address.
        /// </summary>
        public IKey? TransactionPublicKey { get; set; }

        [JsonConverter(typeof(KeyJsonConverter))]
        /// <summary>
        /// C = x * G + I, where I is elliptic curve point representing asset id
        /// </summary>
        public IKey? AssetCommitment { get; set; }

        public BiometricProof? BiometricProof { get; set; }

        [JsonConverter(typeof(MemoryByteJsonConverter))]
        /// <summary>
        /// Hash of the data with required proofs that was transferred off-chain
        /// </summary>
        public Memory<byte> ProofsHash { get; set; }
    }
}
