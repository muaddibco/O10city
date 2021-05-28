﻿namespace Chaos.NaCl.Internal.Ed25519Ref10
{
    internal static partial class ScalarOperations
    {
        internal static void sc_negate(byte[] n, byte[] s)
        {
            sc_muladd(n, negone, s, zero);
        }
    }
}
