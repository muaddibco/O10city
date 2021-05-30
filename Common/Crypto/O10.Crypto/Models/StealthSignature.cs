using O10.Core.Cryptography;
using O10.Core.Identity;
using System.Collections.Generic;

namespace O10.Crypto.Models
{
    public class StealthSignature : SignatureBase
    {
        public IEnumerable<IKey>? Sources { get; set; }
        public IEnumerable<RingSignature>? Signature { get; set; }
    }
}
