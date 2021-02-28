using O10.Core.Cryptography;
using System.Collections.Generic;

namespace O10.Crypto.Models
{
    public class StealthSignature : SignatureBase
    {
        public IEnumerable<RingSignature> Signature { get; set; }
    }
}
