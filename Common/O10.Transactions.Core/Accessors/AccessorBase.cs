﻿using O10.Crypto.Models;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace O10.Transactions.Core.Accessors
{
    public abstract class AccessorBase : IAccessor
    {
        public abstract LedgerType LedgerType { get; }

        protected abstract IEnumerable<string> GetAccessingKeys();

        protected virtual string ValidateEvidence(EvidenceDescriptor evidence) => string.Empty;

        protected abstract Task<TransactionBase> GetTransactionInner(EvidenceDescriptor accessDescriptor);

        public virtual async Task<T> GetTransaction<T>(EvidenceDescriptor accessDescriptor) where T : TransactionBase
        {
            if (accessDescriptor is null)
            {
                throw new ArgumentNullException(nameof(accessDescriptor));
            }

            //ValidateKeys(accessDescriptor);

            var msg = ValidateEvidence(accessDescriptor);
            if (!string.IsNullOrEmpty(msg))
            {
                throw new AccessorValidationFailedException(msg);
            }

            var packet = await GetTransactionInner(accessDescriptor).ConfigureAwait(false);

            if (packet is T resultPacket)
            {
                return resultPacket;
            }

            throw new InvalidCastException($"Unable to cast obtained packet of the type {packet.GetType().FullName} to the requested type {typeof(T).FullName}");
        }

        private void ValidateKeys(EvidenceDescriptor accessDescriptor)
        {
            List<string> missedKeys = new List<string>();
            foreach (var key in GetAccessingKeys())
            {
                if (!accessDescriptor.Parameters.ContainsKey(key))
                {
                    missedKeys.Add(key);
                }
            }

            if (missedKeys.Count > 0)
            {
                throw new AccessorValidationFailedException($"The provided access descriptor misses the following key(s): {string.Join(',', missedKeys)}");
            }
        }

        public abstract EvidenceDescriptor GetEvidence(TransactionBase transaction);
    }
}
