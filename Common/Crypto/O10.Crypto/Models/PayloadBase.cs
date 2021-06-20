using O10.Core.Models;

namespace O10.Crypto.Models
{
    public interface IPayload<out TTransaction> where TTransaction : TransactionBase
    {
        TTransaction? GetTransaction();
    }

    public abstract class PayloadBase<T> : SerializableEntity, IPayload<T> where T: TransactionBase
    {
        public PayloadBase()
        {

        }

        public PayloadBase(T transaction)
        {
            Transaction = transaction;
        }

        public T? Transaction { get; set; }

        public T? GetTransaction()
        {
            return Transaction;
        }
    }
}
