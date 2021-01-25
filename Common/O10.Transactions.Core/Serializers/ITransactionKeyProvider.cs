using O10.Core.Identity;

namespace O10.Transactions.Core.Serializers
{
    public interface ITransactionKeyProvider
    {
        IKey GetKey();
    }
}
