﻿using O10.Core.Identity;

namespace O10.Transactions.Core.DataModel.Registry
{
    public class WitnessStateKey
    {
        public IKey PublicKey { get; set; }

        public ulong Height { get; set; }
    }
}
