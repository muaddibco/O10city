﻿using O10.Core.Models;

namespace O10.Crypto.Models
{
    public abstract class PayloadBase<T> : SerializableEntity<PayloadBase<T>> where T: TransactionBase
    {
        public T? Transaction { get; set; }
    }
}