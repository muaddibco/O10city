using O10.Core.Cryptography;
using System;

namespace O10.Core.ExtensionMethods
{
    public static class RingSignatureExtensionMethods
    {
        public static byte[] ToByteArray(this RingSignature ringSignature)
        {
            if (ringSignature is null)
            {
                throw new ArgumentNullException(nameof(ringSignature));
            }

            byte[] signature = new byte[64];

            Array.Copy(ringSignature.C, signature, 32);
            Array.Copy(ringSignature.R, 0, signature, 32, 32);

            return signature;
        }

        public static void FromByteArray(this RingSignature ringSignature, byte[] signature)
        {
            if (ringSignature is null)
            {
                throw new ArgumentNullException(nameof(ringSignature));
            }

            if (signature is null)
            {
                throw new ArgumentNullException(nameof(signature));
            }

            if(signature.Length != 64)
            {
                throw new ArgumentException("Only arrays of length of 64 bytes can be parsed", nameof(signature));
            }

            Array.Copy(signature, ringSignature.C, 32);
            Array.Copy(signature, 32, ringSignature.R, 0, 32);
        }
    }
}
