﻿using System.Collections.Generic;
using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers.Registry
{
    public class RegistryShortBlock : RegistryBlockBase, IEqualityComparer<RegistryShortBlock>
    {
        public override ushort PacketType => PacketTypes.Registry_ShortBlock;

        public override ushort Version => 1;

        public WitnessStateKey[] WitnessStateKeys { get; set; }
        public WitnessUtxoKey[] WitnessUtxoKeys { get; set; }

        public bool Equals(RegistryShortBlock x, RegistryShortBlock y)
        {
            if(x != null && y != null)
            {
                return x.SyncHeight == y.SyncHeight && x.Height == y.Height && x.Signer.Equals(y.Signer);
            }

            return x == null && y == null;
        }

        public int GetHashCode(RegistryShortBlock obj)
        {
            int hash = obj.SyncHeight.GetHashCode() ^ obj.Height.GetHashCode() ^ obj.Signer.GetHashCode();

            hash += hash << 13;
            hash ^= hash >> 7;
            hash += hash << 3;
            hash ^= hash >> 17;
            hash += hash << 5;

            return hash;
        }
    }
}