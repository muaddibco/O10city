﻿using O10.Core.ExtensionMethods;
using Chaos.NaCl.Internal.Ed25519Ref10;
using HashLib;
using System;
using System.Linq;
using System.Security.Cryptography;
using O10.Core.Cryptography;

namespace O10.Crypto.Experiment.ConfidentialAssets
{
    public static class ConfidentialAssetsHelper
    {
        #region Ring Signatures 

        // Inputs:
        //
        // 1. `msg`: the 32-byte string to be signed.
        // 2. `{P[i]}`: `n` public keys, [points](data.md#public-key) on the elliptic curve.
        // 3. `j`: the index of the designated public key, so that `P[j] == p*G`.
        // 4. `p`: the private key for the public key `P[j]`.
        //
        // Output: `{e0, s[0], ..., s[n-1]}`: the ring signature, `n+1` 32-byte elements.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg">32 byte of message to sign</param>
        /// <param name="pks">collection of public key where secret key of one of the is known to signer</param>
        /// <param name="j">index of public key that its secret key is provided in argument "sk"</param>
        /// <param name="sk">secret key for public key with index j</param>
        /// <returns></returns>
        internal static BorromeanRingSignature CreateRingSignature(byte[] msg, GroupElementP3[] pks, int j, byte[] sk)
        {
            BorromeanRingSignature ringSignature = null;

            if (msg == null)
            {
                throw new ArgumentNullException(nameof(msg));
            }

            if (pks == null)
            {
                throw new ArgumentNullException(nameof(pks));
            }

            if (sk == null)
            {
                throw new ArgumentNullException(nameof(sk));
            }

            if(pks.Length == 0)
            {
                ringSignature = new BorromeanRingSignature();
                return ringSignature;
            }

            ulong n = (ulong)pks.Length;
            ringSignature = new BorromeanRingSignature((int)n);

            // 1. Let `counter = 0`.
            ulong counter = 0;
            while (true)
            {
                byte[][] e0 = new byte[2][]; // second slot is to put non-zero value in a time-constant manner

                // 2. Calculate a sequence of: `n-1` 32-byte random values, 64-byte `nonce` and 1-byte `mask`:
                //    `{r[i], nonce, mask} = SHAKE256(counter || p || msg, 8*(32*(n-1) + 64 + 1))`,
                //    where `p` is encoded in 32 bytes using little-endian convention, and `counter` is encoded as a 64-bit little-endian integer.
                byte[][] r = new byte[n][];

                //ShakeDigest shakeDigest = new ShakeDigest(256);
                //shakeDigest.BlockUpdate(BitConverter.GetBytes(counter), 0, 8);
                //shakeDigest.BlockUpdate(msg, 0, msg.Length);
                //shakeDigest.BlockUpdate(sk, 0, sk.Length);
                //shakeDigest.BlockUpdate(BitConverter.GetBytes((ulong)j), 0, 8);
                //foreach (GroupElementP3 pk in pks)
                //{
                //    shakeDigest.BlockUpdate(EncodePoint(pk), 0, 32);
                //}
                string[][] s_r =
                {
                    new string[]
                    {
                        "805f7b62d2d2c5fc1c5b473774e8aaf6ca57109c594359c5788b20de2d65a451","9943d46be56f3231e0b465748c56836ed261ee45b157f59dd9f359ab6a61d757",""
                    },
                    new string[]
                    {
                        "3212556130fbd7124282398074f179d65ed69356f6c031d94c560c3b2dcff975","89f771d0d185441690110d5b915821baeb483e494d2dd55782822327863b6d1a",""
                    },
                    new string[]
                    {
                        "6355e95f940192b7ccf9bb4c983a1782f767145655743753750f1ba212ad9410","3236c19443686c570a2fbcfd4d62b21a8c362d7a491a6f8106508d8f9aa2baac",""
                    },
                };

                for (int m = 0; m < (int)n - 1; m++)
                {
                    r[m] = GetRandomSeed();//s_r[j][m].HexStringToByteArray();// GetRandomSeed();// new byte[32];
                    //shakeDigest.DoOutput(r[m], 0, 32);
                }

                byte[] nonce = new byte[64];
                byte[] mask = new byte[1];

                //shakeDigest.DoOutput(nonce, 0, 64);
                //shakeDigest.DoOutput(mask, 0, 1);

                // 3. Calculate `k = nonce mod L`, where `nonce` is interpreted as a 64-byte little-endian integer and reduced modulo subgroup order `L`.
                //byte[] k = ReduceScalar64(nonce);
                nonce = GetRandomSeed();
                ScalarOperations.sc_reduce32(nonce);
                byte[] k = nonce;
                //k = j == 0 ? "986cb32f12c9f79b1947c5d67816dee292d6e90c728fd4fe83e43cec38e82603".HexStringToByteArray() : j == 1 ? "3b8a60cce7570d272e0c5e23d52cfc935fd8dbe2854737a7b1e21f3727e2df08".HexStringToByteArray() : "378166d12f716cb5cb7c63e3b1549345ad81a179f1ea33273c724b029effa70b".HexStringToByteArray();
                mask[0] = (byte)(j == 0 ? 253 : j == 1 ? 231 : 104);

                // 4. Calculate the initial e-value, let `i = j+1 mod n`:
                ulong i = ((ulong)j + 1L) % n;

                // 4.1. Calculate `R[i]` as the point `k*G`.
                GroupOperations.ge_scalarmult_base(out GroupElementP3 Ri, k, 0);

                // 4.2. Define `w[j]` as `mask` with lower 4 bits set to zero: `w[j] = mask & 0xf0`.
                byte wj = (byte)(mask[0] & 0xf0);

                // 4.3. Calculate `e[i] = SHA3-512(R[i] || msg || i)` where `i` is encoded as a 64-bit little-endian integer. Interpret `e[i]` as a little-endian integer reduced modulo `L`.
                byte[] Rienc = new byte[32];
                GroupOperations.ge_p3_tobytes(Rienc, 0, ref Ri);

                byte[] ei = ComputeE(Rienc, msg, i);//, wj);
                string s_ei = j == 0 ? "f38c74f0e1655d2eb8aff03e4f35d3fff6781d95dceee8e0652347ad95c62309" : j == 1 ? "41196abe8f5e39d7408da165a5cfc6cc0af79702048581178ab1b95591f3450c" : "f3d5e3b6d181756454ba4e0a72aa1f8bb08907961a4cefe95d3177a2f8a7930e";
                //ei = s_ei.HexStringToByteArray();
                //ei = StringToByteArray("fa0a00c10f5a7ebabaf71f72091dcd9443d6b74a1225ea68cacf5631ba734301");
                if (i == 0)
                {
                    e0[0] = new byte[32];
                    Array.Copy(ei, 0, e0[0], 0, ei.Length);
                }
                else
                {
                    e0[1] = new byte[32];
                    Array.Copy(ei, 0, e0[1], 0, ei.Length);
                }

                // 5. For `step` from `1` to `n-1` (these steps are skipped if `n` equals 1):
                for (ulong step = 1; step < n; step++)
                {
                    // 5.1. Let `i = (j + step) mod n`.
                    i = ((ulong)j + step) % n;

                    // 5.2. Set the forged s-value `s[i] = r[step-1]`
                    ringSignature.S[i] = new byte[32];
                    Array.Copy(r[step - 1], 0, ringSignature.S[i], 0, 32);

                    // 5.3. Define `z[i]` as `s[i]` with the most significant 4 bits set to zero.
                    byte[] z = new byte[32];
                    Array.Copy(ringSignature.S[i], 0, z, 0, 32);
                    z[31] &= 0x0f;

                    // 5.4. Define `w[i]` as a most significant byte of `s[i]` with lower 4 bits set to zero: `w[i] = s[i][31] & 0xf0`.
                    byte wi = (byte)(ringSignature.S[i][31] & 0xf0);

                    // 5.5. Let `i’ = i+1 mod n`.
                    ulong i1 = (i + 1) % n;

                    // 5.6. Calculate `R[i’] = z[i]*G - e[i]*P[i]` and encode it as a 32-byte public key.

                    byte[] nei = NegateScalar(ei);
                    GroupOperations.ge_double_scalarmult_vartime(out GroupElementP2 p2, nei, ref pks[i], z);
                    byte[] Ri1 = new byte[32];
                    GroupOperations.ge_tobytes(Ri1, 0, ref p2);
                    //byte[] Ri1 = ScalarMultSub(pks, ei, z);

                    // 5.7. Calculate `e[i’] = SHA3-512(R[i’] || msg || i’)` where `i’` is encoded as a 64-bit little-endian integer.
                    // Interpret `e[i’]` as a little-endian integer.
                    ei = ComputeE(Ri1, msg, i1);//, wi);

                    switch (j)
                    {
                        case 0:
                            s_ei = step == 1 ? "741940d078c7b6ff8e6775ba068687f1877eda45fd19052560be10febd3eb406" : "d2da942b6957a0406468ddfef63837a30362e2bf147dd6999f26e10a7c66000c";
                            break;
                        case 1:
                            s_ei = step == 1 ? "755c8ddec50106899c0eddede35345708fd5fbf1f72c8a994728112e7e989f0b" : "ec80106da0bbbfe04d857caad199715a4fab49fd16a1888b75c3aef17ac5090b";
                            break;
                        default:
                            s_ei = step == 1 ? "74b8a362b955eda625b5b0dc0826d938011e69ca7a474634c56b9db1fb734405" : "04e1c9f1b04cc665d7927a49be0fb98942956768fa8d6f71f6a867fdd8bc8503";
                            break;
                    }
                    //ei = s_ei.HexStringToByteArray();

                    if (i1 == 0)
                    {
                        e0[0] = new byte[32];
                        Array.Copy(ei, 0, e0[0], 0, ei.Length);
                    }
                    else
                    {
                        e0[1] = new byte[32];
                        Array.Copy(ei, 0, e0[1], 0, ei.Length);
                    }
                }

                // 6. Calculate the non-forged `z[j] = k + p*e[j] mod L` and encode it as a 32-byte little-endian integer.
                byte[] zj = new byte[32];
                ScalarOperations.sc_muladd(zj, sk, ei, k);

                // 7. If `z[j]` is greater than 2<sup>252</sup>–1, then increment the `counter` and try again from the beginning.
                //    The chance of this happening is below 1 in 2<sup>124</sup>.
                if ((zj[31] & 0xf0) != 0)
                {
                    // We won a lottery and will try again with an incremented counter.
                    counter++;
                }
                else
                {
                    // 8. Define `s[j]` as `z[j]` with 4 high bits set to high 4 bits of the `mask`.
                    //zj[31] ^= (byte)(mask[0] & 0xf0); // zj now == sj

                    // Put non-forged s[j] into ringsig
                    Array.Copy(zj, 0, ringSignature.S[j], 0, zj.Length);

                    // Put e[0] inside the ringsig
                    Array.Copy(e0[0], 0, ringSignature.E, 0, e0[0].Length);

                    break;
                }
            }

            // 9. Return the ring signature `{e[0], s[0], ..., s[n-1]}`, total `n+1` 32-byte elements.
            return ringSignature;
        }

        private static byte[] NegateScalar(byte[] s)
        {
            byte[] res = new byte[32];
            ScalarOperations.sc_negate(res, s);

            return res;
        }

        private static byte[] ScalarMultSub(GroupElementP3[] pks, ulong i, byte[] z)
        {
            byte[] Ri1 = new byte[32];
            GroupOperations.ge_scalarmult_base(out GroupElementP3 zG_P3, z, 0);
            GroupOperations.ge_p3_to_cached(out GroupElementCached pke_cached, ref pks[i]);
            GroupOperations.ge_sub(out GroupElementP1P1 rP1P1, ref zG_P3, ref pke_cached);
            GroupOperations.ge_p1p1_to_p3(out GroupElementP3 rP3, ref rP1P1);
            GroupOperations.ge_p3_tobytes(Ri1, 0, ref rP3);
            return Ri1;
        }

        internal static bool VerifyRingSignature(BorromeanRingSignature ringSignature, byte[] msg, GroupElementP3[] pks)
        {
            if(ringSignature.S.Length != pks.Length)
            {
                throw new ArgumentException($"ring size {ringSignature.S.Length} does not equal number of pubkeys {pks.Length}");
            }

            // 1. For each `i` from `0` to `n-1`:
            ulong n = (ulong)pks.Length;
            byte[] e = ringSignature.E;


            for (ulong i = 0; i < n; i++)
            {
                // 1. Define `z[i]` as `s[i]` with the most significant 4 bits set to zero (see note below).
                byte[] z = new byte[32];
                Array.Copy(ringSignature.S[i], 0, z, 0, 32);
                z[31] &= 0x0f;

                // 2. Define `w[i]` as a most significant byte of `s[i]` with lower 4 bits set to zero: `w[i] = s[i][31] & 0xf0`.
                //byte w = (byte)(ringSignature.S[i][31] & 0xf0);

                // 3. Calculate `R[i+1] = z[i]*G - e[i]*P[i]` and encode it as a 32-byte public key.
                byte[] R = new byte[32];
                //GroupOperations.ge_scalarmult_base(out GroupElementP3 zG_P3, z, 0);
                //GroupOperations.ge_scalarmult_p3(out GroupElementP3 pke_P3, e, ref pks[i]);
                //GroupOperations.ge_p3_to_cached(out GroupElementCached pke_cached, ref pke_P3);
                //GroupOperations.ge_sub(out GroupElementP1P1 rP1P1, ref zG_P3, ref pke_cached);
                //GroupOperations.ge_p1p1_to_p3(out GroupElementP3 rP3, ref rP1P1);
                //GroupOperations.ge_p3_tobytes(R, 0, ref rP3);
                byte[] ne = NegateScalar(e);
                
                GroupOperations.ge_double_scalarmult_vartime(out GroupElementP2 p2, ne, ref pks[i], z);
                //byte[] p2bytes = new byte[32];
                GroupOperations.ge_tobytes(R, 0, ref p2);

                // 4. Calculate `e[i+1] = SHA3-512(R[i+1] || msg || i+1)` where `i+1` is encoded as a 64-bit little-endian integer.
                // 5. Interpret `e[i+1]` as a little-endian integer reduced modulo subgroup order `L`.
                e = ComputeE(R, msg, (ulong)((i + 1) % n));//, w);
                //e = s_e[i].HexStringToByteArray();
            }

            return e.Equals32(ringSignature.E);
        }

        #endregion Ring Signatures

        #region Range Proofs

        internal static byte[] CalcAssetRangeProofMsg(GroupElementP3 assetCommitment, GroupElementP3[] candidateAssetCommitments)
        {
            IHash hash = HashFactory.Crypto.CreateSHA256();
            hash.TransformBytes(EncodePoint(assetCommitment));
            foreach (GroupElementP3 candidate in candidateAssetCommitments)
            {
                hash.TransformBytes(EncodePoint(candidate));
            }
            //hash.TransformBytes(encryptedAssetID.AssetId);
            //hash.TransformBytes(encryptedAssetID.BlindingFactor);

            byte[] msg = hash.TransformFinal().GetBytes();

            return msg;
        }

        // Calculate the set of public keys for the ring signature from the set of input asset ID commitments: `P[i] = H’ - H[i]`.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetCommitment"></param>
        /// <param name="candidateAssetCommitments"></param>
        /// <returns>array of 32 byte array representing point on EC</returns>
        internal static GroupElementP3[] CalcAssetRangeProofPubkeys(GroupElementP3 assetCommitment, GroupElementP3[] candidateAssetCommitments)
        {
            GroupElementP3[] pubKeys = new GroupElementP3[candidateAssetCommitments.Length];

            int index = 0;
            foreach (GroupElementP3 candidateAssetCommitment in candidateAssetCommitments)
            {
                GroupElementP3 candidateAssetCommitmentP3 = candidateAssetCommitment;
                GroupOperations.ge_p3_to_cached(out GroupElementCached candidateAssetCommitmentCached, ref candidateAssetCommitmentP3);
                GroupOperations.ge_sub(out GroupElementP1P1 pubKeyP1P1, ref assetCommitment, ref candidateAssetCommitmentCached);

                GroupOperations.ge_p1p1_to_p3(out GroupElementP3 pubKeyP3, ref pubKeyP1P1);
                pubKeys[index++] = pubKeyP3;
            }

            return pubKeys;
        }

        internal static GroupElementP3[] CalcIARPPubKeys(GroupElementP3 assetCommitment, GroupElementP3[] allAssetCommitments, byte[] h, GroupElementP3[] issuanceKeys)
        {
            GroupElementP3[] pubKeys = new GroupElementP3[allAssetCommitments.Length];

            for (int i = 0; i < allAssetCommitments.Length; i++)
            {
                GroupOperations.ge_p3_to_cached(out GroupElementCached elementCached, ref allAssetCommitments[i]);
                GroupOperations.ge_sub(out GroupElementP1P1 p1P1, ref assetCommitment, ref elementCached);
                GroupOperations.ge_p1p1_to_p3(out pubKeys[i], ref p1P1);

                GroupOperations.ge_scalarmult_p3(out GroupElementP3 p3, h, ref issuanceKeys[i]);
                GroupOperations.ge_p3_to_cached(out elementCached, ref p3);
                GroupOperations.ge_add(out p1P1, ref pubKeys[i], ref elementCached);
                GroupOperations.ge_p1p1_to_p3(out pubKeys[i], ref p1P1);
            }

            return pubKeys;
        }

        /// <summary>
        /// Calculates blinding factor that will be used for creating blinded Asset Commitment that will be set in output
        /// </summary>
        /// <param name="cprev">Previous Asset Commitment - one that is used as input for transaction</param>
        /// <param name="assetEncryptionKey">Asset Encryption Key is key that is passed to recipient</param>
        /// <returns></returns>
        public static byte[] ComputeDifferentialBlindingFactor(byte[] cprev, byte[] assetEncryptionKey)
        {
            IHash hash = HashFactory.Crypto.CreateSHA512();
            hash.Initialize();
            hash.TransformBytes(cprev);
            hash.TransformBytes(assetEncryptionKey);

            byte[] hashValue = hash.TransformFinal().GetBytes();
            return ReduceScalar64(hashValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetCommitment">Asset Commitment being sent to recipient</param>
        /// <param name="encryptedAssetID">Encrypted Asset Id being sent to recipient</param>
        /// <param name="candidateAssetCommitments"></param>
        /// <param name="j">index of input commitment among all input commitments that belong to sender and transferred to recipient</param>
        /// <param name="blindingFactor">Blinding factor used for creation Asset Commitment being sent to recipient</param>
        /// <returns></returns>
        internal static SurjectionProof CreateAssetRangeProof(GroupElementP3 assetCommitment, GroupElementP3[] candidateAssetCommitments, int j, byte[] blindingFactor)
        {
            byte[] msg = CalcAssetRangeProofMsg(assetCommitment, candidateAssetCommitments);
            GroupElementP3[] pubkeys = CalcAssetRangeProofPubkeys(assetCommitment, candidateAssetCommitments);

            BorromeanRingSignature ringSignature = CreateRingSignature(msg, pubkeys, j, blindingFactor);

            SurjectionProof assetRangeProof = new SurjectionProof
            {
                H = candidateAssetCommitments,
                Rs = ringSignature
            };

            return assetRangeProof;
        }

        internal static bool VerifyAssetRangeProof(SurjectionProof assetRangeProof, GroupElementP3 assetCommitment)
        {
            byte[] msg = CalcAssetRangeProofMsg(assetCommitment, assetRangeProof.H);

            GroupElementP3[] pubkeys = CalcAssetRangeProofPubkeys(assetCommitment, assetRangeProof.H);

            bool res = VerifyRingSignature(assetRangeProof.Rs, msg, pubkeys);

            return res;
        }

        internal static SurjectionProof CreateIssuanceSurjectionProof(GroupElementP3 assetCommitment, byte[] c, byte[][] assetIds, GroupElementP3[] issuanceKeys, int index, byte[] issuancePrivateKey)
        {
            int n = assetIds.Length;

            if(n == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(assetIds), "list of non-blinded asset IDs is empty");
            }

            if(n != issuanceKeys.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(issuanceKeys), "lists of non-blinded asset IDs and issuance keys are not of the same length");
            }

            if(index < 0 || index >= n)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "designated index is out of bounds");
            }

            GroupElementP3[] nonBlindedAssetCommitments = new GroupElementP3[n];
            for (int i = 0; i < n; i++)
            {
                nonBlindedAssetCommitments[i] = CreateNonblindedAssetCommitment(assetIds[i]);
            }

            IHash hasher = HashFactory.Crypto.SHA3.CreateKeccak512();
            for (int i = 0; i < n; i++)
            {
                byte[] a = new byte[32];
                GroupOperations.ge_p3_tobytes(a, 0, ref nonBlindedAssetCommitments[i]);
                hasher.TransformBytes(a);
            }
            for (int i = 0; i < n; i++)
            {
                byte[] a = new byte[32];
                GroupOperations.ge_p3_tobytes(a, 0, ref issuanceKeys[i]);
                hasher.TransformBytes(a);
            }

            Span<byte> span = new Span<byte>(hasher.TransformFinal().GetBytes());
            byte[] msg = span.Slice(0, 32).ToArray();
            byte[] h = span.Slice(32, 32).ToArray();
            ScalarOperations.sc_reduce32(h);

            GroupElementP3[] pubKeys = CalcIARPPubKeys(assetCommitment, nonBlindedAssetCommitments, h, issuanceKeys);

            byte[] p = new byte[32];
            ScalarOperations.sc_muladd(p, h, issuancePrivateKey, c);

            BorromeanRingSignature borromeanRingSignature = CreateRingSignature(msg, pubKeys, index, p);

            SurjectionProof surjectionProof = new SurjectionProof
            {
                H = issuanceKeys,
                Rs = borromeanRingSignature
            };

            return surjectionProof;
        }

        internal static bool VerifyIssuanceSurjectionProof(SurjectionProof surjectionProof, GroupElementP3 assetCommitment, byte[][] assetIds)
        {
            int n = assetIds.Length;

            if (n == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(assetIds), "list of non-blinded asset IDs is empty");
            }

            if (n != surjectionProof.H.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(surjectionProof), "number of issuance keys does not match length of assetID list");
            }

            GroupElementP3[] nonBlindedAssetCommitments = new GroupElementP3[n];
            for (int i = 0; i < n; i++)
            {
                nonBlindedAssetCommitments[i] = CreateNonblindedAssetCommitment(assetIds[i]);
            }

            IHash hasher = HashFactory.Crypto.SHA3.CreateKeccak512();
            for (int i = 0; i < n; i++)
            {
                byte[] a = new byte[32];
                GroupOperations.ge_p3_tobytes(a, 0, ref nonBlindedAssetCommitments[i]);
                hasher.TransformBytes(a);
            }
            for (int i = 0; i < n; i++)
            {
                byte[] a = new byte[32];
                GroupOperations.ge_p3_tobytes(a, 0, ref surjectionProof.H[i]);
                hasher.TransformBytes(a);
            }

            Span<byte> span = new Span<byte>(hasher.TransformFinal().GetBytes());
            byte[] msg = span.Slice(0, 32).ToArray();
            byte[] h = span.Slice(32, 32).ToArray();
            ScalarOperations.sc_reduce32(h);

            GroupElementP3[] pubKeys = CalcIARPPubKeys(assetCommitment, nonBlindedAssetCommitments, h, surjectionProof.H);

            bool res = VerifyRingSignature(surjectionProof.Rs, msg, pubKeys);

            return res;
        }

        internal static ValueRangeProof CreateValueRangeProof(byte[] assetCommitmentBytes, byte[] valueCommitmentBytes, ulong value, byte[] blindingFactor)
        {
            GroupOperations.ge_frombytes(out GroupElementP3 assetCommitment, assetCommitmentBytes, 0);
            GroupOperations.ge_frombytes(out GroupElementP3 valueCommitment, valueCommitmentBytes, 0);

            byte[] msg = FastHash256(assetCommitmentBytes, valueCommitmentBytes);

            byte n = 32;
            byte[][] b = new byte[n][];
            byte[] bsum = new byte[32];
            for (int i = 0; i < n - 1; i++)
            {
                b[i] = GetRandomSeed();
                //ScalarOperations.sc_reduce32(b[i]);
                //ScalarOperations.sc_muladd(bsum, ScalarOperations.one, b[i], bsum);
                ScalarOperations.sc_add(bsum, bsum, b[i]);
            }

            b[n - 1] = new byte[32];
            //ScalarOperations.sc_muladd(b[n - 1], ScalarOperations.negone, bsum, blindingFactor);
            ScalarOperations.sc_sub(b[n - 1], blindingFactor, bsum);

            GroupElementP3[][] P = new GroupElementP3[n][];
            GroupElementP3[] D = new GroupElementP3[n];
            int[] j = new int[n];
            ulong coefBase = 1;
            for (byte t = 0; t < n; t++)
            {
                ulong digit = value & (ulong)(0x03 << (2 * t));
                ScalarmulBaseAddKeys(out D[t], digit, assetCommitment, b[t]);
                j[t] = (int)(digit >> (2 * t));
                P[t] = CalculateDigitalPoints(coefBase, assetCommitment, D[t]);
                coefBase *= 4;
            }
            string s = msg.ToHexString();
            BorromeanRingSignatureEx borromeanRingSignature = CreateBorromeanRingSignature(msg, P, b, j);

            ValueRangeProof valueRangeProof = new ValueRangeProof
            {
                D = new byte[n][],
                BorromeanRingSignature = borromeanRingSignature
            };

            for (int i = 0; i < n; i++)
            {
                valueRangeProof.D[i] = new byte[32];
                GroupOperations.ge_p3_tobytes(valueRangeProof.D[i], 0, ref D[i]);
            }

            return valueRangeProof;
        }

        internal static bool VerifyValueRangeProof(ValueRangeProof valueRangeProof, byte[] assetCommitmentBytes, byte[] valueCommitmentBytes)
        {
            GroupOperations.ge_frombytes(out GroupElementP3 assetCommitment, assetCommitmentBytes, 0);
            GroupOperations.ge_frombytes(out GroupElementP3 valueCommitment, valueCommitmentBytes, 0);

            byte[] msg = FastHash256(assetCommitmentBytes, valueCommitmentBytes);
            GroupElementP3[] D = new GroupElementP3[valueRangeProof.D.Length];
            for (int i = 0; i < D.Length; i++)
            {
                GroupOperations.ge_frombytes(out D[i], valueRangeProof.D[i], 0);
            }

            GroupOperations.ge_p3_0(out GroupElementP3 Dsum);
            foreach (GroupElementP3 p3 in D)
            {
                GroupElementP3 p3_ = p3;
                GroupOperations.ge_p3_to_cached(out GroupElementCached cached, ref p3_);
                GroupOperations.ge_add(out GroupElementP1P1 p1p1, ref Dsum, ref cached);
                GroupOperations.ge_p1p1_to_p3(out Dsum, ref p1p1);
            }

            //GroupOperations.ge_p3_0(out GroupElementP3 vminH);
            //GroupOperations.ge_p3_to_cached(out GroupElementCached vminHCached, ref vminH);
            //GroupOperations.ge_sub(out GroupElementP1P1 dlastP1P1, ref valueCommitment, ref vminHCached);
            //GroupOperations.ge_p1p1_to_p3(out GroupElementP3 dlastTemp, ref dlastP1P1);
            //GroupOperations.ge_double_scalarmult_vartime(out GroupElementP2 dlastP2, ScalarOperations.one, ref dlastTemp, ScalarOperations.zero);
            //byte[] tmp = new byte[32];
            //GroupOperations.ge_tobytes(tmp, 0, ref dlastP2);
            //GroupOperations.ge_frombytes(out GroupElementP3 dlast, tmp, 0);
            //GroupOperations.ge_p3_to_cached(out GroupElementCached dsumCached, ref Dsum);

            //GroupOperations.ge_sub(out dlastP1P1, ref dlast, ref dsumCached);
            //GroupOperations.ge_p1p1_to_p3(out dlast, ref dlastP1P1);

            int n = 32;
            GroupElementP3[][] P = new GroupElementP3[n][];
            ulong coefBase = 1;
            for (int t = 0; t < n ; t++)
            {
                P[t] = CalculateDigitalPoints(coefBase, assetCommitment, D[t]);
                //P[t] = CalculateDigitalPoints(coefBase, assetCommitment, (t == n - 1) ? dlast : D[t]);
                coefBase *= 4;
            }

            bool res = VerifyBorromeanRingSignature(valueRangeProof.BorromeanRingSignature, msg, P);

            return res;
        }

        #endregion Range Proofs

        #region Commitment Functions

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetId">32-byte code of asset</param>
        /// <returns></returns>
        internal static GroupElementP3 CreateNonblindedAssetCommitment(byte[] assetId)
        {
            if (assetId == null)
            {
                throw new ArgumentNullException(nameof(assetId));
            }

            if(assetId.Length != 32)
            {
                throw new ArgumentOutOfRangeException(nameof(assetId));
            }

            GroupElementP3 assetIdCommitment = new GroupElementP3();
            ulong counter = 0;
            bool succeeded = false;
            do
            {
                byte[] hashValue = FastHash256(assetId, BitConverter.GetBytes(counter++));

                succeeded = GroupOperations.ge_frombytes(out GroupElementP3 p3, hashValue, 0) == 0;

                if (succeeded)
                {
                    GroupOperations.ge_double_scalarmult_vartime(out GroupElementP2 p2_1, ScalarOperations.cofactor, ref p3, ScalarOperations.zero);
                    byte[] s1 = new byte[32];
                    GroupOperations.ge_tobytes(s1, 0, ref p2_1);
                    GroupOperations.ge_frombytes(out assetIdCommitment, s1, 0);


                    GroupOperations.ge_p3_to_p2(out GroupElementP2 p2, ref p3);
                    GroupOperations.ge_mul8(out GroupElementP1P1 p1P1, ref p2);

                    GroupOperations.ge_p1p1_to_p2(out p2, ref p1P1);
                    byte[] s = new byte[32];
                    GroupOperations.ge_tobytes(s, 0, ref p2);
                    GroupOperations.ge_frombytes(out assetIdCommitment, s, 0);
                }
            } while (!succeeded);

            return assetIdCommitment;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetCommitment">either non blinded asset commitment in case when asset is just issued 
        /// or previous blinded asset commitment in case when it was received from another transaction output</param>
        /// <param name="prevBlindingFactor">either <see cref="ScalarOperations.zero"/> in case when asset is just issued or blinding factor from previous transaction</param>
        /// <param name="assetEncryptionKey">32-byte encryption key used for encrypting</param>
        /// <param name="newBlindingFactor">output parameter that will hold 32-byte value of blinding factor used for blinding asset</param>
        /// <returns></returns>
        internal static GroupElementP3 CreateBlindedAssetCommitment(GroupElementP3 assetCommitment, byte[] prevBlindingFactor, byte[] assetEncryptionKey, out byte[] newBlindingFactor)
        {
            newBlindingFactor = ComputeDifferentialBlindingFactor(prevBlindingFactor, assetEncryptionKey);
            GroupElementP3 assetCommitmentBlinded = BlindAssetCommitment(assetCommitment, newBlindingFactor);

            return assetCommitmentBlinded;
        }

        internal static GroupElementP3 BlindAssetCommitment(GroupElementP3 assetCommitment, byte[] blindingFactor)
        {
            GroupOperations.ge_scalarmult_base(out GroupElementP3 p3, blindingFactor, 0);
            GroupOperations.ge_p3_to_cached(out GroupElementCached assetCommitmentCached, ref assetCommitment);
            GroupOperations.ge_add(out GroupElementP1P1 assetCommitmentP1P1, ref p3, ref assetCommitmentCached);
            GroupOperations.ge_p1p1_to_p3(out GroupElementP3 assetCommitmentP3, ref assetCommitmentP1P1);
            return assetCommitmentP3;
        }

        internal static GroupElementP3 CreateNonblindedValueCommitment(GroupElementP3 assetCommitment, ulong value)
        {
            byte[] valueScalar = new byte[32];
            byte[] valueBytes = BitConverter.GetBytes(value);
            Array.Copy(valueBytes, 0, valueScalar, 0, valueBytes.Length);

            GroupOperations.ge_p3_to_cached(out GroupElementCached assetCommitmentCached, ref assetCommitment);
            GroupOperations.ge_scalarmult_base(out GroupElementP3 p3, ScalarOperations.zero, 0);
            GroupOperations.ge_add(out GroupElementP1P1 assetCommitmentP1P1, ref p3, ref assetCommitmentCached);
            GroupOperations.ge_p1p1_to_p3(out GroupElementP3 assetCommitmentP3, ref assetCommitmentP1P1);
            return assetCommitmentP3;
        }

        internal static GroupElementP3 CreateBlindedValueCommitmentFromBlindingFactor(GroupElementP3 assetCommitment, ulong value, byte[] blindingFactor)
        {
            byte[] valueScalar = new byte[32];
            byte[] valueBytes = BitConverter.GetBytes(value);
            Array.Copy(valueBytes, 0, valueScalar, 0, valueBytes.Length);

            GroupOperations.ge_double_scalarmult_vartime(out GroupElementP2 p2, valueScalar, ref assetCommitment, blindingFactor);
            byte[] buf = new byte[32];
            GroupOperations.ge_tobytes(buf, 0, ref p2);
            GroupOperations.ge_frombytes(out GroupElementP3 assetCommitmentP3, buf, 0);

            return assetCommitmentP3;
        }

        internal static GroupElementP3 CreateBlindedValueCommitment(GroupElementP3 assetCommitment, ulong value, byte[] valueEncryptionKey, out byte[] valueBlindingFactor)
        {
            IHash hash = HashFactory.Crypto.CreateSHA512();
            hash.Initialize();
            hash.TransformByte(0xBF);
            hash.TransformBytes(valueEncryptionKey);
            valueBlindingFactor = ReduceScalar64(hash.TransformFinal().GetBytes());

            GroupElementP3 blindedValueCommitment = CreateBlindedValueCommitmentFromBlindingFactor(assetCommitment, value, valueBlindingFactor);

            return blindedValueCommitment;
        }

        internal static byte[] BalanceBlindingFactors(BlindingFactors[] inputs, BlindingFactors[] outputs)
        {
            byte[] fInput = (byte[])ScalarOperations.zero.Clone();

            if (inputs != null)
            {
                foreach (BlindingFactors blindingFactors in inputs)
                {
                    byte[] totalFactor = new byte[32];
                    ScalarOperations.sc_muladd(totalFactor, CryptoHelper.GetScalar(blindingFactors.Value), blindingFactors.AssetBF, blindingFactors.ValueBF);
                    ScalarOperations.sc_add(fInput, fInput, totalFactor);
                }
            }

            byte[] fOutput = (byte[])ScalarOperations.zero.Clone();

            if (outputs != null)
            {
                foreach (BlindingFactors blindingFactors in outputs)
                {
                    byte[] totalFactor = new byte[32];
                    ScalarOperations.sc_muladd(totalFactor, CryptoHelper.GetScalar(blindingFactors.Value), blindingFactors.AssetBF, blindingFactors.ValueBF);
                    ScalarOperations.sc_add(fOutput, fOutput, totalFactor);
                }
            }

            ScalarOperations.sc_sub(fInput, fInput, fOutput);

            return fInput;
        }

        #endregion Commitment Functions

        #region Keys Derivation

        /// <summary>
        /// Intermediate encryption key (IEK) allows
        /// decrypting the asset ID and the value in the output commitment.
        /// It is derived from the REK:
        ///     iek = SHA3-256(0x00 || rek)
        /// </summary>
        /// <param name="recordEncryptionKey"></param>
        /// <returns></returns>
        public static byte[] DeriveIntermediateKey(byte[] recordEncryptionKey)
        {
            byte[] hashVlaue = FastHash256(new byte[] { 0x00 }, recordEncryptionKey);
            return hashVlaue;
        }

        /// <summary>
        /// Asset ID encryption key (AEK) allows decrypting the asset ID
        /// in the output commitment. It is derived from the IEK as follows:
        ///     aek = SHA3-256(0x00 || iek)
        /// </summary>
        /// <param name="intermediateKey"></param>
        /// <returns></returns>
        public static byte[] DeriveAssetKey(byte[] intermediateKey)
        {
            byte[] hashVlaue = FastHash256(new byte[] { 0x00 }, intermediateKey);
            return hashVlaue;
        }

        /// <summary>
        /// Value encryption key (VEK) allows decrypting the amount
        /// in the output commitment. It is derived from the IEK as follows:
        ///     vek = SHA3-256(0x01 || iek)
        /// </summary>
        /// <param name="intermediateKey"></param>
        /// <returns></returns>
        public static byte[] DeriveValueKey(byte[] intermediateKey)
        {
            byte[] hashVlaue = FastHash256(new byte[] { 0x01 }, intermediateKey);
            return hashVlaue;
        }

        #endregion Keys Derivation

        #region Encrypt / Decrypt assetId

        internal static EncryptedAssetID EncryptAssetId(byte[] assetId, GroupElementP3 assetCommitment, byte[] blindingFactor, byte[] assetEncryptionKey)
        {
            byte[] assetCommitmentOriginal = new byte[32];
            GroupOperations.ge_p3_tobytes(assetCommitmentOriginal, 0, ref assetCommitment);
            byte[] ek = FastHash512(assetEncryptionKey, assetCommitmentOriginal);
            Span<byte> span = new Span<byte>(ek);
            EncryptedAssetID r = new EncryptedAssetID
            {
                AssetId = Xor256(assetId, span.Slice(0, 32).ToArray()),
                BlindingFactor = Xor256(blindingFactor, span.Slice(32).ToArray())
            };

            return r;
        }

        internal static byte[] DecryptAssetId(EncryptedAssetID encryptedAssetID, GroupElementP3 assetCommitment, byte[] assetEncryptionKey, out byte[] blindingFactor)
        {
            byte[] assetCommitmentOriginal = new byte[32];
            GroupOperations.ge_p3_tobytes(assetCommitmentOriginal, 0, ref assetCommitment);
            byte[] ek = FastHash512(assetEncryptionKey, assetCommitmentOriginal);
            Span<byte> span = new Span<byte>(ek);

            byte[] assetId = Xor256(encryptedAssetID.AssetId, span.Slice(0, 32).ToArray());
            blindingFactor = Xor256(encryptedAssetID.BlindingFactor, span.Slice(32).ToArray());

            GroupElementP3 nonblindedAssetCommitment = CreateNonblindedAssetCommitment(assetId);
            GroupElementP3 assetCommitmentBlinded = BlindAssetCommitment(nonblindedAssetCommitment, blindingFactor);
            //GroupOperations.ge_p3_to_cached(out GroupElementCached assetIdCached, ref nonblindedAssetCommitment);
            //GroupOperations.ge_scalarmult_base(out GroupElementP3 p3, blindingFactor, 0);
            //GroupOperations.ge_add(out GroupElementP1P1 p1p1, ref p3, ref assetIdCached);
            //GroupOperations.ge_p1p1_to_p3(out p3, ref p1p1);
            byte[] assetCommitmentTemp = new byte[32];
            //GroupOperations.ge_p3_tobytes(assetCommitmentTemp, 0, ref p3);
            GroupOperations.ge_p3_tobytes(assetCommitmentTemp, 0, ref assetCommitmentBlinded);

            if(!assetCommitmentTemp.Equals32(assetCommitmentOriginal))
            {
                throw new Exception("Asset ID decryption failed");
            }

            return assetId;
        }

        #endregion Encrypt / Decrypt assetId

        #region

        internal static BorromeanRingSignatureEx CreateBorromeanRingSignature(byte[] msg, GroupElementP3[][] pubkeys, byte[][] privkeys, int[] indexes)
        {
            int n = pubkeys.Length;

            if(n < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pubkeys), "number of rings cannot be less than 1");
            }

            int m = pubkeys[0].Length;

            if(m < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pubkeys), "number of signatures per ring cannot be less than 1");
            }

            if(privkeys.Length != n)
            {
                throw new ArgumentOutOfRangeException(nameof(privkeys), "number of secret keys must equal number of rings");
            }

            if(indexes.Length != n)
            {
                throw new ArgumentOutOfRangeException(nameof(indexes), "number of secret indexes must equal number of rings");
            }

            //if(payload.Length != n * m)
            //{
            //    throw new ArgumentOutOfRangeException(nameof(payload), "number of random elements must equal n*m (rings*signatures)");
            //}

            BorromeanRingSignatureEx borromeanRingSignature = new BorromeanRingSignatureEx();
            ulong counter = 0;

            while (true)
            {
                byte w;
                byte[][][] s = new byte[n][][];
                byte[][] k = new byte[n][];
                byte[] mask = new byte[n];

                IHash E = HashFactory.Crypto.SHA3.CreateKeccak512();
                E.Initialize();

                byte cnt = (byte)(counter & 0x0f);

                byte[][] r = new byte[n * m][];
                for (int i = 0; i < n * m; i++)
                {
                    r[i] = GetRandomSeed();
                }

                // 5. For `t` from `0` to `n-1` (each ring):
                for (int t = 0; t < n; t++)
                {
                    s[t] = new byte[m][];

                    // 5.1. Let `j = j[t]`
                    int j = indexes[t];

                    // 5.2. Let `x = r[m·t + j]` interpreted as a little-endian integer.
                    byte[] x = r[m * t + j];

                    // 5.3. Define `k[t]` as the lower 252 bits of `x`.
                    k[t] = (byte[])x.Clone();
                    k[t][31] &= 0x0f;

                    // 5.4. Define `mask[t]` as the higher 4 bits of `x`.
                    mask[t] = (byte)(x[31] & 0xf0);

                    // 5.5. Define `w[t,j]` as a byte with lower 4 bits set to zero and higher 4 bits equal `mask[t]`.
                    w = mask[t];

                    // 5.6. Calculate the initial e-value for the ring:

                    // 5.6.1. Let `j’ = j+1 mod m`.
                    int j1 = (j + 1) % m;

                    // 5.6.2. Calculate `R[t,j’]` as the point `k[t]*G` and encode it as a 32-byte [public key](data.md#public-key).
                    GroupOperations.ge_scalarmult_base(out GroupElementP3 R, k[t], 0);

                    // 5.6.3. Calculate `e[t,j’] = SHA3-512(R[t, j’] || msg || t || j’ || w[t,j])` where `t` and `j’` are encoded as 64-bit little-endian integers. Interpret `e[t,j’]` as a little-endian integer reduced modulo `L`.
                    byte[] e = ComputeInnerE(cnt, R, msg, (ulong)t, (ulong)j1, w);

                    // 5.7. If `j ≠ m-1`, then for `i` from `j+1` to `m-1`:
                    for (int i = j + 1; i < m; i++) // note that j+1 can be == m in which case loop is empty as we need it to be.
                    {
                        // 5.7.1. Calculate the forged s-value: `s[t,i] = r[m·t + i]`.
                        s[t][i] = r[m * t + i];
                        // 5.7.2. Define `z[t,i]` as `s[t,i]` with 4 most significant bits set to zero.
                        byte[] z = (byte[])s[t][i].Clone();
                        z[31] &= 0xf;

                        // 5.7.3. Define `w[t,i]` as a most significant byte of `s[t,i]` with lower 4 bits set to zero: `w[t,i] = s[t,i][31] & 0xf0`.
                        w = (byte)(s[t][i][31] & 0xf0);

                        // 5.7.4. Let `i’ = i+1 mod m`.
                        int i1 = (i + 1) % m;

                        GroupOperations.ge_scalarmult_base(out GroupElementP3 zG_P3, z, 0);
                        GroupOperations.ge_p3_to_cached(out GroupElementCached pke_cached, ref pubkeys[t][i]);
                        GroupOperations.ge_sub(out GroupElementP1P1 rP1P1, ref zG_P3, ref pke_cached);
                        GroupOperations.ge_p1p1_to_p3(out GroupElementP3 rP3, ref rP1P1);
                        //ScalarOperations.sc_negate(e, e);
                        //GroupOperations.ge_double_scalarmult_vartime(out GroupElementP2 p2, e, ref pubkeys[t][i], z);
                        //byte[] tmp = new byte[32];
                        //GroupOperations.ge_tobytes(tmp, 0, ref p2);
                        //GroupOperations.ge_frombytes(out GroupElementP3 rP3, tmp, 0);

                        e = ComputeInnerE(cnt, rP3, msg, (ulong)t, (ulong)i1, w);
                    }

                    E.TransformBytes(e);
                }

                // 6.2. Calculate `e0 = SHA3-512(E)`. Interpret `e0` as a little-endian integer reduced modulo `L`.
                byte[] e0hash = E.TransformFinal().GetBytes();
                byte[] e0 = ReduceScalar64(e0hash);

                // 6.3. If `e0` is greater than 2<sup>252</sup>–1, then increment the `counter` and try again from step 2.
                //      The chance of this happening is below 1 in 2<sup>124</sup>.
                if ((e0[31] & 0xf0) != 0)
                {
                    counter++;
                    continue;
                }

                // 7. For `t` from `0` to `n-1` (each ring):
                for (int t = 0; t < n; t++)
                {
                    // 7.1. Let `j = j[t]`
                    int j = indexes[t];

                    // 7.2. Let `e[t,0] = e0`.
                    byte[] e = (byte[])e0.Clone();

                    // 7.3. If `j` is not zero, then for `i` from `0` to `j-1`:
                    for (int i = 0; i < j; i++)
                    {
                        // 7.3.1. Calculate the forged s-value: `s[t,i] = r[m·t + i]`.
                        s[t][i] = r[m * t + i];

                        // 7.3.2. Define `z[t,i]` as `s[t,i]` with 4 most significant bits set to zero.
                        byte[] z1 = (byte[])s[t][i].Clone();
                        z1[31] &= 0x0f;

                        // 7.3.3. Define `w[t,i]` as a most significant byte of `s[t,i]` with lower 4 bits set to zero: `w[t,i] = s[t,i][31] & 0xf0`.
                        w = (byte)(s[t][i][31] & 0xf0);

                        // 7.3.4. Let `i’ = i+1 mod m`.
                        int i1 = (i + 1) % m;

                        // 7.3.5. Calculate point `R[t,i’] = z[t,i]*G - e[t,i]*P[t,i]` and encode it as a 32-byte [public key](data.md#public-key). If `i` is zero, use `e0` in place of `e[t,0]`.
                        GroupOperations.ge_scalarmult_base(out GroupElementP3 zG_P3, z1, 0);
                        GroupOperations.ge_p3_to_cached(out GroupElementCached pke_cached, ref pubkeys[t][i]);
                        GroupOperations.ge_sub(out GroupElementP1P1 rP1P1, ref zG_P3, ref pke_cached);
                        GroupOperations.ge_p1p1_to_p3(out GroupElementP3 rP3, ref rP1P1);
                        //ScalarOperations.sc_negate(e, e);
                        //GroupOperations.ge_double_scalarmult_vartime(out GroupElementP2 p2, e, ref pubkeys[t][i], z1);
                        //byte[] tmp = new byte[32];
                        //GroupOperations.ge_tobytes(tmp, 0, ref p2);
                        //GroupOperations.ge_frombytes(out GroupElementP3 rP3, tmp, 0);

                        // 7.3.6. Calculate `e[t,i’] = SHA3-512(R[t,i’] || msg || t || i’ || w[t,i])` where `t` and `i’` are encoded as 64-bit little-endian integers. Interpret `e[t,i’]` as a little-endian integer reduced modulo subgroup order `L`.
                        e = ComputeInnerE(cnt, rP3, msg, (ulong)t, (ulong)i1, w);
                    }

                    // 7.4. Calculate the non-forged `z[t,j] = k[t] + p[t]*e[t,j] mod L` and encode it as a 32-byte little-endian integer.
                    byte[] z = new byte[32];
                    ScalarOperations.sc_muladd(z, privkeys[t], e, k[t]);

                    // 7.5. If `z[t,j]` is greater than 2<sup>252</sup>–1, then increment the `counter` and try again from step 2.
                    //      The chance of this happening is below 1 in 2<sup>124</sup>.
                    if ((z[31] & 0xf0) != 0)
                    {
                        counter++;
                        continue;
                    }

                    // 7.6. Define `s[t,j]` as `z[t,j]` with 4 high bits set to `mask[t]` bits.
                    s[t][j] = z;
                    s[t][j][31] |= mask[t];
                }

                // 8. Set low 4 bits of `counter` to top 4 bits of `e0`.
                byte counterByte = (byte)(counter & 0xff);
                e0[31] |= (byte)((counterByte << 4) & 0xf0);

                // 9. Return the borromean ring signature: `{e,s[t,j]}`: `n*m+1` 32-byte elements
                borromeanRingSignature.E = e0;
                borromeanRingSignature.S = s;

                break;
            }

            return borromeanRingSignature;
        }

        internal static bool VerifyBorromeanRingSignature(BorromeanRingSignatureEx borromeanRingSignature, byte[] msg, GroupElementP3[][] pubkeys)
        {
            int n = pubkeys.Length;

            if (n < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pubkeys), "number of rings cannot be less than 1");
            }

            int m = pubkeys[0].Length;

            if (m < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pubkeys), "number of signatures per ring cannot be less than 1");
            }

            if(borromeanRingSignature.S.Length != n)
            {
                throw new ArgumentOutOfRangeException(nameof(borromeanRingSignature), $"number of s values {borromeanRingSignature.S.Length} does not match number of rings {n}");
            }

            IHash E = HashFactory.Crypto.SHA3.CreateKeccak512();
            E.Initialize();

            byte cnt = (byte)(borromeanRingSignature.E[31] >> 4);

            byte[] e0 = (byte[])borromeanRingSignature.E.Clone();
            e0[31] &= 0x0f;

            for (int t = 0; t < n; t++)
            {
                if(borromeanRingSignature.S[t].Length != m)
                {
                    throw new ArgumentOutOfRangeException(nameof(borromeanRingSignature), $"number of s values ({borromeanRingSignature.S[t].Length}) in ring {t} does not match m ({m})");
                }

                if(pubkeys[t].Length != m)
                {
                    throw new ArgumentOutOfRangeException(nameof(pubkeys), $"number of pubkeys ({pubkeys[t].Length}) in ring {t} does not match m ({m})");
                }

                byte[] e = (byte[])e0.Clone();

                // 4.2. For `i` from `0` to `m-1`:
                for (int i = 0; i < m; i++)
                {
                    // 4.2.1. Calculate `z[t,i]` as `s[t,i]` with the most significant 4 bits set to zero.
                    byte[] z = (byte[])borromeanRingSignature.S[t][i].Clone();
                    z[31] &= 0x0f;

                    // 4.2.2. Calculate `w[t,i]` as a most significant byte of `s[t,i]` with lower 4 bits set to zero: `w[t,i] = s[t,i][31] & 0xf0`.
                    byte w = (byte)(borromeanRingSignature.S[t][i][31] & 0xf0);

                    // 4.2.3. Let `i’ = i+1 mod m`.
                    int i1 = (i + 1) % m;

                    // 4.2.4. Calculate point `R[t,i’] = z[t,i]·G - e[t,i]·P[t,i]` and encode it as a 32-byte [public key](data.md#public-key). Use `e0` instead of `e[t,0]` in each ring.
                    GroupOperations.ge_scalarmult_base(out GroupElementP3 zG_P3, z, 0);
                    GroupOperations.ge_p3_to_cached(out GroupElementCached pke_cached, ref pubkeys[t][i]);
                    GroupOperations.ge_sub(out GroupElementP1P1 rP1P1, ref zG_P3, ref pke_cached);
                    GroupOperations.ge_p1p1_to_p3(out GroupElementP3 rP3, ref rP1P1);
                    //ScalarOperations.sc_negate(e, e);
                    //GroupOperations.ge_double_scalarmult_vartime(out GroupElementP2 p2, e, ref pubkeys[t][i], z);
                    //byte[] tmp = new byte[32];
                    //GroupOperations.ge_tobytes(tmp, 0, ref p2);
                    //GroupOperations.ge_frombytes(out GroupElementP3 rP3, tmp, 0);

                    // 4.2.5. Calculate `e[t,i’] = SHA3-512(R[t,i’] || msg || t || i’ || w[t,i])` where `t` and `i’` are encoded as 64-bit little-endian integers.
                    // 4.2.6. Interpret `e[t,i’]` as a little-endian integer reduced modulo subgroup order `L`.
                    e = ComputeInnerE(cnt, rP3, msg, (ulong)t, (ulong)i1, w);
                }

                // 4.3. Append `e[t,0]` to `E`: `E = E || e[t,0]`, where `e[t,0]` is encoded as a 32-byte little-endian integer.
                E.TransformBytes(e);
            }

            // 5. Calculate `e’ = SHA3-512(E)` and interpret it as a little-endian integer reduced modulo subgroup order `L`, and then encoded as a little-endian 32-byte integer.
            byte[] e1hash = E.TransformFinal().GetBytes();
            byte[] e1 = ReduceScalar64(e1hash);

            bool res = e1.Equals32(e0);

            return res;
        }

        #endregion

        //TODO: have no idea yet what this is needed for :((((
        /// <summary>
        /// Find point on elliptic curve that is associated with original non blinded assetId
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="assetEncryptionKey"></param>
        /// <param name="assetPoint"></param>
        /// <returns></returns>
        internal static byte[] CreateTransientIssuanceKey(byte[] assetId, byte[] assetEncryptionKey, out GroupElementP3 assetPoint)
        {
            byte[] hash = FastHash512(new byte[] { 0xA1 }, assetId, assetEncryptionKey);
            byte[] scalar = ReduceScalar64(hash);

            GroupOperations.ge_scalarmult_base(out assetPoint, scalar, 0);

            return scalar;
        }

        #region Private Functions

        //TODO: make unsafe with ulongs for better performance
        private static byte[] Xor256(byte[] a, byte[] b)
        {
            byte[] r = new byte[32];

            for (int i = 0; i < 32; i++)
            {
                r[i] = (byte)(a[i] ^ b[i]);
            }

            return r;
        }

        private static byte[] FastHash512(params byte[][] bytes)
        {
            IHash hash = HashFactory.Crypto.SHA3.CreateKeccak512();
            return FastHash(bytes, hash);
        }

        private static byte[] FastHash256(params byte[][] bytes)
        {
            IHash hash = HashFactory.Crypto.CreateSHA256();
            return FastHash(bytes, hash);
        }

        private static byte[] FastHash(byte[][] bytes, IHash hash)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                hash.TransformBytes(bytes[i]);
            }
            byte[] hashValue = hash.TransformFinal().GetBytes();

            return hashValue;
        }

        /// <summary>
        /// aGbB = aG + bB where a, b are scalars, G is the basepoint and B is a point
        /// </summary>
        /// <param name="aGbB"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="bPoint"></param>
        private static void ScalarmulBaseAddKeys2(byte[] aGbB, byte[] a, byte[] b, byte[] bPoint)
        {
            if (aGbB == null)
            {
                throw new ArgumentNullException(nameof(aGbB));
            }

            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            if (bPoint == null)
            {
                throw new ArgumentNullException(nameof(bPoint));
            }

            GroupElementP2 rv;
            GroupElementP3 bPointP3;
            if (GroupOperations.ge_frombytes_negate_vartime(out bPointP3, bPoint, 0) != 0)
            {
                throw new ArgumentException(nameof(bPoint), $"Failed to convert to {nameof(GroupElementP3)}");
            }
            GroupOperations.ge_double_scalarmult_vartime(out rv, a, ref bPointP3, b);
            GroupOperations.ge_tobytes(aGbB, 0, ref rv);
        }

        private static byte[] ComputeE(byte[] r, byte[] msg, ulong i)//, byte w)
        {
            byte[] hash = FastHash512(r, msg, BitConverter.GetBytes(i));//, new byte[] { w });
            byte[] res = ReduceScalar64(hash);

            return res;
        }

        private static byte[] ComputeInnerE(byte cnt, GroupElementP3 p3, byte[] msg, ulong t, ulong i, byte w)
        {
            byte[] p3bytes = new byte[32];
            GroupOperations.ge_p3_tobytes(p3bytes, 0, ref p3);
            byte[] hash = FastHash512(new byte[] { cnt }, p3bytes, msg, BitConverter.GetBytes(t), BitConverter.GetBytes(i), new byte[] { w });

            return ReduceScalar64(hash);
        }

        public static byte[] ReduceScalar64(byte[] hash)
        {
            ScalarOperations.sc_reduce(hash);
            byte[] res = new byte[32];
            Array.Copy(hash, 0, res, 0, 32);
            return res;
        }

        private static byte[] EncodePoint(GroupElementP3 p3)
        {
            byte[] res = new byte[32];
            GroupOperations.ge_p3_tobytes(res, 0, ref p3);

            return res;
        }

        public static byte[] StringToByteArray(string s)
        {
            return Enumerable.Range(0, s.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(s.Substring(x, 2), 16))
                     .ToArray();
        }

        private static byte[] GetRandomSeed()
        {
            byte[] seed = new byte[32];
            RNGCryptoServiceProvider.Create().GetNonZeroBytes(seed);

            return seed;
        }

        //aGbB = aG + bB where a, b are scalars, G is the basepoint and B is a point
        private static void ScalarmulBaseAddKeys(out GroupElementP3 aGbB, ulong b, GroupElementP3 bPoint, byte[] a)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            byte[] bBytes = new byte[32];
            Array.Copy(BitConverter.GetBytes(b), 0, bBytes, 0, 8);

            GroupOperations.ge_double_scalarmult_vartime(out GroupElementP2 rv, bBytes, ref bPoint, a);
            byte[] rvBytes = new byte[32];
            GroupOperations.ge_tobytes(rvBytes, 0, ref rv);
            GroupOperations.ge_frombytes(out aGbB, rvBytes, 0);
        }

        private static GroupElementP3[] CalculateDigitalPoints(ulong coefBase, GroupElementP3 assetCommitment, GroupElementP3 D)
        {
            GroupElementP3[] res = new GroupElementP3[4];
            for (ulong i = 0; i < 4; i++)
            {
                byte[] scalar = new byte[32];
                Array.Copy(BitConverter.GetBytes(i * coefBase), 0, scalar, 0, sizeof(ulong));
                //GroupOperations.ge_scalarmult_p3(out GroupElementP3 p3, scalar, ref assetCommitment);
                //GroupOperations.ge_p3_to_cached(out GroupElementCached cached, ref p3);
                //GroupOperations.ge_sub(out GroupElementP1P1 p1p1, ref D, ref cached);
                //GroupOperations.ge_p1p1_to_p3(out res[i], ref p1p1);
                GroupOperations.ge_double_scalarmult_vartime(out GroupElementP2 p2, scalar, ref assetCommitment, ScalarOperations.zero);
                byte[] tmp = new byte[32];
                GroupOperations.ge_tobytes(tmp, 0, ref p2);
                GroupOperations.ge_frombytes(out GroupElementP3 p3, tmp, 0);
                GroupOperations.ge_p3_to_cached(out GroupElementCached cached, ref p3);
                GroupOperations.ge_sub(out GroupElementP1P1 p1p1, ref D, ref cached);
                GroupOperations.ge_p1p1_to_p3(out res[i], ref p1p1);
            }

            return res;
        }

        #endregion Private Functions

        public static bool IsDestinationKeyMine(byte[] destinationKey, byte[] transactionKey, byte[] secretViewKey, byte[] publicSpendKey)
        {
            GroupOperations.ge_frombytes(out GroupElementP3 transactionKeyP3, transactionKey, 0);
            GroupOperations.ge_scalarmult(out GroupElementP2 fP2, secretViewKey, ref transactionKeyP3);

            byte[] f = new byte[32];
            GroupOperations.ge_tobytes(f, 0, ref fP2);

            f = FastHash256(f);

            GroupOperations.ge_scalarmult_base(out GroupElementP3 p3, f, 0);
            GroupOperations.ge_frombytes(out GroupElementP3 publicSpendKeyP3, publicSpendKey, 0);
            GroupOperations.ge_p3_to_cached(out GroupElementCached cached, ref publicSpendKeyP3);
            GroupOperations.ge_add(out GroupElementP1P1 p1p1, ref p3, ref cached);
            GroupOperations.ge_p1p1_to_p3(out GroupElementP3 destinationKey1P3, ref p1p1);

            byte[] destinationKey1 = new byte[32];
            GroupOperations.ge_p3_tobytes(destinationKey1, 0, ref destinationKey1P3);

            return destinationKey.Equals32(destinationKey1);
        }

        public static byte[] GetDestinationKey(byte[] secretKey, byte[] publicViewKey, byte[] publicSpendKey)
        {
            GroupOperations.ge_frombytes(out GroupElementP3 publicViewKeyP3, publicViewKey, 0);
            GroupOperations.ge_frombytes(out GroupElementP3 publicSpendKeyP3, publicSpendKey, 0);

            GroupOperations.ge_scalarmult(out GroupElementP2 fP3, secretKey, ref publicViewKeyP3);
            byte[] f = new byte[32];
            GroupOperations.ge_tobytes(f, 0, ref fP3);
            byte[] hs = FastHash256(f);

            GroupOperations.ge_scalarmult_base(out GroupElementP3 hsG, hs, 0);
            GroupOperations.ge_p3_to_cached(out GroupElementCached publicSpendKeyCached, ref publicSpendKeyP3);
            GroupOperations.ge_add(out GroupElementP1P1 destinationKeyP1P1, ref hsG, ref publicSpendKeyCached);
            GroupOperations.ge_p1p1_to_p3(out GroupElementP3 destinationKeyP3, ref destinationKeyP1P1);

            byte[] spendKey = new byte[32];
            GroupOperations.ge_p3_tobytes(spendKey, 0, ref destinationKeyP3);

            return spendKey;
        }
        public static byte[] GetTrancationKey(byte[] secretKey)
        {
            GroupOperations.ge_scalarmult_base(out GroupElementP3 p3, secretKey, 0);
            byte[] transactionKey = new byte[32];
            GroupOperations.ge_p3_tobytes(transactionKey, 0, ref p3);

            return transactionKey;
        }
    }
}