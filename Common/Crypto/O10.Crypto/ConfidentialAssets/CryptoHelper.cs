using Chaos.NaCl;
using Chaos.NaCl.Internal.Ed25519Ref10;
using HashLib;
using Isopoh.Cryptography.Argon2;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using O10.Core;
using O10.Core.Cryptography;
using O10.Core.ExtensionMethods;
using O10.Crypto.ExtensionMethods;
using O10.Core.Identity;

namespace O10.Crypto.ConfidentialAssets
{
    public static class CryptoHelper
    {
        public static byte[] I = new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        #region Public Methods

        public static bool IsDestinationKeyMine(byte[] destinationKey, byte[] transactionKey, byte[] secretViewKey, byte[] publicSpendKey)
        {
            GroupOperations.ge_frombytes(out GroupElementP3 transactionKeyP3, transactionKey, 0);
            GroupOperations.ge_scalarmult(out GroupElementP2 fP2, secretViewKey, ref transactionKeyP3);

            byte[] f = new byte[32];
            GroupOperations.ge_tobytes(f, 0, ref fP2);

            f = FastHash256(f);
            ScalarOperations.sc_reduce32(f);

            GroupOperations.ge_scalarmult_base(out GroupElementP3 p3, f, 0);
            GroupOperations.ge_frombytes(out GroupElementP3 publicSpendKeyP3, publicSpendKey, 0);
            GroupOperations.ge_p3_to_cached(out GroupElementCached cached, ref publicSpendKeyP3);
            GroupOperations.ge_add(out GroupElementP1P1 p1p1, ref p3, ref cached);
            GroupOperations.ge_p1p1_to_p3(out GroupElementP3 destinationKey1P3, ref p1p1);

            byte[] destinationKey1 = new byte[32];
            GroupOperations.ge_p3_tobytes(destinationKey1, 0, ref destinationKey1P3);

            return destinationKey.Equals32(destinationKey1);
        }

        /// <summary>
        /// H(sk * pVk) * G + pSk
        /// </summary>
        /// <param name="secretKey"></param>
        /// <param name="publicSpendKey"></param>
        /// <param name="publicViewKey"></param>
        /// <returns></returns>
        public static byte[] GetDestinationKey(byte[] secretKey, byte[] publicSpendKey, byte[] publicViewKey)
        {
            GroupOperations.ge_frombytes(out GroupElementP3 publicViewKeyP3, publicViewKey, 0);
            GroupOperations.ge_frombytes(out GroupElementP3 publicSpendKeyP3, publicSpendKey, 0);

            GroupOperations.ge_scalarmult(out GroupElementP2 fP3, secretKey, ref publicViewKeyP3);
            byte[] f = new byte[32];
            GroupOperations.ge_tobytes(f, 0, ref fP3);
            byte[] hs = FastHash256(f);
            ScalarOperations.sc_reduce32(hs);

            GroupOperations.ge_scalarmult_base(out GroupElementP3 hsG, hs, 0);
            GroupOperations.ge_p3_to_cached(out GroupElementCached publicSpendKeyCached, ref publicSpendKeyP3);
            GroupOperations.ge_add(out GroupElementP1P1 destinationKeyP1P1, ref hsG, ref publicSpendKeyCached);
            GroupOperations.ge_p1p1_to_p3(out GroupElementP3 destinationKeyP3, ref destinationKeyP1P1);

            byte[] spendKey = new byte[32];
            GroupOperations.ge_p3_tobytes(spendKey, 0, ref destinationKeyP3);

            return spendKey;
        }

        public static byte[] GetAssetCommitment(IEnumerable<Memory<byte>> blindingFactors, Memory<byte> assetId)
        {
            GroupElementP3 nonBlindedAssetCommitment = CreateNonblindedAssetCommitment(new List<Memory<byte>> { assetId });
            GroupElementP3 blindedAssetCommitment = BlindAssetCommitment(nonBlindedAssetCommitment, blindingFactors.ToArray());
            byte[] assetCommitment = new byte[32];
            GroupOperations.ge_p3_tobytes(assetCommitment, 0, ref blindedAssetCommitment);

            return assetCommitment;
        }

        public static byte[] GetAssetCommitment(Memory<byte> blindingFactor, params Memory<byte>[] assetIds)
        {
            GroupElementP3 nonBlindedAssetCommitment = CreateNonblindedAssetCommitment(assetIds);
            GroupElementP3 blindedAssetCommitment = BlindAssetCommitment(nonBlindedAssetCommitment, blindingFactor);
            byte[] assetCommitment = new byte[32];
            GroupOperations.ge_p3_tobytes(assetCommitment, 0, ref blindedAssetCommitment);

            return assetCommitment;
        }

        public static byte[] GetNonblindedAssetCommitment(params Memory<byte>[] assetIds)
        {
            GroupElementP3 nonBlindedAssetCommitment = CreateNonblindedAssetCommitment(assetIds);
            byte[] assetCommitment = new byte[32];
            GroupOperations.ge_p3_tobytes(assetCommitment, 0, ref nonBlindedAssetCommitment);
            return assetCommitment;
        }

        public static byte[] BlindAssetCommitment(Memory<byte> assetCommitment, Memory<byte> newBlindingFactor)
        {
            GroupOperations.ge_frombytes(out GroupElementP3 p3, assetCommitment.Span, 0);
            GroupElementP3 p3Blinded = BlindAssetCommitment(p3, newBlindingFactor);

            byte[] blindedBytes = new byte[32];
            GroupOperations.ge_p3_tobytes(blindedBytes, 0, ref p3Blinded);

            return blindedBytes;
        }

        public static byte[] CreateBlindedValueCommitmentFromBlindingFactor(byte[] assetCommitment, ulong value, byte[] blindingFactor)
        {
            GroupOperations.ge_frombytes(out GroupElementP3 assetCommitmentP3, assetCommitment, 0);

            byte[] valueScalar = new byte[32];
            byte[] valueBytes = BitConverter.GetBytes(value);
            Array.Copy(valueBytes, 0, valueScalar, 0, valueBytes.Length);

            GroupOperations.ge_double_scalarmult_vartime(out GroupElementP2 p2, valueScalar, ref assetCommitmentP3, blindingFactor);
            byte[] buf = new byte[32];
            GroupOperations.ge_tobytes(buf, 0, ref p2);

            return buf;
        }

        public static SurjectionProof CreateSurjectionProof(Span<byte> assetCommitment, byte[][] candidateAssetCommitments, int index, byte[] blindingFactor, params byte[][] aux) =>
            CreateSurjectionProof(assetCommitment, candidateAssetCommitments.Select(s => s.AsMemory()).ToArray(), index, blindingFactor, aux);

        public static SurjectionProof CreateSurjectionProof(Span<byte> assetCommitment, Memory<byte>[] candidateAssetCommitments, int index, byte[] blindingFactor, params byte[][] aux)
        {
            if (candidateAssetCommitments is null)
            {
                throw new ArgumentNullException(nameof(candidateAssetCommitments));
            }

            GroupOperations.ge_frombytes(out GroupElementP3 assetCommitmentP3, assetCommitment, 0);

            GroupElementP3[] candidateAssetCommitmentsP3 = TranslatePoints(candidateAssetCommitments);

            BorromeanRingSignature ringSignature = CreateSignatureForSurjectionProof(assetCommitmentP3, candidateAssetCommitmentsP3, index, blindingFactor, aux);

            SurjectionProof assetRangeProof = new SurjectionProof
            {
                AssetCommitments = candidateAssetCommitments.Select(m => m.ToArray()).ToArray(),
                Rs = ringSignature
            };

            return assetRangeProof;
        }

        public static byte[] SubCommitments(byte[] commitmentA, byte[] commitmentB)
        {
            GroupOperations.ge_frombytes(out GroupElementP3 p3A, commitmentA, 0);
            GroupOperations.ge_frombytes(out GroupElementP3 p3B, commitmentB, 0);
            GroupOperations.ge_p3_to_cached(out GroupElementCached cachedB, ref p3B);
            GroupOperations.ge_sub(out GroupElementP1P1 p1p1, ref p3A, ref cachedB);
            GroupOperations.ge_p1p1_to_p3(out GroupElementP3 p3Res, ref p1p1);
            byte[] res = new byte[32];
            GroupOperations.ge_p3_tobytes(res, 0, ref p3Res);

            return res;
        }

        public static byte[] SumCommitments(Span<byte> commitmentA, Span<byte> commitmentB)
        {
            GroupOperations.ge_frombytes(out GroupElementP3 p3A, commitmentA, 0);
            GroupOperations.ge_frombytes(out GroupElementP3 p3B, commitmentB, 0);
            GroupOperations.ge_p3_to_cached(out GroupElementCached cachedB, ref p3B);
            GroupOperations.ge_add(out GroupElementP1P1 p1p1, ref p3A, ref cachedB);
            GroupOperations.ge_p1p1_to_p3(out GroupElementP3 p3Res, ref p1p1);
            byte[] res = new byte[32];
            GroupOperations.ge_p3_tobytes(res, 0, ref p3Res);

            return res;
        }

        public static byte[] AddAssetIds(Span<byte> commitmentA, IEnumerable<Memory<byte>> assetIds)
        {
            if(assetIds == null)
            {
                return commitmentA.ToArray();
            }

            GroupOperations.ge_frombytes(out GroupElementP3 geCommitmentA, commitmentA, 0);
            var nb = CreateNonblindedAssetCommitment(assetIds);
            GroupOperations.ge_p3_to_cached(out GroupElementCached c1, ref nb);
            GroupOperations.ge_add(out GroupElementP1P1 dSumP1, ref geCommitmentA, ref c1);
            GroupOperations.ge_p1p1_to_p3(out GroupElementP3 dSum, ref dSumP1);
            byte[] sum = new byte[32];
            GroupOperations.ge_p3_tobytes(sum, 0, ref dSum);

            return sum;
        }

        public static byte[] GetReducedSharedSecret(Span<byte> sk, Span<byte> pk)
        {
            GroupOperations.ge_frombytes(out GroupElementP3 p3, pk, 0);
            GroupOperations.ge_scalarmult(out GroupElementP2 p2, sk, ref p3);

            byte[] sharedSecret = new byte[32];
            GroupOperations.ge_tobytes(sharedSecret, 0, ref p2);
            ScalarOperations.sc_reduce32(sharedSecret);

            return sharedSecret;
        }

        public static SurjectionProof CreateNewIssuanceSurjectionProof(byte[] assetCommitment, byte[][] assetIds, int index, byte[] blindingFactor)
        {
            GroupOperations.ge_frombytes(out GroupElementP3 assetCommitmentP3, assetCommitment, 0);

            byte[][] issuanceSecretKeys = new byte[assetIds.Length][];
            GroupElementP3[] issuanceP3Keys = new GroupElementP3[assetIds.Length];
            byte[][] issuanceKeys = new byte[assetIds.Length][];

            for (int i = 0; i < assetIds.Length; i++)
            {
                issuanceSecretKeys[i] = GetRandomSeed();
                GroupOperations.ge_scalarmult_base(out issuanceP3Keys[i], issuanceSecretKeys[i], 0);
                issuanceKeys[i] = new byte[32];
                GroupOperations.ge_p3_tobytes(issuanceKeys[i], 0, ref issuanceP3Keys[i]);
            }

            BorromeanRingSignature borromeanRingSignature = CreateIssuanceSurjectionProof(assetCommitmentP3, blindingFactor, assetIds, issuanceP3Keys, index, issuanceSecretKeys[index]);

            SurjectionProof surjectionProof = new SurjectionProof
            {
                AssetCommitments = issuanceKeys,
                Rs = borromeanRingSignature
            };

            return surjectionProof;
        }

        public static bool VerifyIssuanceSurjectionProof(SurjectionProof surjectionProof, Span<byte> assetCommitment, byte[][] assetIds)
        {
            if (GroupOperations.ge_frombytes(out GroupElementP3 assetCommitmentP3, assetCommitment, 0) != 0)
            {
                return false;
            }

            int n = assetIds.Length;

            if (n == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(assetIds), "list of non-blinded asset IDs is empty");
            }

            if (n != surjectionProof.AssetCommitments.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(surjectionProof), "number of issuance keys does not match length of assetID list");
            }

            GroupElementP3[] nonBlindedAssetCommitments = new GroupElementP3[n];
            for (int i = 0; i < n; i++)
            {
                nonBlindedAssetCommitments[i] = CreateNonblindedAssetCommitment(new List<Memory<byte>> { assetIds[i].AsMemory() });
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
                hasher.TransformBytes(surjectionProof.AssetCommitments[i]);
            }

            Span<byte> span = new Span<byte>(hasher.TransformFinal().GetBytes());
            byte[] msg = span.Slice(0, 32).ToArray();
            byte[] h = span.Slice(32, 32).ToArray();
            ScalarOperations.sc_reduce32(h);

            GroupElementP3[] issuanceKeys = new GroupElementP3[surjectionProof.AssetCommitments.Length];
            for (int i = 0; i < surjectionProof.AssetCommitments.Length; i++)
            {
                GroupOperations.ge_frombytes(out issuanceKeys[i], surjectionProof.AssetCommitments[i], 0);
            }

            GroupElementP3[] pubKeys = CalcIARPPubKeys(assetCommitmentP3, nonBlindedAssetCommitments, h, issuanceKeys);

            bool res = VerifyRingSignature(surjectionProof.Rs, msg, pubKeys);

            return res;
        }

        public static bool VerifySurjectionProof(SurjectionProof assetRangeProof, Span<byte> assetCommitment, params byte[][] aux)
        {
            if (assetRangeProof is null)
            {
                throw new ArgumentNullException(nameof(assetRangeProof));
            }

            if (GroupOperations.ge_frombytes(out GroupElementP3 assetCommitmentP3, assetCommitment, 0) != 0)
            {
                return false;
            }

            GroupElementP3[] candidateAssetCommitmentsP3 = TranslatePoints(assetRangeProof.AssetCommitments.Select(s => s.AsMemory()).ToArray());

            byte[] msg = CalcAssetRangeProofMsg(assetCommitmentP3, candidateAssetCommitmentsP3, aux);

            GroupElementP3[] pubkeys = CalcAssetRangeProofPubkeys(assetCommitmentP3, candidateAssetCommitmentsP3);

            bool res = VerifyRingSignature(assetRangeProof.Rs, msg, pubkeys);

            return res;
        }

        public static RangeProof CreateValueRangeProof(byte[] assetCommitmentBytes, byte[] valueCommitmentBytes, ulong value, byte[] blindingFactor)
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
                ScalarOperations.sc_add(bsum, bsum, b[i]);
            }

            b[n - 1] = new byte[32];
            ScalarOperations.sc_sub(b[n - 1], blindingFactor, bsum);

            GroupElementP3[][] P = new GroupElementP3[n][];
            GroupElementP3[] D = new GroupElementP3[n];
            int[] j = new int[n];
            ulong coefBase = 1;
            for (byte t = 0; t < n; t++)
            {
                ulong digit = value & (uint)(0x03 << (2 * t));
                ScalarmulBaseAddKeys(out D[t], b[t], digit.ToByteArray(), assetCommitment);
                j[t] = (int)(digit >> (2 * t));
                P[t] = CalculateDigitalPoints(coefBase, assetCommitment, D[t]);
                coefBase *= 4;
            }

            GroupOperations.ge_p3_to_cached(out GroupElementCached c1, ref D[1]);
            GroupOperations.ge_add(out GroupElementP1P1 dSumP1, ref D[0], ref c1);
            GroupOperations.ge_p1p1_to_p3(out GroupElementP3 dSum, ref dSumP1);
            for (byte t = 2; t < n; t++)
            {
                GroupOperations.ge_p3_to_cached(out GroupElementCached c, ref D[t]);
                GroupOperations.ge_add(out GroupElementP1P1 p, ref dSum, ref c);
                GroupOperations.ge_p1p1_to_p3(out dSum, ref p);
            }

            byte[] dSumBytes = new byte[32];
            GroupOperations.ge_p3_tobytes(dSumBytes, 0, ref dSum);

            GroupOperations.ge_frombytes(out GroupElementP3 p3, dSumBytes, 0);


            BorromeanRingSignatureEx borromeanRingSignature = CreateBorromeanRingSignature(msg, P, b, j);

            RangeProof valueRangeProof = new RangeProof
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

        public static bool VerifyValueRangeProof(RangeProof valueRangeProof, byte[] assetCommitmentBytes, byte[] valueCommitmentBytes)
        {
            GroupOperations.ge_frombytes(out GroupElementP3 assetCommitment, assetCommitmentBytes, 0);
            GroupOperations.ge_frombytes(out GroupElementP3 valueCommitment, valueCommitmentBytes, 0);

            byte[] msg = FastHash256(assetCommitmentBytes, valueCommitmentBytes);
            GroupElementP3[] D = new GroupElementP3[valueRangeProof.D.Length - 1];
            for (int i = 0; i < D.Length; i++)
            {
                //D[i] = LoadFromFile($@"C:\Temp\D{i}.txt");
                GroupOperations.ge_frombytes(out D[i], valueRangeProof.D[i], 0);
            }

            GroupOperations.ge_p3_0(out GroupElementP3 Dsum);
            for (int i = 0; i < D.Length; i++)
            {
                GroupElementP3 d = D[i];
                GroupOperations.ge_p3_to_cached(out GroupElementCached cached, ref d);
                GroupOperations.ge_add(out GroupElementP1P1 p1p1, ref Dsum, ref cached);
                GroupOperations.ge_p1p1_to_p3(out Dsum, ref p1p1);
            }


            //assetCommitment.X = new FieldElement(ParseString("-46188607|-5956786|-32141344|-2404336|-4254295|-23427673|-15466767|-9202666|-25069144|-32959020"));
            //assetCommitment.Y = new FieldElement(ParseString("14627736|3008725|13695403|6915405|22967818|-9189600|27379251|-5954262|16296743|-3193622"));
            //assetCommitment.Z = new FieldElement(ParseString("1|0|0|0|0|0|0|0|0|0"));
            //assetCommitment.T = new FieldElement(ParseString("11804595|-13064553|1418185|-11133329|-33024206|5840579|-33171992|6588227|-30967223|-13218082"));

            //valueCommitment.X = new FieldElement(ParseString("26572090|16223214|3299138|26093402|18257717|9382251|15000629|455204|16712848|33067143"));
            //valueCommitment.Y = new FieldElement(ParseString("23399425|-10771458|20990293|16635329|-6468744|-14033263|-3957929|-10053197|-8679398|-5251169"));
            //valueCommitment.Z = new FieldElement(ParseString("1|0|0|0|0|0|0|0|0|0"));
            //valueCommitment.T = new FieldElement(ParseString("-33148384|-14582137|-28651481|10235383|-32193649|12289341|13899637|-505698|26488280|7932607"));

            GroupOperations.ge_p3_0(out GroupElementP3 vminH);
            GroupOperations.ge_p3_to_cached(out GroupElementCached vminHCached, ref vminH);
            GroupOperations.ge_sub(out GroupElementP1P1 dlastP1P1, ref valueCommitment, ref vminHCached);
            GroupOperations.ge_p1p1_to_p3(out GroupElementP3 dlastTemp, ref dlastP1P1);
            GroupOperations.ge_double_scalarmult_vartime(out GroupElementP2 dlastP2, ScalarOperations.one, ref dlastTemp, ScalarOperations.zero);
            byte[] tmp = new byte[32];
            GroupOperations.ge_tobytes(tmp, 0, ref dlastP2);
            GroupOperations.ge_frombytes(out GroupElementP3 dlast, tmp, 0);
            GroupOperations.ge_p3_to_cached(out GroupElementCached dsumCached, ref Dsum);

            GroupOperations.ge_sub(out dlastP1P1, ref valueCommitment, ref dsumCached);
            GroupOperations.ge_p1p1_to_p3(out dlast, ref dlastP1P1);

            GroupOperations.ge_p3_to_cached(out GroupElementCached d0C, ref D[0]);
            GroupOperations.ge_sub(out GroupElementP1P1 dlast0P1P1, ref valueCommitment, ref d0C);
            GroupOperations.ge_p1p1_to_p3(out dlast, ref dlast0P1P1);

            for (int i = 1; i < D.Length; i++)
            {
                GroupOperations.ge_p3_to_cached(out d0C, ref D[i]);
                GroupOperations.ge_sub(out dlast0P1P1, ref dlast, ref d0C);
                GroupOperations.ge_p1p1_to_p3(out dlast, ref dlast0P1P1);
            }

            int n = 32;
            GroupElementP3[][] P = new GroupElementP3[n][];
            ulong coefBase = 1;
            for (int t = 0; t < n; t++)
            {
                P[t] = CalculateDigitalPoints(coefBase, assetCommitment, (t == n - 1 ? dlast : D[t]));
                //P[t] = CalculateDigitalPoints(coefBase, assetCommitment, D[t]);
                coefBase *= 4;
            }

            //msg = LoadArrayFromFile("c:\\Temp\\msg.txt");
            //BorromeanRingSignatureEx bs = new BorromeanRingSignatureEx
            //{
            //    E = LoadArrayFromFile("c:\\Temp\\e.txt"),
            //    S = new byte[32][][]
            //};

            //for (int i = 0; i < 32; i++)
            //{
            //    bs.S[i] = new byte[4][];

            //    for (int j = 0; j < 4; j++)
            //    {
            //        bs.S[i][j] = LoadArrayFromFile($"c:\\Temp\\s_{i}_{j}.txt");
            //    }
            //}

            bool res = VerifyBorromeanRingSignature(valueRangeProof.BorromeanRingSignature, msg, P);

            return res;
        }

        public static RingSignature[] GenerateRingSignature(byte[] msg, byte[] keyImage, IEnumerable<IKey> publicKeys, byte[] secretKey, int secretKeyIndex) =>
            GenerateRingSignature(msg, keyImage, publicKeys.Select(s => s.Value).ToArray(), secretKey, secretKeyIndex);

        public static RingSignature[] GenerateRingSignature(byte[] msg, byte[] keyImage, Memory<byte>[] publicKeys, byte[] secretKey, int secretKeyIndex)
        {
            RingSignature[] signatures = new RingSignature[publicKeys.Length];

            GroupOperations.ge_frombytes(out GroupElementP3 imageP3, keyImage, 0);

            GroupElementCached[] image_pre = new GroupElementCached[8];
            GroupOperations.ge_dsm_precomp(image_pre, ref imageP3);
            byte[] sum = new byte[32], k = null;
            //buf->h = prefix_hash;

            IHash hasher = HashFactory.Crypto.SHA3.CreateKeccak256();
            hasher.TransformBytes(msg);

            for (int i = 0; i < publicKeys.Length; i++)
            {
                signatures[i] = new RingSignature();

                if (i == secretKeyIndex)
                {
                    k = GetRandomSeed();
                    GroupOperations.ge_scalarmult_base(out GroupElementP3 kG, k, 0);
                    byte[] kGbytes = new byte[32];
                    GroupOperations.ge_p3_tobytes(kGbytes, 0, ref kG);
                    hasher.TransformBytes(kGbytes);
                    GroupElementP3 hash2Point_I = Hash2Point(publicKeys[i].Span);
                    GroupOperations.ge_scalarmult(out GroupElementP2 kH2P, k, ref hash2Point_I);
                    byte[] kH2Pbytes = new byte[32];
                    GroupOperations.ge_tobytes(kH2Pbytes, 0, ref kH2P);
                    hasher.TransformBytes(kH2Pbytes);
                }
                else
                {
                    signatures[i].C = GetRandomSeed();
                    signatures[i].R = GetRandomSeed();
                    GroupOperations.ge_frombytes(out GroupElementP3 tmp3, publicKeys[i].Span, 0);
                    GroupOperations.ge_double_scalarmult_vartime(out GroupElementP2 tmp2, signatures[i].C, ref tmp3, signatures[i].R);
                    byte[] tmp2bytes = new byte[32];
                    GroupOperations.ge_tobytes(tmp2bytes, 0, ref tmp2);
                    hasher.TransformBytes(tmp2bytes);
                    tmp3 = Hash2Point(publicKeys[i].Span);
                    GroupOperations.ge_double_scalarmult_precomp_vartime(out tmp2, signatures[i].R, tmp3, signatures[i].C, image_pre);
                    tmp2bytes = new byte[32];
                    GroupOperations.ge_tobytes(tmp2bytes, 0, ref tmp2);
                    hasher.TransformBytes(tmp2bytes);
                    ScalarOperations.sc_add(sum, sum, signatures[i].C);
                }
            }

            byte[] h = hasher.TransformFinal().GetBytes();
            ScalarOperations.sc_sub(signatures[secretKeyIndex].C, h, sum);
            ScalarOperations.sc_reduce32(signatures[secretKeyIndex].C);
            ScalarOperations.sc_mulsub(signatures[secretKeyIndex].R, signatures[secretKeyIndex].C, secretKey, k);
            ScalarOperations.sc_reduce32(signatures[secretKeyIndex].R);

            return signatures;
        }

        public static bool VerifyRingSignature(byte[] msg, byte[] keyImage, byte[][] pubs, RingSignature[] signatures)
        {
            GroupOperations.ge_frombytes(out GroupElementP3 image_unp, keyImage, 0);

            GroupElementCached[] image_pre = new GroupElementCached[8];
            GroupOperations.ge_dsm_precomp(image_pre, ref image_unp);
            byte[] sum = new byte[32];

            IHash hasher = HashFactory.Crypto.SHA3.CreateKeccak256();
            hasher.TransformBytes(msg);

            for (int i = 0; i < pubs.Length; i++)
            {
                if (ScalarOperations.sc_check(signatures[i].C) != 0 || ScalarOperations.sc_check(signatures[i].R) != 0)
                    return false;

                GroupOperations.ge_frombytes(out GroupElementP3 tmp3, pubs[i], 0);
                GroupOperations.ge_double_scalarmult_vartime(out GroupElementP2 tmp2, signatures[i].C, ref tmp3, signatures[i].R);
                byte[] tmp2bytes = new byte[32];
                GroupOperations.ge_tobytes(tmp2bytes, 0, ref tmp2);
                hasher.TransformBytes(tmp2bytes);
                tmp3 = Hash2Point(pubs[i]);
                GroupOperations.ge_double_scalarmult_precomp_vartime(out tmp2, signatures[i].R, tmp3, signatures[i].C, image_pre);
                tmp2bytes = new byte[32];
                GroupOperations.ge_tobytes(tmp2bytes, 0, ref tmp2);
                hasher.TransformBytes(tmp2bytes);
                ScalarOperations.sc_add(sum, sum, signatures[i].C);
            }

            byte[] h = hasher.TransformFinal().GetBytes();
            ScalarOperations.sc_reduce32(h);
            ScalarOperations.sc_sub(h, h, sum);

            int res = ScalarOperations.sc_isnonzero(h);

            return res == 0;
        }

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
        public static BorromeanRingSignature GenerateBorromeanRingSignature(byte[] msg, byte[][] pks, int j, byte[] sk)
        {
            GroupElementP3[] p3s = new GroupElementP3[pks.Length];

            for (int i = 0; i < pks.Length; i++)
            {
                GroupOperations.ge_frombytes(out p3s[i], pks[i], 0);
            }

            BorromeanRingSignature borromeanRingSignature = CreateRingSignature(msg, p3s, j, sk);

            return borromeanRingSignature;
        }

        public static BorromeanRingSignatureEx GenerateBorromeanRingSignature(byte[] msg, byte[][][] pubkeys, byte[][] privkeys, int[] indicies)
        {
            GroupElementP3[][] pubKeysP3 = new GroupElementP3[pubkeys.Length][];
            for (int i = 0; i < pubkeys.Length; i++)
            {
                pubKeysP3[i] = new GroupElementP3[pubkeys[i].Length];

                for (int j = 0; j < pubkeys[i].Length; j++)
                {
                    GroupOperations.ge_frombytes(out pubKeysP3[i][j], pubkeys[i][j], 0);
                }
            }

            BorromeanRingSignatureEx brs = CreateBorromeanRingSignature(msg, pubKeysP3, privkeys, indicies);

            return brs;
        }

        public static bool VerifyBorromeanRingSignature(BorromeanRingSignatureEx borromeanRingSignature, byte[] msg, byte[][][] pubkeys)
        {
            GroupElementP3[][] pubKeysP3 = new GroupElementP3[pubkeys.Length][];
            for (int i = 0; i < pubkeys.Length; i++)
            {
                pubKeysP3[i] = new GroupElementP3[pubkeys[i].Length];

                for (int j = 0; j < pubkeys[i].Length; j++)
                {
                    GroupOperations.ge_frombytes(out pubKeysP3[i][j], pubkeys[i][j], 0);
                }
            }

            bool res = VerifyBorromeanRingSignature(borromeanRingSignature, msg, pubKeysP3);

            return res;
        }

        public static byte[] GetPublicKey(byte[] secretKey)
        {
            GroupElementP3 p3 = GetPublicKeyP3(secretKey);
            byte[] transactionKey = new byte[32];
            GroupOperations.ge_p3_tobytes(transactionKey, 0, ref p3);

            return transactionKey;
        }

        internal static GroupElementP3 GetPublicKeyP3(byte[] secretKey)
        {
            if (secretKey == null)
            {
                throw new ArgumentNullException(nameof(secretKey));
            }

            if (secretKey.Length != 32)
            {
                throw new ArgumentOutOfRangeException(nameof(secretKey), $"{nameof(secretKey)} must be 32 bytes length");
            }

            GroupOperations.ge_scalarmult_base(out GroupElementP3 p3, secretKey, 0);
            return p3;
        }

        //TODO: seems can be removed - just check that seed is 32 bytes length
        public static byte[] GetPublicKeyFromSeed(byte[] seed)
        {
            return Ed25519.PublicKeyFromSeed(seed);
        }

        public static byte[] GetExpandedPrivateKey(byte[] seed)
        {
            return Ed25519.ExpandedPrivateKeyFromSeed(seed);
        }

        public static byte[] Sign(byte[] msg, byte[] expandedPrivateKey)
        {
            return Ed25519.Sign(msg, expandedPrivateKey);
        }

        public static byte[] GetOTSK(Memory<byte> transactionKey, byte[] secretViewKey, byte[]? secretSpendKey = null)
        {
            GroupOperations.ge_frombytes(out GroupElementP3 transactionKeyP3, transactionKey.Span, 0);
            GroupOperations.ge_scalarmult_p3(out GroupElementP3 p3, secretViewKey, ref transactionKeyP3);

            byte[] p3Bytes = new byte[32];
            GroupOperations.ge_p3_tobytes(p3Bytes, 0, ref p3);
            byte[] p3hash = FastHash256(p3Bytes);
            ScalarOperations.sc_reduce32(p3hash);

            if(secretSpendKey == null)
            {
                return p3hash;
            }

            byte[] otsk = new byte[32];
            ScalarOperations.sc_add(otsk, p3hash, secretSpendKey);

            return otsk;
        }

        public static byte[] GenerateKeyImage(byte[] otsk)
        {
            GroupOperations.ge_scalarmult_base(out GroupElementP3 otpk, otsk, 0);

            byte[] hashed = new byte[32];
            GroupOperations.ge_p3_tobytes(hashed, 0, ref otpk);
            GroupElementP3 p3 = Hash2Point(hashed);
            GroupOperations.ge_scalarmult(out GroupElementP2 p2, otsk, ref p3);
            byte[] image = new byte[32];
            GroupOperations.ge_tobytes(image, 0, ref p2);

            return image;
        }

        public static byte[] CreateEncodedCommitment(byte[] commitment, byte[] secretKey, byte[] receiverPublicKey)
        {
            GroupOperations.ge_frombytes(out GroupElementP3 p3, receiverPublicKey, 0);
            GroupOperations.ge_scalarmult_p3(out GroupElementP3 sharedSecretP3, secretKey, ref p3);

            byte[] sharedSecret = new byte[32];
            GroupOperations.ge_p3_tobytes(sharedSecret, 0, ref sharedSecretP3);

            return EncodeCommitment(commitment, sharedSecret);
        }

        public static EcdhTupleCA CreateEcdhTupleCA(byte[] blindingFactor, byte[] assetId, byte[] secretKey, byte[] receiverViewKey)
        {
            EcdhTupleCA ecdhTupleCA = new EcdhTupleCA
            {
                Mask = (byte[])blindingFactor.Clone(),
                AssetId = (byte[])assetId.Clone()
            };

            GroupOperations.ge_frombytes(out GroupElementP3 p3, receiverViewKey, 0);
            GroupOperations.ge_scalarmult_p3(out GroupElementP3 sharedSecretP3, secretKey, ref p3);

            byte[] sharedSecret = new byte[32];
            GroupOperations.ge_p3_tobytes(sharedSecret, 0, ref sharedSecretP3);

            EcdhEncodeCA(ecdhTupleCA, sharedSecret);

            return ecdhTupleCA;
        }

        public static EcdhTupleProofs CreateEcdhTupleProofs(byte[] blindingFactor, byte[] assetId, byte[] issuer, byte[] payload, byte[] secretKey, byte[] receiverViewKey)
        {
            EcdhTupleProofs ecdhTuple = new EcdhTupleProofs
            {
                Mask = (byte[])blindingFactor.Clone(),
                AssetId = (byte[])assetId.Clone(),
                AssetIssuer = (byte[])issuer.Clone(),
                Payload = (byte[])payload.Clone()
            };

            GroupOperations.ge_frombytes(out GroupElementP3 p3, receiverViewKey, 0);
            GroupOperations.ge_scalarmult_p3(out GroupElementP3 sharedSecretP3, secretKey, ref p3);

            byte[] sharedSecret = new byte[32];
            GroupOperations.ge_p3_tobytes(sharedSecret, 0, ref sharedSecretP3);

            EcdhEncodeProofs(ecdhTuple, sharedSecret);

            return ecdhTuple;
        }

        public static byte[] GetAssetIdFromEcdhTupleCA(EcdhTupleCA ecdhTuple, byte[] transactionKey, byte[] secretKey)
        {
            GroupOperations.ge_frombytes(out GroupElementP3 transactionKeyP3, transactionKey, 0);
            GroupOperations.ge_scalarmult_p3(out GroupElementP3 sharedSecretP3, secretKey, ref transactionKeyP3);

            byte[] sharedSecret = new byte[32];
            GroupOperations.ge_p3_tobytes(sharedSecret, 0, ref sharedSecretP3);
            EcdhTupleCA ecdhTupleTemp = new EcdhTupleCA
            {
                Mask = (byte[])ecdhTuple.Mask.Clone(),
                AssetId = (byte[])ecdhTuple.AssetId.Clone()
            };

            EcdhDecode(ecdhTupleTemp, sharedSecret);

            return ecdhTupleTemp.AssetId;
        }

        public static byte[] DecodeCommitment(byte[] encodedCommitment, byte[] transactionKey, byte[] secretKey)
        {
            byte[] decodedCommitment = (byte[])encodedCommitment.Clone();

            GroupOperations.ge_frombytes(out GroupElementP3 transactionKeyP3, transactionKey, 0);
            GroupOperations.ge_scalarmult_p3(out GroupElementP3 sharedSecretP3, secretKey, ref transactionKeyP3);

            byte[] sharedSecret = new byte[32];
            GroupOperations.ge_p3_tobytes(sharedSecret, 0, ref sharedSecretP3);

            byte[] sharedSecret1 = FastHash256(sharedSecret);
            ScalarOperations.sc_reduce32(sharedSecret1);

            ScalarOperations.sc_sub(decodedCommitment, decodedCommitment, sharedSecret1);

            return decodedCommitment;
        }

        public static void DecodeEcdhTuple(EcdhTupleCA ecdhTuple, byte[] transactionKey, byte[] secretKey, out byte[] blindingFactor, out byte[] assetId)
        {
            GroupOperations.ge_frombytes(out GroupElementP3 transactionKeyP3, transactionKey, 0);
            GroupOperations.ge_scalarmult_p3(out GroupElementP3 sharedSecretP3, secretKey, ref transactionKeyP3);

            byte[] sharedSecret = new byte[32];
            GroupOperations.ge_p3_tobytes(sharedSecret, 0, ref sharedSecretP3);
            EcdhTupleCA ecdhTupleCA = new EcdhTupleCA
            {
                Mask = (byte[])ecdhTuple.Mask.Clone(),
                AssetId = (byte[])ecdhTuple.AssetId.Clone()
            };

            EcdhDecode(ecdhTupleCA, sharedSecret);

            assetId = ecdhTupleCA.AssetId;
            blindingFactor = ecdhTupleCA.Mask;
        }

        public static void DecodeEcdhTuple(EcdhTupleProofs ecdhTuple, byte[] transactionKey, byte[] secretKey, out byte[] blindingFactor, out byte[] assetId, out byte[] issuer, out byte[] payload)
        {
            GroupOperations.ge_frombytes(out GroupElementP3 transactionKeyP3, transactionKey, 0);
            GroupOperations.ge_scalarmult_p3(out GroupElementP3 sharedSecretP3, secretKey, ref transactionKeyP3);

            byte[] sharedSecret = new byte[32];
            GroupOperations.ge_p3_tobytes(sharedSecret, 0, ref sharedSecretP3);
            EcdhTupleProofs ecdhTupleTemp = new EcdhTupleProofs
            {
                Mask = (byte[])ecdhTuple.Mask.Clone(),
                AssetId = (byte[])ecdhTuple.AssetId.Clone(),
                AssetIssuer = (byte[])ecdhTuple.AssetIssuer.Clone(),
                Payload = (byte[])ecdhTuple.Payload.Clone()
            };

            EcdhDecode(ecdhTupleTemp, sharedSecret);

            assetId = ecdhTupleTemp.AssetId;
            blindingFactor = ecdhTupleTemp.Mask;
            issuer = ecdhTupleTemp.AssetIssuer;
            payload = ecdhTupleTemp.Payload;
        }

        //public static void GetAssetIdFromEcdhTupleCA(EcdhTupleCA ecdhTuple, byte[] transactionKey, byte[] secretViewKey, out byte[] blindingFactor, out byte[] assetId)
        //{
        //    GroupOperations.ge_frombytes(out GroupElementP3 transactionKeyP3, transactionKey, 0);
        //    GroupOperations.ge_scalarmult_p3(out GroupElementP3 sharedSecretP3, secretViewKey, ref transactionKeyP3);

        //    byte[] sharedSecret = new byte[32];
        //    GroupOperations.ge_p3_tobytes(sharedSecret, 0, ref sharedSecretP3);
        //    EcdhTupleCA ecdhTupleCA = new EcdhTupleCA
        //    {
        //        Mask = ecdhTuple.Mask,
        //        AssetId = ecdhTuple.AssetId
        //    };

        //    EcdhDecodeCA(ecdhTupleCA, sharedSecret);

        //    blindingFactor = ecdhTuple.Mask;
        //    assetId = ecdhTuple.AssetId;
        //}

        public static byte[] GetRandomSeed()
        {
            byte[] seed = new byte[32];
            byte[] limit = { 0xe3, 0x6a, 0x67, 0x72, 0x8b, 0xce, 0x13, 0x29, 0x8f, 0x30, 0x82, 0x8c, 0x0b, 0xa4, 0x10, 0x39, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xf0 };
            bool less32, isZero;
            do
            {
                RandomNumberGenerator.Create().GetNonZeroBytes(seed);
                isZero = ScalarOperations.sc_isnonzero(seed) == 0;
                less32 = Less32(seed, limit);
            } while (isZero && !less32);

            ScalarOperations.sc_reduce32(seed);

            return seed;
        }

        public static byte[] FastHash512(params Memory<byte>[] bytes)
        {
            IHash hash = HashFactory.Crypto.SHA3.CreateKeccak512();
            return FastHash(bytes, hash);
        }

        public static byte[] FastHash256(params Memory<byte>[] bytes)
        {
            IHash hash = HashFactory.Crypto.CreateSHA256();
            return FastHash(bytes, hash);
        }

        public static byte[] FastHash224(params Memory<byte>[] bytes)
        {
            IHash hash = HashFactory.Crypto.CreateSHA224();
            return FastHash(bytes, hash);
        }

        /// <summary>
        /// Returns bf = bf1 - bf2
        /// </summary>
        /// <param name="blindingFactor1"></param>
        /// <param name="blindingFactor2"></param>
        /// <returns></returns>
        public static byte[] GetDifferentialBlindingFactor(Span<byte> blindingFactor1, Span<byte> blindingFactor2)
        {
            byte[] newBlindingFactor = new byte[32];
            ScalarOperations.sc_sub(newBlindingFactor, blindingFactor1, blindingFactor2);
            ScalarOperations.sc_reduce32(newBlindingFactor);

            return newBlindingFactor;
        }

        public static byte[] NegateBlindingFactor(byte[] blindingFactor)
        {
            byte[] negated = new byte[32];
            ScalarOperations.sc_negate(negated, blindingFactor);

            return negated;
        }

        public static bool IsPointValid(byte[] point)
        {
            return GroupOperations.ge_frombytes(out GroupElementP3 p3, point, 0) == 0;
        }

        public static byte[] SumScalars(params byte[][] scalars)
        {
            byte[] s = new byte[32];

            if (scalars.Length > 0)
            {
                foreach (var scalar in scalars)
                {
                    if (scalar == null || scalar.Length != 32)
                    {
                        throw new IndexOutOfRangeException("All scalars must of 32 bytes length");
                    }
                }

                Array.Copy(scalars[0], 0, s, 0, 32);

                if (scalars.Length > 1)
                {
                    for (int i = 1; i < scalars.Length; i++)
                    {
                        ScalarOperations.sc_add(s, s, scalars[i]);
                    }
                }
            }

            ScalarOperations.sc_reduce32(s);

            return s;
        }

        public static byte[] PasswordHash(string pwd)
        {
            using Argon2 hasher = new Argon2(new Argon2Config
            {
                HashLength = Globals.DEFAULT_HASH_SIZE,
                TimeCost = 1,
                MemoryCost = 16384,
                Type = Argon2Type.HybridAddressing,
                Password = Encoding.ASCII.GetBytes(pwd)
            });

            var secureArray = hasher.Hash();

            byte[] hash = new byte[secureArray.Buffer.Length];
            Array.Copy(secureArray.Buffer, hash, secureArray.Buffer.Length);
            return hash;
        }

        #endregion Public Methods

        #region Internal Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetCommitment">Asset Commitment being sent to recipient</param>
        /// <param name="encryptedAssetID">Encrypted Asset Id being sent to recipient</param>
        /// <param name="candidateAssetCommitments"></param>
        /// <param name="j">index of input commitment among all input commitments that belong to sender and transferred to recipient</param>
        /// <param name="blindingFactor">Blinding factor used for creation Asset Commitment being sent to recipient</param>
        /// <returns></returns>
        internal static BorromeanRingSignature CreateSignatureForSurjectionProof(GroupElementP3 assetCommitment, GroupElementP3[] candidateAssetCommitments, int j, byte[] blindingFactor, params byte[][] aux)
        {
            byte[] msg = CalcAssetRangeProofMsg(assetCommitment, candidateAssetCommitments, aux);
            GroupElementP3[] pubkeys = CalcAssetRangeProofPubkeys(assetCommitment, candidateAssetCommitments);

            BorromeanRingSignature ringSignature = CreateRingSignature(msg, pubkeys, j, blindingFactor);

            return ringSignature;
        }

        internal static BorromeanRingSignature CreateIssuanceSurjectionProof(GroupElementP3 assetCommitment, byte[] c, byte[][] assetIds, GroupElementP3[] issuanceKeys, int index, byte[] issuancePrivateKey)
        {
            int n = assetIds.Length;

            if (n == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(assetIds), "list of non-blinded asset IDs is empty");
            }

            if (n != issuanceKeys.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(issuanceKeys), "lists of non-blinded asset IDs and issuance keys are not of the same length");
            }

            if (index < 0 || index >= n)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "designated index is out of bounds");
            }

            GroupElementP3[] nonBlindedAssetCommitments = new GroupElementP3[n];
            for (int i = 0; i < n; i++)
            {
                nonBlindedAssetCommitments[i] = CreateNonblindedAssetCommitment(new List<Memory<byte>> { assetIds[i].AsMemory() });
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

            return borromeanRingSignature;
        }

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

            BorromeanRingSignature ringSignature;
            if (pks.Length == 0)
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

                for (int m = 0; m < (int)n - 1; m++)
                {
                    r[m] = GetRandomSeed();
                }

                byte[] nonce = new byte[32];
                byte[] mask = new byte[] { GetRandomSeed()[0] };

                // 3. Calculate `k = nonce mod L`, where `nonce` is interpreted as a 64-byte little-endian integer and reduced modulo subgroup order `L`.
                //byte[] k = ReduceScalar64(nonce);
                nonce = GetRandomSeed();
                ScalarOperations.sc_reduce32(nonce);
                byte[] k = nonce;

                // 4. Calculate the initial e-value, let `i = j+1 mod n`:
                ulong i = ((ulong)j + 1L) % n;

                // 4.1. Calculate `R[i]` as the point `k*G`.
                GroupOperations.ge_scalarmult_base(out GroupElementP3 Ri, k, 0);

                // 4.2. Define `w[j]` as `mask` with lower 4 bits set to zero: `w[j] = mask & 0xf0`.
                byte wj = (byte)(mask[0] & 0xf0);

                // 4.3. Calculate `e[i] = SHA3-512(R[i] || msg || i)` where `i` is encoded as a 64-bit little-endian integer. Interpret `e[i]` as a little-endian integer reduced modulo `L`.
                byte[] Rienc = new byte[32];
                GroupOperations.ge_p3_tobytes(Rienc, 0, ref Ri);

                byte[] ei = ComputeE(Rienc, msg, i, wj);

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

                    // 5.7. Calculate `e[i’] = SHA3-512(R[i’] || msg || i’)` where `i’` is encoded as a 64-bit little-endian integer.
                    // Interpret `e[i’]` as a little-endian integer.
                    ei = ComputeE(Ri1, msg, i1, wi);

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
                    zj[31] ^= (byte)(mask[0] & 0xf0); // zj now == sj

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

        public static bool VerifyRingSignature(BorromeanRingSignature ringSignature, byte[] msg, byte[][] pks)
        {
            GroupElementP3[] pubKeys = TranslatePoints(pks);

            return VerifyRingSignature(ringSignature, msg, pubKeys);
        }

        internal static bool VerifyRingSignature(BorromeanRingSignature ringSignature, byte[] msg, GroupElementP3[] pks)
        {
            if (ringSignature.S.Length != pks.Length)
            {
                throw new ArgumentException($"ring size {ringSignature.S.Length} does not equal number of pubkeys {pks.Length}");
            }

            // 1. For each `i` from `0` to `n-1`:
            ulong n = (ulong)pks.Length;
            byte[] e = (byte[])ringSignature.E.Clone();


            for (ulong i = 0; i < n; i++)
            {
                // 1. Define `z[i]` as `s[i]` with the most significant 4 bits set to zero (see note below).
                byte[] z = new byte[32];
                Array.Copy(ringSignature.S[i], 0, z, 0, 32);
                z[31] &= 0x0f;

                // 2. Define `w[i]` as a most significant byte of `s[i]` with lower 4 bits set to zero: `w[i] = s[i][31] & 0xf0`.
                byte w = (byte)(ringSignature.S[i][31] & 0xf0);

                // 3. Calculate `R[i+1] = z[i]*G - e[i]*P[i]` and encode it as a 32-byte public key.
                byte[] R = new byte[32];
                byte[] ne = NegateScalar(e);

                GroupOperations.ge_double_scalarmult_vartime(out GroupElementP2 p2, ne, ref pks[i], z);
                GroupOperations.ge_tobytes(R, 0, ref p2);

                // 4. Calculate `e[i+1] = SHA3-512(R[i+1] || msg || i+1)` where `i+1` is encoded as a 64-bit little-endian integer.
                // 5. Interpret `e[i+1]` as a little-endian integer reduced modulo subgroup order `L`.
                e = ComputeE(R, msg, (ulong)((i + 1) % n), w);
            }

            return e.Equals32(ringSignature.E);
        }

        internal static byte[] CalcAssetRangeProofMsg(GroupElementP3 assetCommitment, GroupElementP3[] candidateAssetCommitments, params byte[][] aux)
        {
            IHash hash = HashFactory.Crypto.CreateSHA256();
            hash.TransformBytes(assetCommitment.ToBytes());

            foreach (GroupElementP3 candidate in candidateAssetCommitments)
            {
                hash.TransformBytes(candidate.ToBytes());
            }

            if (aux != null && aux.Length > 0)
            {
                foreach (byte[] item in aux)
                {
                    hash.TransformBytes(item);
                }
            }

            byte[] msg = hash.TransformFinal().GetBytes();

            return msg;
        }

        internal static BorromeanRingSignatureEx CreateBorromeanRingSignature(byte[] msg, GroupElementP3[][] pubkeys, byte[][] privkeys, int[] indicies)
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

            if (privkeys.Length != n)
            {
                throw new ArgumentOutOfRangeException(nameof(privkeys), "number of secret keys must equal number of rings");
            }

            if (indicies.Length != n)
            {
                throw new ArgumentOutOfRangeException(nameof(indicies), "number of secret indexes must equal number of rings");
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
                    int j = indicies[t];

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
                    int j = indicies[t];

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

            if (borromeanRingSignature.S.Length != n)
            {
                throw new ArgumentOutOfRangeException(nameof(borromeanRingSignature), $"number of s values {borromeanRingSignature.S.Length} does not match number of rings {n}");
            }

            IHash E = HashFactory.Crypto.SHA3.CreateKeccak512();

            byte cnt = (byte)(borromeanRingSignature.E[31] >> 4);

            byte[] e0 = (byte[])borromeanRingSignature.E.Clone();
            e0[31] &= 0x0f;

            for (int t = 0; t < n; t++)
            {
                if (borromeanRingSignature.S[t].Length != m)
                {
                    throw new ArgumentOutOfRangeException(nameof(borromeanRingSignature), $"number of s values ({borromeanRingSignature.S[t].Length}) in ring {t} does not match m ({m})");
                }

                if (pubkeys[t].Length != m)
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

            bool res = ConstTimeEquals(e1, e0);

            return res;
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

        #endregion Internal Methods

        #region Private Methods

        private static byte[] ComputeE(byte[] r, byte[] msg, ulong i, byte w)
        {
            byte[] hash = FastHash512(r, msg, BitConverter.GetBytes(i), new byte[] { w });
            byte[] res = ReduceScalar64(hash);

            return res;
        }

        private static byte[] FastHash(Memory<byte>[] bytes, IHash hash)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                hash.TransformBytes(bytes[i].Span);
            }
            byte[] hashValue = hash.TransformFinal().GetBytes();

            return hashValue;
        }

        public static byte[] ReduceScalar64(byte[] hash)
        {
            ScalarOperations.sc_reduce(hash);
            byte[] res = new byte[32];
            Array.Copy(hash, 0, res, 0, 32);
            return res;
        }

        public static byte[] ReduceScalar32(byte[] hash)
        {
            ScalarOperations.sc_reduce32(hash);
            return hash;
        }

        private static byte[] NegateScalar(byte[] s)
        {
            byte[] res = new byte[32];
            ScalarOperations.sc_negate(res, s);

            return res;
        }

        private static GroupElementP3[] TranslatePoints(Memory<byte>[] points)
        {
            GroupElementP3[] pointsP3 = new GroupElementP3[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                GroupOperations.ge_frombytes(out pointsP3[i], points[i].Span, 0);
            }

            return pointsP3;
        }

        private static GroupElementP3[] TranslatePoints(byte[][] points)
        {
            GroupElementP3[] pointsP3 = new GroupElementP3[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                GroupOperations.ge_frombytes(out pointsP3[i], points[i], 0);
            }

            return pointsP3;
        }

        private static bool Less32(byte[] k0, byte[] k1)
        {
            for (int n = 31; n >= 0; --n)
            {
                if (k0[n] < k1[n])
                    return true;
                if (k0[n] > k1[n])
                    return false;
            }
            return false;
        }

        internal static GroupElementP3 Hash2Point(Span<byte> hashed)
        {
            byte[] hashValue = HashFactory.Crypto.SHA3.CreateKeccak256().ComputeBytes(hashed).GetBytes();
            //byte[] hashValue = HashFactory.Crypto.SHA3.CreateKeccak512().ComputeBytes(hashed).GetBytes();
            ScalarOperations.sc_reduce32(hashValue);
            GroupOperations.ge_fromfe_frombytes_vartime(out GroupElementP2 p2, hashValue, 0);
            GroupOperations.ge_mul8(out GroupElementP1P1 p1p1, ref p2);
            GroupOperations.ge_p1p1_to_p3(out GroupElementP3 p3, ref p1p1);
            return p3;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetId">32-byte code of asset</param>
        /// <returns></returns>
        private static GroupElementP3 CreateNonblindedAssetCommitment(IEnumerable<Memory<byte>> assetIds)
        {
            if (assetIds == null)
                throw new ArgumentNullException(nameof(assetIds));

            GroupElementP3 assetIdCommitmentTotal = new GroupElementP3();
            bool firstPassed = false;

            foreach (var assetId in assetIds)
            {
                if(assetId.IsEmpty)
                {
                    continue;
                }

                if (assetId.Length != 32)
                {
                    throw new ArgumentException("assetId must be of length 32 bytes", nameof(assetIds));
                }

                GroupElementP3 assetIdCommitment = new GroupElementP3();
                ulong counter = 0;
                bool succeeded;
                do
                {
                    byte[] hashValue = FastHash256(assetId, BitConverter.GetBytes(counter++));

                    succeeded = GroupOperations.ge_frombytes(out GroupElementP3 p3, hashValue, 0) == 0;

                    if (succeeded)
                    {
                        //GroupOperations.ge_double_scalarmult_vartime(out GroupElementP2 p2_1, ScalarOperations.cofactor, ref p3, ScalarOperations.zero);
                        //byte[] s1 = new byte[32];
                        //GroupOperations.ge_tobytes(s1, 0, ref p2_1);
                        //GroupOperations.ge_frombytes(out assetIdCommitment, s1, 0);


                        GroupOperations.ge_p3_to_p2(out GroupElementP2 p2, ref p3);
                        GroupOperations.ge_mul8(out GroupElementP1P1 p1P1, ref p2);

                        GroupOperations.ge_p1p1_to_p2(out p2, ref p1P1);
                        byte[] s = new byte[32];
                        GroupOperations.ge_tobytes(s, 0, ref p2);
                        GroupOperations.ge_frombytes(out assetIdCommitment, s, 0);

                        if (firstPassed)
                        {
                            GroupOperations.ge_p3_to_cached(out GroupElementCached cachedComitment, ref assetIdCommitment);
                            GroupOperations.ge_add(out GroupElementP1P1 p1p1Commitment, ref assetIdCommitmentTotal, ref cachedComitment);
                            GroupOperations.ge_p1p1_to_p3(out assetIdCommitmentTotal, ref p1p1Commitment);
                        }
                        else
                        {
                            assetIdCommitmentTotal = assetIdCommitment;
                            firstPassed = true;
                        }
                    }
                } while (!succeeded);
            }



            return assetIdCommitmentTotal;
        }

        private static GroupElementP3 BlindAssetCommitment(GroupElementP3 assetCommitment, params Memory<byte>[] blindingFactors)
        {
            GroupElementP3 p3 = GetBlindingPoint(blindingFactors);
            GroupOperations.ge_p3_to_cached(out GroupElementCached assetCommitmentCached, ref assetCommitment);
            GroupOperations.ge_add(out GroupElementP1P1 assetCommitmentP1P1, ref p3, ref assetCommitmentCached);
            GroupOperations.ge_p1p1_to_p3(out GroupElementP3 assetCommitmentP3, ref assetCommitmentP1P1);
            return assetCommitmentP3;
        }

        private static GroupElementP3 GetBlindingPoint(params Memory<byte>[] blindingFactors)
        {
            if(blindingFactors.Length == 1)
            {
                GroupOperations.ge_scalarmult_base(out GroupElementP3 p3, blindingFactors[0].Span, 0);
                return p3;
            }
            else
            {
                var hash = ReduceScalar32(FastHash256(blindingFactors));
                GroupOperations.ge_scalarmult_base(out GroupElementP3 p3, hash, 0);
                return p3;
            }
        }

        /// <summary>
        /// aGbB = bB + aG where a, b are scalars, G is the basepoint and B is a point
        /// </summary>
        /// <param name="aGbB"></param>
        /// <param name="b"></param>
        /// <param name="bPoint"></param>
        /// <param name="a"></param>
        internal static void ScalarmulBaseAddKeys(out GroupElementP3 aGbB, byte[] a, byte[] b, GroupElementP3 bPoint)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            GroupOperations.ge_double_scalarmult_vartime(out GroupElementP2 rv, b, ref bPoint, a);
            byte[] rvBytes = new byte[32];
            GroupOperations.ge_tobytes(rvBytes, 0, ref rv);
            GroupOperations.ge_frombytes(out aGbB, rvBytes, 0);
        }

        /// <summary>
        /// aGbB = bB + aG where a, b are scalars, G is the basepoint and B is a point
        /// </summary>
        /// <param name="aGbB"></param>
        /// <param name="b"></param>
        /// <param name="bPoint"></param>
        /// <param name="a"></param>
        internal static byte[] ScalarmulBaseAddKeys1(byte[] a, byte[] point)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            GroupOperations.ge_frombytes(out GroupElementP3 bPoint, point, 0);
            GroupOperations.ge_scalarmult_base(out GroupElementP3 p3, a, 0);
            GroupOperations.ge_p3_to_cached(out GroupElementCached r, ref p3);
            GroupOperations.ge_add(out GroupElementP1P1 p1p1, ref bPoint, ref r);
            GroupOperations.ge_p1p1_to_p2(out GroupElementP2 p2, ref p1p1);
            byte[] rvBytes = new byte[32];
            GroupOperations.ge_tobytes(rvBytes, 0, ref p2);

            return rvBytes;
        }

        /// <summary>
        /// aGbB = aG where a is scalar, G is the basepoint
        /// </summary>
        /// <param name="a"></param>
        internal static byte[] ScalarmulBase(byte[] a)
        {
            GroupOperations.ge_scalarmult_base(out GroupElementP3 p3, a, 0);
            byte[] ret = new byte[32];
            GroupOperations.ge_p3_tobytes(ret, 0, ref p3);

            return ret;
        }

        /// <summary>
        /// aGbB = aA where a is scalar, A is a point
        /// </summary>
        /// <param name="a"></param>
        internal static byte[] ScalarmulPoint(byte[] a, byte[] A)
        {
            GroupElementP2 p2 = ScalarmulPoint2(a, A);
            byte[] ret = new byte[32];
            GroupOperations.ge_tobytes(ret, 0, ref p2);

            return ret;
        }

        /// <summary>
        /// aGbB = aA where a is scalar, A is a point
        /// </summary>
        /// <param name="a"></param>
        internal static GroupElementP2 ScalarmulPoint2(byte[] a, byte[] A)
        {
            GroupOperations.ge_frombytes(out GroupElementP3 p3, A, 0);
            GroupOperations.ge_scalarmult(out GroupElementP2 p2, a, ref p3);

            return p2;
        }

        /// <summary>
        /// aGbB = aG + bB where a, b are scalars, G is the basepoint and B is a point
        /// </summary>
        /// <param name="aGbB"></param>
        /// <param name="b"></param>
        /// <param name="bPoint"></param>
        /// <param name="a"></param>
        internal static byte[] ScalarmulBaseAddKeys(byte[] a, byte[] b, byte[] bPoint)
        {
            GroupOperations.ge_frombytes(out GroupElementP3 p3, bPoint, 0);
            ScalarmulBaseAddKeys(out GroupElementP3 aGbB, a, b, p3);
            byte[] ret = new byte[32];
            GroupOperations.ge_p3_tobytes(ret, 0, ref aGbB);

            return ret;
        }

        /// <summary>
        /// aAbB = a*A + b*B where a, b are scalars, A, B are curve points
        /// B must be input after applying "precomp"
        /// </summary>
        /// <param name="aAbB"></param>
        /// <param name="a"></param>
        /// <param name="A"></param>
        /// <param name="b"></param>
        /// <param name="B"></param>
        internal static void ScalarmulBaseAddKeys2(out GroupElementP2 aAbB, byte[] a, GroupElementP3 A, byte[] b, GroupElementCached[] B)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            if (B == null)
            {
                throw new ArgumentNullException(nameof(B));
            }

            GroupOperations.ge_double_scalarmult_precomp_vartime(out aAbB, a, A, b, B);
        }

        /// <summary>
        /// aAbB = a*A + b*B where a, b are scalars, A, B are curve points
        /// B must be input after applying "precomp"
        /// </summary>
        /// <param name="aAbB"></param>
        /// <param name="a"></param>
        /// <param name="A"></param>
        /// <param name="b"></param>
        /// <param name="B"></param>
        internal static byte[] ScalarmulBaseAddKeys2(byte[] a, GroupElementP3 A, byte[] b, GroupElementCached[] B)
        {
            ScalarmulBaseAddKeys2(out GroupElementP2 p2, a, A, b, B);
            byte[] res = new byte[32];
            GroupOperations.ge_tobytes(res, 0, ref p2);

            return res;
        }

        private static GroupElementP3[] CalculateDigitalPoints(ulong coefBase, GroupElementP3 assetCommitment, GroupElementP3 D)
        {
            GroupElementP3[] res = new GroupElementP3[4];
            for (ulong i = 0; i < 4; i++)
            {
                byte[] scalar = new byte[32];
                Array.Copy(BitConverter.GetBytes(i * coefBase), 0, scalar, 0, sizeof(ulong));
                GroupOperations.ge_scalarmult_p3(out GroupElementP3 p3, scalar, ref assetCommitment);
                //byte[] tmp = new byte[32];
                //GroupOperations.ge_tobytes(tmp, 0, ref p2);
                //GroupOperations.ge_frombytes(out GroupElementP3 p3, tmp, 0);
                GroupOperations.ge_p3_to_cached(out GroupElementCached cached, ref p3);
                GroupOperations.ge_sub(out GroupElementP1P1 p1p1, ref D, ref cached);
                GroupOperations.ge_p1p1_to_p3(out res[i], ref p1p1);
            }

            return res;
        }

        private static byte[] ComputeInnerE(byte cnt, GroupElementP3 p3, byte[] msg, ulong t, ulong i, byte w)
        {
            byte[] p3bytes = new byte[32];
            GroupOperations.ge_p3_tobytes(p3bytes, 0, ref p3);
            byte[] hash = FastHash512(new byte[] { cnt }, p3bytes, msg, BitConverter.GetBytes(t), BitConverter.GetBytes(i), new byte[] { w });

            return ReduceScalar64(hash);
        }

        private static GroupElementP3[] CalcIARPPubKeys(GroupElementP3 assetCommitment, GroupElementP3[] allAssetCommitments, byte[] h, GroupElementP3[] issuanceKeys)
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

        private static byte[] EncodeCommitment(byte[] commitment, byte[] sharedSecret)
        {
            byte[] sharedSecret1 = FastHash256(sharedSecret);
            ScalarOperations.sc_reduce32(sharedSecret1);

            byte[] encodedCommitment = (byte[])commitment.Clone();
            ScalarOperations.sc_add(encodedCommitment, encodedCommitment, sharedSecret1);

            return encodedCommitment;
        }

        private static void EcdhEncodeCA(EcdhTupleCA unmasked, byte[] sharedSecret)
        {
            if (unmasked == null)
            {
                throw new ArgumentNullException(nameof(unmasked));
            }

            if (sharedSecret == null)
            {
                throw new ArgumentNullException(nameof(sharedSecret));
            }

            byte[] sharedSecret1 = FastHash256(sharedSecret);
            ScalarOperations.sc_reduce32(sharedSecret1);

            byte[] sharedSecret2 = FastHash256(sharedSecret1);
            ScalarOperations.sc_reduce32(sharedSecret2);

            ScalarOperations.sc_add(unmasked.Mask, unmasked.Mask, sharedSecret1);
            ScalarOperations.sc_add(unmasked.AssetId, unmasked.AssetId, sharedSecret2);
        }

        private static void EcdhEncodeProofs(EcdhTupleProofs unmasked, byte[] sharedSecret)
        {
            if (unmasked == null)
            {
                throw new ArgumentNullException(nameof(unmasked));
            }

            if (sharedSecret == null)
            {
                throw new ArgumentNullException(nameof(sharedSecret));
            }

            byte[] sharedSecret1 = FastHash256(sharedSecret);
            ScalarOperations.sc_reduce32(sharedSecret1);

            byte[] sharedSecret2 = FastHash256(sharedSecret1);
            ScalarOperations.sc_reduce32(sharedSecret2);

            byte[] sharedSecret3 = FastHash256(sharedSecret2);
            ScalarOperations.sc_reduce32(sharedSecret3);

            byte[] sharedSecret4 = FastHash256(sharedSecret3);
            ScalarOperations.sc_reduce32(sharedSecret4);

            ScalarOperations.sc_add(unmasked.Mask, unmasked.Mask, sharedSecret1);
            ScalarOperations.sc_add(unmasked.AssetId, unmasked.AssetId, sharedSecret2);
            ScalarOperations.sc_add(unmasked.AssetIssuer, unmasked.AssetIssuer, sharedSecret3);
            ScalarOperations.sc_add(unmasked.Payload, unmasked.Payload, sharedSecret4);
        }

        private static void EcdhDecode(EcdhTupleCA masked, byte[] sharedSecret)
        {
            if (masked == null)
            {
                throw new ArgumentNullException(nameof(masked));
            }

            if (sharedSecret == null)
            {
                throw new ArgumentNullException(nameof(sharedSecret));
            }

            byte[] sharedSecret1 = FastHash256(sharedSecret);
            ScalarOperations.sc_reduce32(sharedSecret1);

            byte[] sharedSecret2 = FastHash256(sharedSecret1);
            ScalarOperations.sc_reduce32(sharedSecret2);

            ScalarOperations.sc_sub(masked.Mask, masked.Mask, sharedSecret1);
            ScalarOperations.sc_sub(masked.AssetId, masked.AssetId, sharedSecret2);
        }

        private static void EcdhDecode(EcdhTupleProofs masked, byte[] sharedSecret)
        {
            if (masked == null)
            {
                throw new ArgumentNullException(nameof(masked));
            }

            if (sharedSecret == null)
            {
                throw new ArgumentNullException(nameof(sharedSecret));
            }

            byte[] sharedSecret1 = FastHash256(sharedSecret);
            ScalarOperations.sc_reduce32(sharedSecret1);

            byte[] sharedSecret2 = FastHash256(sharedSecret1);
            ScalarOperations.sc_reduce32(sharedSecret2);

            byte[] sharedSecret3 = FastHash256(sharedSecret2);
            ScalarOperations.sc_reduce32(sharedSecret3);

            byte[] sharedSecret4 = FastHash256(sharedSecret3);
            ScalarOperations.sc_reduce32(sharedSecret4);

            ScalarOperations.sc_sub(masked.Mask, masked.Mask, sharedSecret1);
            ScalarOperations.sc_sub(masked.AssetId, masked.AssetId, sharedSecret2);
            ScalarOperations.sc_sub(masked.AssetIssuer, masked.AssetIssuer, sharedSecret3);
            ScalarOperations.sc_sub(masked.Payload, masked.Payload, sharedSecret4);
        }

        private static bool ConstTimeEquals(byte[] a, byte[] b)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            if (a.Length != b.Length)
            {
                return false;
            }

            byte v = 0;
            for (int i = 0; i < a.Length; i++)
            {
                v |= (byte)(a[i] ^ b[i]);
            }

            return ConstantTimeByteEq(v, 0) == 1;
        }

        private static int ConstantTimeByteEq(byte a, byte b)
        {
            return (int)(((uint)(a ^ b) - 1) >> 31);
        }

        #endregion Private Methods

        #region Private Debug Purpose Methods
        private static int[] ParseString(string str)
        {
            int[] fields = str.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s)).ToArray();

            return fields;
        }

        private static GroupElementP3 LoadFromFile(string path)
        {
            string[] lines = File.ReadAllLines(path);

            GroupElementP3 p3 = new GroupElementP3
            {
                X = new FieldElement(ParseString(lines[0])),
                Y = new FieldElement(ParseString(lines[1])),
                Z = new FieldElement(ParseString(lines[2])),
                T = new FieldElement(ParseString(lines[3])),
            };

            return p3;
        }

        private static byte[] LoadArrayFromFile(string path)
        {
            string line = File.ReadAllText(path);
            byte[] arr = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => byte.Parse(s)).ToArray();

            return arr;
        }

        #endregion Private Debug Purpose Methods

        #region MLSAG

        //Multilayered Spontaneous Anonymous Group Signatures (MLSAG signatures)
        //This is a just slightly more efficient version than the ones described below
        //(will be explained in more detail in Ring Multisig paper
        //These are aka MG signatures in earlier drafts of the ring ct paper
        // c.f. https://eprint.iacr.org/2015/1098 section 2. 
        // Gen creates a signature which proves that for some column in the keymatrix "pk"
        //   the signer knows a secret key for each row in that column
        // Ver verifies that the MG sig was created correctly        
        private static MgSig MLSAG_Gen(byte[] message, byte[][][] pk, byte[][] sk, int index, int dsRows)
        {
            MgSig mgSig = new MgSig();
            int cols = pk.Length;
            if (cols < 2)
            {
                throw new ArgumentException("Error! What is c if count of pk = 1!");
            }
            if (index >= cols)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            int rows = pk[0].Length;
            if (rows == 0)
            {
                throw new ArgumentException(nameof(pk), "Empty pk");
            }
            for (int i1 = 1; i1 < cols; i1++)
            {
                if (pk[i1].Length != rows)
                {
                    throw new ArgumentException(nameof(pk), "PK matrix is not rectangular");
                }
            }

            if (sk.Length != rows)
            {
                throw new ArgumentException(nameof(sk), $"Bad {nameof(sk)} size");
            }

            if (dsRows > rows)
            {
                throw new ArgumentException(nameof(dsRows), $"Bad {nameof(dsRows)} size");
            }

            List<GroupElementCached[]> Ip = new List<GroupElementCached[]>(dsRows);
            for (int k = 0; k < dsRows; k++)
            {
                Ip.Add(new GroupElementCached[8]);
            }

            mgSig.II = new byte[dsRows][];
            byte[][] alpha = new byte[rows][];

            mgSig.SS = new byte[cols][][];
            for (int n = 0; n < cols; n++)
            {
                mgSig.SS[n] = new byte[rows][];
            }

            byte[][] aHP = new byte[dsRows][];
            Memory<byte>[] toHash = new Memory<byte>[1 + 3 * dsRows + 2 * (rows - dsRows)];
            toHash[0] = message;

            for (int i1 = 0; i1 < dsRows; i1++)
            {
                for (int j = 0; j < mgSig.SS[index].Length; j++)
                {
                    mgSig.SS[index][j] = new byte[32];
                }

                toHash[3 * i1 + 1] = pk[index][i1];
                GroupElementP3 Hi = Hash2Point(pk[index][i1]);
                Mlsag_Prepare(Hi, sk[i1], out byte[] alphaI, out byte[] aGi, out aHP[i1], out mgSig.II[i1]);

                alpha[i1] = alphaI; // alphaI - generated secret key

                toHash[3 * i1 + 2] = aGi;
                toHash[3 * i1 + 3] = aHP[i1];
                Precomp(Ip[i1], mgSig.II[i1]);
            }

            int ndsRows = 3 * dsRows; //non Double Spendable Rows (see identity chains paper)
            for (int i1 = dsRows, ii = 0; i1 < rows; i1++, ii++)
            {
                alpha[i1] = GetRandomSeed();
                toHash[ndsRows + 2 * ii + 1] = pk[index][i1];
                toHash[ndsRows + 2 * ii + 2] = GetPublicKey(alpha[i1]);
            }

            byte[] c_old = FastHash256(toHash);
            ReduceScalar32(c_old);

            int i = (index + 1) % cols;
            if (i == 0)
            {
                Array.Copy(c_old, 0, mgSig.CC, 0, c_old.Length);
            }

            while (i != index)
            {
                for (int k = 0; k < rows; k++)
                {
                    mgSig.SS[i][k] = GetRandomSeed();
                }

                for (int j = 0; j < dsRows; j++)
                {
                    byte[] L = ScalarmulBaseAddKeys(mgSig.SS[i][j], c_old, pk[i][j]);
                    GroupElementP3 Hi = Hash2Point(pk[i][j]);
                    byte[] R = ScalarmulBaseAddKeys2(mgSig.SS[i][j], Hi, c_old, Ip[j]);
                    toHash[3 * j + 1] = pk[i][j];
                    toHash[3 * j + 2] = L;
                    toHash[3 * j + 3] = R;
                }

                for (int j = dsRows, ii = 0; j < rows; j++, ii++)
                {
                    byte[] L = ScalarmulBaseAddKeys(mgSig.SS[i][j], c_old, pk[i][j]);
                    toHash[ndsRows + 2 * ii + 1] = pk[i][j];
                    toHash[ndsRows + 2 * ii + 2] = L;
                }

                byte[] c = FastHash256(toHash);
                ReduceScalar32(c);

                Array.Copy(c, 0, c_old, 0, c.Length);

                i = (i + 1) % cols;

                if (i == 0)
                {
                    Array.Copy(c_old, 0, mgSig.CC, 0, c_old.Length);
                }
            }

            Mlsag_Sign(c_old, sk, alpha, rows, dsRows, mgSig.SS[index]);
            return mgSig;
        }

        private static void Precomp(GroupElementCached[] groupElementCached, byte[] keyImage)
        {
            GroupOperations.ge_frombytes(out GroupElementP3 imageP3, keyImage, 0);

            GroupOperations.ge_dsm_precomp(groupElementCached, ref imageP3);
        }

        //Ring-ct MG sigs
        //Prove: 
        //   c.f. https://eprint.iacr.org/2015/1098 section 4. definition 10. 
        //   This does the MG sig on the "dest" part of the given key matrix, and 
        //   the last row is the sum of input commitments from that column - sum output commitments
        //   this shows that sum inputs = sum outputs
        //Ver:    
        //   verifies the above sig is created correctly
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="pubs">collection of collections of pairs of public key and Pedersen Commitment used for inputs where index of real collection of pairs designated by argument "index"</param>
        /// <param name="inSk">collection of pairs of secrect keys and blinding factors of all inputs used for transaction</param>
        /// <param name="outSk">collection of pairs of secrect keys and blinding factors of all outputs used for transaction</param>
        /// <param name="outPk">collection of pairs of public keys of receiver and pedersen commitments of amount sent to him</param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static MgSig ProveRctMG(byte[] message, CtTuple[][] pubs, CtTuple[] inSk, CtTuple[] outSk, CtTuple[] outPk, int index)
        {
            //setup vars
            int cols = pubs.Length;
            if (cols == 0)
            {
                throw new ArgumentException(nameof(pubs), $"Empty {nameof(pubs)}");
            }

            int rows = pubs[0].Length;
            if (rows == 0)
            {
                throw new ArgumentException(nameof(pubs), $"Empty {nameof(pubs)}");
            }

            for (int k = 1; k < cols; k++)
            {
                if (pubs[k].Length != rows)
                {
                    throw new ArgumentException(nameof(pubs), $"{nameof(pubs)} is not rectangular");
                }
            }

            if (inSk.Length != rows)
            {
                throw new ArgumentException(nameof(inSk), $"Bad {nameof(inSk)} size");
            }

            if (outSk.Length != outPk.Length)
            {
                throw new ArgumentException(nameof(outSk), $"Bad {nameof(outSk)}/{nameof(outPk)} size");
            }

            byte[][] sk = new byte[rows + 1][];

            byte[][] tmp = new byte[rows + 1][];
            for (int k = 0; k < rows + 1; k++)
            {
                tmp[k] = (byte[])I.Clone();
            }

            byte[][][] M = new byte[cols][][];

            //create the matrix to mg sig
            for (int i = 0; i < cols; i++)
            {
                M[i] = new byte[rows + 1][];
                M[i][rows] = (byte[])I.Clone();
                for (int j = 0; j < rows; j++)
                {
                    M[i][j] = (byte[])pubs[i][j].Dest.Clone();
                    M[i][rows] = SumCommitments(M[i][rows], pubs[i][j].Mask); //add input commitments in last row
                }
            }

            sk[rows] = new byte[32];
            for (int j = 0; j < rows; j++)
            {
                sk[j] = new byte[32];
                Array.Copy(inSk[j].Dest, 0, sk[j], 0, inSk[j].Dest.Length);
                ScalarOperations.sc_add(sk[rows], sk[rows], inSk[j].Mask); //add blinding factor in last row
            }

            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < outPk.Length; j++)
                {
                    M[i][rows] = SubCommitments(M[i][rows], outPk[j].Mask); //subtract output Ci's in last row
                }
            }

            for (int j = 0; j < outPk.Length; j++)
            {
                ScalarOperations.sc_sub(sk[rows], sk[rows], outSk[j].Mask); //subtract output masks in last row..
            }

            MgSig result = MLSAG_Gen(message, M, sk, index, rows);

            for (int i = 0; i < sk.Length; i++)
            {
                Array.Clear(sk[i], 0, sk[i].Length);
            }

            return result;
        }

        //Ring-ct MG sigs
        //Prove: 
        //   c.f. https://eprint.iacr.org/2015/1098 section 4. definition 10. 
        //   This does the MG sig on the "dest" part of the given key matrix, and 
        //   the last row is the sum of input commitments from that column - sum output commitments
        //   this shows that sum inputs = sum outputs
        //Ver:    
        //   verifies the above sig is created corretly
        public static bool VerRctMG(MgSig mg, CtTuple[][] pubs, CtTuple[] outPk, byte[] message)
        {
            //setup vars
            int cols = pubs.Length;
            if (cols == 0)
            {
                throw new ArgumentException(nameof(pubs), $"Empty {nameof(pubs)}");
            }
            int rows = pubs[0].Length;
            if (rows == 0)
            {
                throw new ArgumentException(nameof(pubs), $"Empty {nameof(pubs)}");
            }
            for (int k = 1; k < cols; k++)
            {
                if (pubs[k].Length != rows)
                {
                    throw new ArgumentException(nameof(pubs), $"{nameof(pubs)} is not rectangular");
                }
            }

            byte[][] tmp = new byte[rows + 1][];
            for (int k = 0; k < rows + 1; k++)
            {
                tmp[k] = (byte[])I.Clone();
            }

            byte[][][] M = new byte[cols][][];

            //create the matrix to mg sig
            for (int i = 0; i < cols; i++)
            {
                M[i] = new byte[rows + 1][];
                M[i][rows] = (byte[])I.Clone();
                for (int j = 0; j < rows; j++)
                {
                    M[i][j] = (byte[])pubs[i][j].Dest.Clone();
                    M[i][rows] = SumCommitments(M[i][rows], pubs[i][j].Mask); //add input commitments in last row
                }
            }

            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < outPk.Length; j++)
                {
                    M[i][rows] = SubCommitments(M[i][rows], outPk[j].Mask); //subtract output Ci's in last row
                }
            }

            return MLSAG_Ver(message, M, mg, rows);
        }

        //Multilayered Spontaneous Anonymous Group Signatures (MLSAG signatures)
        //This is a just slghtly more efficient version than the ones described below
        //(will be explained in more detail in Ring Multisig paper
        //These are aka MG signatutes in earlier drafts of the ring ct paper
        // c.f. https://eprint.iacr.org/2015/1098 section 2. 
        // Gen creates a signature which proves that for some column in the keymatrix "pk"
        //   the signer knows a secret key for each row in that column
        // Ver verifies that the MG sig was created correctly            
        private static bool MLSAG_Ver(byte[] message, byte[][][] pk, MgSig rv, int dsRows)
        {

            int cols = pk.Length;
            if (cols <= 1)
            {
                throw new ArgumentException(nameof(pk), $"Error! What is c if {nameof(cols)} = 1!");
            }

            int rows = pk[0].Length;
            if (rows == 0)
            {
                throw new ArgumentException(nameof(pk), $"Empty {nameof(pk)}");
            }

            for (int k = 1; k < cols; ++k)
            {
                if (pk[k].Length != rows)
                {
                    throw new ArgumentException(nameof(pk), $"{nameof(pk)} is not rectangular");
                }
            }

            if (rv.II.Length != dsRows)
            {
                throw new ArgumentException(nameof(rv), $"Bad {rv.II} size");
            }

            if (rv.SS.Length != cols)
            {
                throw new ArgumentException(nameof(rv), $"Bad {rv.SS} size");
            }

            for (int k = 0; k < cols; ++k)
            {
                if (rv.SS[k].Length != rows)
                {
                    throw new ArgumentException(nameof(rv), $"{rv.SS} is not rectangular");
                }
            }
            if (dsRows > rows)
            {
                throw new ArgumentException(nameof(dsRows), $"Bad {nameof(dsRows)} value");
            }

            for (int i = 0; i < rv.SS.Length; ++i)
            {
                for (int j = 0; j < rv.SS[i].Length; ++j)
                {
                    int scCheck = ScalarOperations.sc_check(rv.SS[i][j]);
                    if (scCheck != 0)
                    {
                        throw new ArgumentException(nameof(rv.SS), $"Bad {rv.SS} slot");
                    }
                }
            }

            if (ScalarOperations.sc_check(rv.CC) != 0)
            {
                throw new ArgumentException(nameof(rv.CC), $"Bad {nameof(rv.CC)}");
            }

            byte[] c_old = new byte[32];
            Array.Copy(rv.CC, 0, c_old, 0, rv.CC.Length);
            List<GroupElementCached[]> Ip = new List<GroupElementCached[]>();
            for (int i = 0; i < dsRows; i++)
            {
                Ip.Add(new GroupElementCached[8]);
                for (int j = 0; j < 8; j++)
                {
                    Ip[i][j] = new GroupElementCached();
                }
            }
            for (int i = 0; i < dsRows; i++)
            {
                Precomp(Ip[i], rv.II[i]);
            }

            int ndsRows = 3 * dsRows; //non Double Spendable Rows (see identity chains paper
            int toHashSize = 1 + 3 * dsRows + 2 * (rows - dsRows);
            Memory<byte>[] toHash = new Memory<byte>[toHashSize];

            toHash[0] = message;
            int i1 = 0;
            while (i1 < cols)
            {
                for (int j = 0; j < dsRows; j++)
                {
                    byte[] L = ScalarmulBaseAddKeys(rv.SS[i1][j], c_old, pk[i1][j]);
                    GroupElementP3 Hi = Hash2Point(pk[i1][j]);
                    byte[] Hi_bytes = new byte[32];
                    GroupOperations.ge_p3_tobytes(Hi_bytes, 0, ref Hi);
                    if (Hi_bytes.Equals32(I))
                    {
                        throw new Exception("Data hashed to point at infinity");
                    }
                    byte[] R = ScalarmulBaseAddKeys2(rv.SS[i1][j], Hi, c_old, Ip[j]);
                    toHash[3 * j + 1] = pk[i1][j];
                    toHash[3 * j + 2] = L;
                    toHash[3 * j + 3] = R;
                }
                for (int j = dsRows, ii = 0; j < rows; j++, ii++)
                {
                    byte[] L = ScalarmulBaseAddKeys(rv.SS[i1][j], c_old, pk[i1][j]);
                    toHash[ndsRows + 2 * ii + 1] = pk[i1][j];
                    toHash[ndsRows + 2 * ii + 2] = L;
                }
                byte[] c = FastHash256(toHash);
                ReduceScalar32(c);
                Array.Copy(c, 0, c_old, 0, c.Length);
                i1++;
            }

            byte[] c_res = new byte[32];
            ScalarOperations.sc_sub(c_res, c_old, rv.CC);
            int res = ScalarOperations.sc_isnonzero(c_res);

            return res == 0;
        }

        private static bool Mlsag_Prepare(GroupElementP3 H, byte[] xx, out byte[] a, out byte[] aG, out byte[] aHP, out byte[] II)
        {
            a = GetRandomSeed();
            aG = GetPublicKey(a);

            GroupOperations.ge_scalarmult(out GroupElementP2 aHP_P2, a, ref H);
            aHP = new byte[32];
            GroupOperations.ge_tobytes(aHP, 0, ref aHP_P2);
            GroupOperations.ge_scalarmult(out GroupElementP2 II_P2, xx, ref H);
            II = new byte[32];
            GroupOperations.ge_tobytes(II, 0, ref II_P2);
            return true;
        }

        private static bool Mlsag_Sign(byte[] c, byte[][] xx, byte[][] alpha, int rows, int dsRows, byte[][] ss)
        {
            if (c == null)
            {
                throw new ArgumentNullException(nameof(c));
            }

            if (xx == null)
            {
                throw new ArgumentNullException(nameof(xx));
            }

            if (alpha == null)
            {
                throw new ArgumentNullException(nameof(alpha));
            }

            if (dsRows > rows)
            {
                throw new ArgumentException(nameof(dsRows), $"{nameof(dsRows)} greater than {nameof(rows)}");
            }

            if (xx.Length != rows)
            {
                throw new ArgumentException(nameof(xx), $"{nameof(xx)} size does not match {nameof(rows)}");
            }

            if (alpha.Length != rows)
            {
                throw new ArgumentException(nameof(alpha), $"{nameof(alpha)} size does not match {nameof(rows)}");
            }

            if (ss.Length != rows)
            {
                throw new ArgumentException(nameof(ss), $"{nameof(ss)} size does not match {nameof(rows)}");
            }

            for (int j = 0; j < rows; j++)
            {
                ScalarOperations.sc_mulsub(ss[j], c, xx[j], alpha[j]); // ss[j] = alpha[j] - xx[j] * c
                int res = ScalarOperations.sc_check(ss[j]);
            }

            return true;
        }

        #endregion MLSAG


        #region Range Proofs

        public static BorromeanRingSignatureEx GenBorromean(byte[][] x, byte[][] P1, byte[][] P2, byte[] indices)
        {
            Memory<byte>[][] L = new Memory<byte>[][] {
                new Memory<byte>[64], new Memory<byte>[64]
            };

            byte[][] alpha = new byte[64][];

            BorromeanRingSignatureEx bb = new BorromeanRingSignatureEx();
            for (int ii = 0; ii < 64; ii++)
            {
                int naught = indices[ii];
                int prime = (indices[ii] + 1) % 2;
                alpha[ii] = GetRandomSeed();
                L[naught][ii] = ScalarmulBase(alpha[ii]);
                if (naught == 0)
                {
                    bb.S[1][ii] = GetRandomSeed();

                    byte[] c = FastHash256(L[naught][ii]);
                    ScalarOperations.sc_reduce32(c);

                    L[prime][ii] = ScalarmulBaseAddKeys(bb.S[1][ii], c, P2[ii]);
                }
            }

            bb.E = FastHash256(L[1]); //or L[1]..
            ScalarOperations.sc_reduce32(bb.E);

            for (int jj = 0; jj < 64; jj++)
            {
                if (indices[jj] == 0)
                {
                    bb.S[0][jj] = new byte[32];
                    ScalarOperations.sc_mulsub(bb.S[0][jj], x[jj], bb.E, alpha[jj]);
                }
                else
                {
                    bb.S[0][jj] = GetRandomSeed();
                    byte[] LL = ScalarmulBaseAddKeys(bb.S[0][jj], bb.E, P1[jj]); //different L0
                    byte[] cc = FastHash256(LL);
                    ScalarOperations.sc_reduce32(cc);
                    bb.S[1][jj] = new byte[32];
                    ScalarOperations.sc_mulsub(bb.S[1][jj], x[jj], cc, alpha[jj]);
                }
            }

            return bb;
        }

        public static RangeProof ProveRange(out byte[] C, out byte[] mask, ulong amount, byte[] assetCommitment)
        {
            mask = new byte[32];
            C = (byte[])I.Clone();
            byte[] b = new byte[64];
            d2b(ref b, amount);
            RangeProof sig = new RangeProof();
            byte[][] ai = new byte[64][];
            byte[][] CiH = new byte[64][];
            byte[][] Hi = new byte[64][];
            for (int i = 0; i < 64; i++)
            {
                Hi[i] = ScalarmulPoint(((ulong)(1 << i)).ToByteArray(), assetCommitment);
            }

            for (int i = 0; i < 64; i++)
            {
                ai[i] = GetRandomSeed();
                if (b[i] == 0)
                {
                    sig.D[i] = ScalarmulBase(ai[i]);
                }
                if (b[i] == 1)
                {
                    sig.D[i] = ScalarmulBaseAddKeys1(ai[i], Hi[i]);
                }

                CiH[i] = SubCommitments(sig.D[i], Hi[i]);

                ScalarOperations.sc_add(mask, mask, ai[i]);
                C = SumCommitments(C, sig.D[i]);
            }

            sig.BorromeanRingSignature = GenBorromean(ai, sig.D, CiH, b);
            return sig;
        }

        private static void d2b(ref byte[] amountb, ulong val)
        {
            int i = 0;
            while (val != 0)
            {
                amountb[i] = (byte)(val & 1);
                i++;
                val >>= 1;
            }
            while (i < 64)
            {
                amountb[i] = 0;
                i++;
            }
        }

        public static bool VerRange(byte[] C, RangeProof rangeProof, byte[] assetCommitment)
        {
            try
            {
                byte[][] Hi = new byte[64][];
                for (int i = 0; i < 64; i++)
                {
                    Hi[i] = ScalarmulPoint(((ulong)(1 << i)).ToByteArray(), assetCommitment);
                }

                GroupElementP3[] CiH = new GroupElementP3[64], asCi = new GroupElementP3[64];
                GroupOperations.ge_frombytes(out GroupElementP3 Ctmp_p3, I, 0);
                for (int i = 0; i < 64; i++)
                {
                    // faster equivalent of:
                    // subKeys(CiH[i], as.Ci[i], H2[i]);
                    // addKeys(Ctmp, Ctmp, as.Ci[i]);
                    if (GroupOperations.ge_frombytes(out GroupElementP3 p3, Hi[i], 0) != 0)
                    {
                        throw new Exception("point conv failed");
                    }
                    GroupOperations.ge_p3_to_cached(out GroupElementCached cached, ref p3);
                    if (GroupOperations.ge_frombytes(out asCi[i], rangeProof.D[i], 0) != 0)
                    {
                        throw new Exception("point conv failed");
                    }
                    GroupOperations.ge_sub(out GroupElementP1P1 p1, ref asCi[i], ref cached);
                    GroupOperations.ge_p3_to_cached(out cached, ref asCi[i]);
                    GroupOperations.ge_p1p1_to_p3(out CiH[i], ref p1);
                    GroupOperations.ge_add(out p1, ref Ctmp_p3, ref cached);
                    GroupOperations.ge_p1p1_to_p3(out Ctmp_p3, ref p1);
                }
                byte[] Ctmp = new byte[32];
                GroupOperations.ge_p3_tobytes(Ctmp, 0, ref Ctmp_p3);
                if (!C.Equals32(Ctmp))
                    return false;
                if (!VerifyBorromean(rangeProof.BorromeanRingSignature, asCi, CiH))
                    return false;
                return true;
            }
            // we can get deep throws from ge_frombytes_vartime if input isn't valid
            catch
            {
                return false;
            }
        }

        private static bool VerifyBorromean(BorromeanRingSignatureEx bb, GroupElementP3[] P1, GroupElementP3[] P2)
        {
            Memory<byte>[] Lv1 = new Memory<byte>[64];
            byte[] chash, LL;

            for (int ii = 0; ii < 64; ii++)
            {
                // equivalent of: addKeys2(LL, bb.s0[ii], bb.ee, P1[ii]);
                GroupOperations.ge_double_scalarmult_vartime(out GroupElementP2 p2, bb.E, ref P1[ii], bb.S[0][ii]);
                LL = new byte[32];
                GroupOperations.ge_tobytes(LL, 0, ref p2);
                chash = FastHash256(LL);
                ScalarOperations.sc_reduce32(chash);
                // equivalent of: addKeys2(Lv1[ii], bb.s1[ii], chash, P2[ii]);
                GroupOperations.ge_double_scalarmult_vartime(out p2, chash, ref P2[ii], bb.S[1][ii]);
                byte[] arr = new byte[32];
                GroupOperations.ge_tobytes(arr, 0, ref p2);
                Lv1[ii] = arr;
            }
            byte[] eeComputed = FastHash256(Lv1); //hash function fine
            ScalarOperations.sc_reduce32(eeComputed);
            return eeComputed.Equals32(bb.E);
        }

        #endregion
    }
}
