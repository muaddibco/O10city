using Newtonsoft.Json;
using O10.Core.Cryptography;
using O10.Core.Serialization;

namespace O10.Transactions.Core.Ledgers.Stealth.Internal
{
    public class BiometricProof
    {
        [JsonConverter(typeof(ByteArrayJsonConverter))]
        public byte[] BiometricCommitment { get; set; }
        public SurjectionProof BiometricSurjectionProof { get; set; }

        [JsonConverter(typeof(ByteArrayJsonConverter))]
        public byte[] VerifierPublicKey { get; set; }
        [JsonConverter(typeof(ByteArrayJsonConverter))]
        public byte[] VerifierSignature { get; set; }
    }
}
