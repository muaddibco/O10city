using System;
using O10.Core.Models;

namespace O10.Core.Cryptography
{
    public class StealthSignatureInput
    {
        public StealthSignatureInput(byte[] sourceTransactionKey, byte[][] publicKeys, int keyPosition, Action<StealthSignedPacketBase> updatePacketAction = null)
        {
            SourceTransactionKey = sourceTransactionKey;
            PublicKeys = publicKeys;
            KeyPosition = keyPosition;
            UpdatePacketAction = updatePacketAction;
        }

        public byte[][] PublicKeys { get;  }
        public int KeyPosition { get; }
        public byte[] SourceTransactionKey { get; }

        public Action<StealthSignedPacketBase> UpdatePacketAction { get; }
    }
}
