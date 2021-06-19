﻿using O10.Core.Cryptography;
using O10.Core.Identity;

namespace O10.Client.Common.Dtos.UniversalProofs
{
    public class PayloadBase
    {
        public IKey Commitment { get; set; }
        public SurjectionProof BindingProof { get; set; }
    }
}
