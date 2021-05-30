using O10.Core.Identity;
using System;
using System.Collections.Generic;

namespace O10.Crypto.Models
{
    public class StealthSignatureInput
    {
        public StealthSignatureInput(IKey sourceTransactionKey, IEnumerable<IKey> publicKeys, int keyPosition, Action<StealthTransactionBase>? preSigningAction = null)
        {
            SourceTransactionKey = sourceTransactionKey;
            PublicKeys = publicKeys;
            KeyPosition = keyPosition;
            PreSigningAction = preSigningAction;
        }

        public IEnumerable<IKey> PublicKeys { get; }

        public int KeyPosition { get; }
        
        public IKey SourceTransactionKey { get; }

        public Action<StealthTransactionBase>? PreSigningAction { get; }
    }
}
