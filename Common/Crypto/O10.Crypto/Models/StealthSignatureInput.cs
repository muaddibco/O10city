using O10.Core.Identity;
using System;
using System.Collections.Generic;

namespace O10.Crypto.Models
{
    public class StealthSignatureInput
    {
        public StealthSignatureInput(byte[] sourceTransactionKey, IEnumerable<IKey> publicKeys, int keyPosition, Action<StealthTransactionBase> updatePacketAction = null)
        {
            SourceTransactionKey = sourceTransactionKey;
            PublicKeys = publicKeys;
            KeyPosition = keyPosition;
            UpdatePacketAction = updatePacketAction;
        }

        public IEnumerable<IKey> PublicKeys { get; }

        public int KeyPosition { get; }
        
        public byte[] SourceTransactionKey { get; }

        public Action<StealthTransactionBase> UpdatePacketAction { get; }
    }
}
