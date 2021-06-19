using O10.Core.Models;

namespace O10.Crypto.Models
{
    public abstract class TransactionBase : SerializableEntity
    {
        public abstract ushort TransactionType { get; }
    }
}
