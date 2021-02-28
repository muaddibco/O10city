using System;

namespace O10.Crypto.Models
{
    public class StealthSignatureInput
    {
        public StealthSignatureInput(byte[] sourceTransactionKey, byte[][] publicKeys, int keyPosition, Action<StealthTransactionBase> updatePacketAction = null)
        {
            SourceTransactionKey = sourceTransactionKey;
            PublicKeys = publicKeys;
            KeyPosition = keyPosition;
            UpdatePacketAction = updatePacketAction;
        }

        public byte[][] PublicKeys { get; }
        public int KeyPosition { get; }
        public byte[] SourceTransactionKey { get; }

        public Action<StealthTransactionBase> UpdatePacketAction { get; }
    }
}
