﻿namespace Chaos.NaCl.Internal.Ed25519Ref10
{
    internal static partial class FieldOperations
    {
        internal static void fe_pow22523(out FieldElement result, ref FieldElement z)
        {
            FieldElement t0;
            FieldElement t1;
            FieldElement t2;
            int i;

            /* qhasm: fe z1 */

            /* qhasm: fe z2 */

            /* qhasm: fe z8 */

            /* qhasm: fe z9 */

            /* qhasm: fe z11 */

            /* qhasm: fe z22 */

            /* qhasm: fe z_5_0 */

            /* qhasm: fe z_10_5 */

            /* qhasm: fe z_10_0 */

            /* qhasm: fe z_20_10 */

            /* qhasm: fe z_20_0 */

            /* qhasm: fe z_40_20 */

            /* qhasm: fe z_40_0 */

            /* qhasm: fe z_50_10 */

            /* qhasm: fe z_50_0 */

            /* qhasm: fe z_100_50 */

            /* qhasm: fe z_100_0 */

            /* qhasm: fe z_200_100 */

            /* qhasm: fe z_200_0 */

            /* qhasm: fe z_250_50 */

            /* qhasm: fe z_250_0 */

            /* qhasm: fe z_252_2 */

            /* qhasm: fe z_252_3 */

            /* qhasm: enter pow22523 */

            /* qhasm: z2 = z1^2^1 */
            /* asm 1: fe_sq(>z2=fe#1,<z1=fe#11); for (i = 1;i < 1;++i) fe_sq(>z2=fe#1,>z2=fe#1); */
            /* asm 2: fe_sq(>z2=t0,<z1=z); for (i = 1;i < 1;++i) fe_sq(>z2=t0,>z2=t0); */
            fe_sq(out t0, ref z); //for (i = 1; i < 1; ++i) fe_sq(out t0, ref t0);

            /* qhasm: z8 = z2^2^2 */
            /* asm 1: fe_sq(>z8=fe#2,<z2=fe#1); for (i = 1;i < 2;++i) fe_sq(>z8=fe#2,>z8=fe#2); */
            /* asm 2: fe_sq(>z8=t1,<z2=t0); for (i = 1;i < 2;++i) fe_sq(>z8=t1,>z8=t1); */
            fe_sq(out t1, ref t0); for (i = 1; i < 2; ++i) fe_sq(out t1, ref t1); //TODO: What is this for???

            /* qhasm: z9 = z1*z8 */
            /* asm 1: fe_mul(>z9=fe#2,<z1=fe#11,<z8=fe#2); */
            /* asm 2: fe_mul(>z9=t1,<z1=z,<z8=t1); */
            fe_mul(out t1, ref z, ref t1);

            /* qhasm: z11 = z2*z9 */
            /* asm 1: fe_mul(>z11=fe#1,<z2=fe#1,<z9=fe#2); */
            /* asm 2: fe_mul(>z11=t0,<z2=t0,<z9=t1); */
            fe_mul(out t0, ref t0, ref t1);

            /* qhasm: z22 = z11^2^1 */
            /* asm 1: fe_sq(>z22=fe#1,<z11=fe#1); for (i = 1;i < 1;++i) fe_sq(>z22=fe#1,>z22=fe#1); */
            /* asm 2: fe_sq(>z22=t0,<z11=t0); for (i = 1;i < 1;++i) fe_sq(>z22=t0,>z22=t0); */
            fe_sq(out t0, ref t0); //for (i = 1; i < 1; ++i) fe_sq(out t0, ref  t0);

            /* qhasm: z_5_0 = z9*z22 */
            /* asm 1: fe_mul(>z_5_0=fe#1,<z9=fe#2,<z22=fe#1); */
            /* asm 2: fe_mul(>z_5_0=t0,<z9=t1,<z22=t0); */
            fe_mul(out t0, ref t1, ref t0);

            /* qhasm: z_10_5 = z_5_0^2^5 */
            /* asm 1: fe_sq(>z_10_5=fe#2,<z_5_0=fe#1); for (i = 1;i < 5;++i) fe_sq(>z_10_5=fe#2,>z_10_5=fe#2); */
            /* asm 2: fe_sq(>z_10_5=t1,<z_5_0=t0); for (i = 1;i < 5;++i) fe_sq(>z_10_5=t1,>z_10_5=t1); */
            fe_sq(out t1, ref t0); for (i = 1; i < 5; ++i) fe_sq(out t1, ref t1);

            /* qhasm: z_10_0 = z_10_5*z_5_0 */
            /* asm 1: fe_mul(>z_10_0=fe#1,<z_10_5=fe#2,<z_5_0=fe#1); */
            /* asm 2: fe_mul(>z_10_0=t0,<z_10_5=t1,<z_5_0=t0); */
            fe_mul(out t0, ref t1, ref t0);

            /* qhasm: z_20_10 = z_10_0^2^10 */
            /* asm 1: fe_sq(>z_20_10=fe#2,<z_10_0=fe#1); for (i = 1;i < 10;++i) fe_sq(>z_20_10=fe#2,>z_20_10=fe#2); */
            /* asm 2: fe_sq(>z_20_10=t1,<z_10_0=t0); for (i = 1;i < 10;++i) fe_sq(>z_20_10=t1,>z_20_10=t1); */
            fe_sq(out t1, ref t0); for (i = 1; i < 10; ++i) fe_sq(out t1, ref t1);

            /* qhasm: z_20_0 = z_20_10*z_10_0 */
            /* asm 1: fe_mul(>z_20_0=fe#2,<z_20_10=fe#2,<z_10_0=fe#1); */
            /* asm 2: fe_mul(>z_20_0=t1,<z_20_10=t1,<z_10_0=t0); */
            fe_mul(out t1, ref t1, ref t0);

            /* qhasm: z_40_20 = z_20_0^2^20 */
            /* asm 1: fe_sq(>z_40_20=fe#3,<z_20_0=fe#2); for (i = 1;i < 20;++i) fe_sq(>z_40_20=fe#3,>z_40_20=fe#3); */
            /* asm 2: fe_sq(>z_40_20=t2,<z_20_0=t1); for (i = 1;i < 20;++i) fe_sq(>z_40_20=t2,>z_40_20=t2); */
            fe_sq(out t2, ref t1); for (i = 1; i < 20; ++i) fe_sq(out t2, ref t2);

            /* qhasm: z_40_0 = z_40_20*z_20_0 */
            /* asm 1: fe_mul(>z_40_0=fe#2,<z_40_20=fe#3,<z_20_0=fe#2); */
            /* asm 2: fe_mul(>z_40_0=t1,<z_40_20=t2,<z_20_0=t1); */
            fe_mul(out t1, ref t2, ref t1);

            /* qhasm: z_50_10 = z_40_0^2^10 */
            /* asm 1: fe_sq(>z_50_10=fe#2,<z_40_0=fe#2); for (i = 1;i < 10;++i) fe_sq(>z_50_10=fe#2,>z_50_10=fe#2); */
            /* asm 2: fe_sq(>z_50_10=t1,<z_40_0=t1); for (i = 1;i < 10;++i) fe_sq(>z_50_10=t1,>z_50_10=t1); */
            fe_sq(out t1, ref t1); for (i = 1; i < 10; ++i) fe_sq(out t1, ref t1);

            /* qhasm: z_50_0 = z_50_10*z_10_0 */
            /* asm 1: fe_mul(>z_50_0=fe#1,<z_50_10=fe#2,<z_10_0=fe#1); */
            /* asm 2: fe_mul(>z_50_0=t0,<z_50_10=t1,<z_10_0=t0); */
            fe_mul(out t0, ref t1, ref t0);

            /* qhasm: z_100_50 = z_50_0^2^50 */
            /* asm 1: fe_sq(>z_100_50=fe#2,<z_50_0=fe#1); for (i = 1;i < 50;++i) fe_sq(>z_100_50=fe#2,>z_100_50=fe#2); */
            /* asm 2: fe_sq(>z_100_50=t1,<z_50_0=t0); for (i = 1;i < 50;++i) fe_sq(>z_100_50=t1,>z_100_50=t1); */
            fe_sq(out t1, ref t0); for (i = 1; i < 50; ++i) fe_sq(out t1, ref t1);

            /* qhasm: z_100_0 = z_100_50*z_50_0 */
            /* asm 1: fe_mul(>z_100_0=fe#2,<z_100_50=fe#2,<z_50_0=fe#1); */
            /* asm 2: fe_mul(>z_100_0=t1,<z_100_50=t1,<z_50_0=t0); */
            fe_mul(out t1, ref t1, ref t0);

            /* qhasm: z_200_100 = z_100_0^2^100 */
            /* asm 1: fe_sq(>z_200_100=fe#3,<z_100_0=fe#2); for (i = 1;i < 100;++i) fe_sq(>z_200_100=fe#3,>z_200_100=fe#3); */
            /* asm 2: fe_sq(>z_200_100=t2,<z_100_0=t1); for (i = 1;i < 100;++i) fe_sq(>z_200_100=t2,>z_200_100=t2); */
            fe_sq(out t2, ref t1); for (i = 1; i < 100; ++i) fe_sq(out t2, ref t2);

            /* qhasm: z_200_0 = z_200_100*z_100_0 */
            /* asm 1: fe_mul(>z_200_0=fe#2,<z_200_100=fe#3,<z_100_0=fe#2); */
            /* asm 2: fe_mul(>z_200_0=t1,<z_200_100=t2,<z_100_0=t1); */
            fe_mul(out t1, ref t2, ref t1);

            /* qhasm: z_250_50 = z_200_0^2^50 */
            /* asm 1: fe_sq(>z_250_50=fe#2,<z_200_0=fe#2); for (i = 1;i < 50;++i) fe_sq(>z_250_50=fe#2,>z_250_50=fe#2); */
            /* asm 2: fe_sq(>z_250_50=t1,<z_200_0=t1); for (i = 1;i < 50;++i) fe_sq(>z_250_50=t1,>z_250_50=t1); */
            fe_sq(out t1, ref t1); for (i = 1; i < 50; ++i) fe_sq(out t1, ref t1);

            /* qhasm: z_250_0 = z_250_50*z_50_0 */
            /* asm 1: fe_mul(>z_250_0=fe#1,<z_250_50=fe#2,<z_50_0=fe#1); */
            /* asm 2: fe_mul(>z_250_0=t0,<z_250_50=t1,<z_50_0=t0); */
            fe_mul(out t0, ref t1, ref t0);

            /* qhasm: z_252_2 = z_250_0^2^2 */
            /* asm 1: fe_sq(>z_252_2=fe#1,<z_250_0=fe#1); for (i = 1;i < 2;++i) fe_sq(>z_252_2=fe#1,>z_252_2=fe#1); */
            /* asm 2: fe_sq(>z_252_2=t0,<z_250_0=t0); for (i = 1;i < 2;++i) fe_sq(>z_252_2=t0,>z_252_2=t0); */
            fe_sq(out t0, ref t0); for (i = 1; i < 2; ++i) fe_sq(out t0, ref t0);

            /* qhasm: z_252_3 = z_252_2*z1 */
            /* asm 1: fe_mul(>z_252_3=fe#12,<z_252_2=fe#1,<z1=fe#11); */
            /* asm 2: fe_mul(>z_252_3=out,<z_252_2=t0,<z1=z); */
            fe_mul(out result, ref t0, ref z);

            /* qhasm: return */
        }

        internal static void fe_divpowm1(out FieldElement r, ref FieldElement u, ref FieldElement v)
        {
            fe_sq(out FieldElement v3, ref v);
            fe_mul(out v3, ref v3, ref v); /* v3 = v^3 */
            fe_sq(out FieldElement uv7, ref v3);
            fe_mul(out uv7, ref uv7, ref v);
            fe_mul(out uv7, ref uv7, ref u); /* uv7 = uv^7 */

            /*fe_pow22523(uv7, uv7);*/

            fe_pow22523(out uv7, ref uv7);

            /* t0 = (uv^7)^((q-5)/8) */
            fe_mul(out FieldElement t0, ref uv7, ref v3);
            fe_mul(out r, ref t0, ref u); /* u^(m+1)v^(-(m+1)) */
        }

    }
}
