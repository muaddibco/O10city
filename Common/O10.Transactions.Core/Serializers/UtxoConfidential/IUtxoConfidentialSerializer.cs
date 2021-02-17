using O10.Transactions.Core.Ledgers.Stealth;

namespace O10.Transactions.Core.Serializers.Stealth
{
    public interface IStealthSerializer : ISerializer
    {
        void Initialize(StealthBase StealthBase, byte[] prevSecretKey, int prevSecretKeyIndex);
    }
}
