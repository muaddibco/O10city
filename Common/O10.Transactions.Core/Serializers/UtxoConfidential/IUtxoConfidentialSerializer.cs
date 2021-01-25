using O10.Transactions.Core.DataModel.Stealth;

namespace O10.Transactions.Core.Serializers.Stealth
{
    public interface IStealthSerializer : ISerializer
    {
        void Initialize(StealthBase StealthBase, byte[] prevSecretKey, int prevSecretKeyIndex);
    }
}
