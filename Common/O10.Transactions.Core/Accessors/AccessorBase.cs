using O10.Core.Models;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Exceptions;
using System;
using System.Collections.Generic;

namespace O10.Transactions.Core.Accessors
{
    public abstract class AccessorBase : IAccessor
    {
        public abstract PacketType PacketType { get; }

        protected abstract IEnumerable<string> GetAccessingKeys();

        protected virtual string ValidateEvidence(EvidenceDescriptor accessDescriptor) => string.Empty;

        protected abstract PacketBase GetPacketInner(EvidenceDescriptor accessDescriptor);

        public virtual T GetPacket<T>(EvidenceDescriptor accessDescriptor) where T : PacketBase
        {
            if (accessDescriptor is null)
            {
                throw new ArgumentNullException(nameof(accessDescriptor));
            }

            ValidateKeys(accessDescriptor);

            var msg = ValidateEvidence(accessDescriptor);
            if (!string.IsNullOrEmpty(msg))
            {
                throw new AccessorValidationFailedException(msg);
            }

            var packet = GetPacketInner(accessDescriptor);
        
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
    }
}
