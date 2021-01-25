using System;
using O10.Transactions.Core.DataModel.Stealth;
using O10.Transactions.Core.DataModel.Stealth.Internal;
using O10.Transactions.Core.Enums;
using O10.Core;
using O10.Core.Cryptography;
using O10.Core.Identity;

namespace O10.Transactions.Core.Parsers.Stealth
{
	public abstract class StealthTransactionParserBase : StealthParserBase
    {
        public StealthTransactionParserBase(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
        {
        }

        public override PacketType PacketType => PacketType.Stealth;

        protected override Memory<byte> ParseStealth(ushort version, Memory<byte> spanBody, out StealthBase StealthBase)
        {
            int readBytes = 0;

			ReadCommitment(ref spanBody, ref readBytes, out byte[] assetCommitment);
			ReadBiometricProof(ref spanBody, ref readBytes, out BiometricProof biometricProof);

			Memory<byte> spanPostBody = ParseStealthTransaction(version, spanBody.Slice(readBytes), out StealthTransactionBase StealthTransactionBase);

			StealthTransactionBase.AssetCommitment = assetCommitment;
			StealthTransactionBase.BiometricProof = biometricProof;
			StealthBase = StealthTransactionBase;

            return spanPostBody;
        }

        protected abstract Memory<byte> ParseStealthTransaction(ushort version, Memory<byte> spanBody, out StealthTransactionBase StealthTransactionBase);
 
		private void ReadBiometricProof(ref Memory<byte> spanBody, ref int readBytes, out BiometricProof biometricProof)
		{
			bool isBiometricProofProvided = Convert.ToBoolean(spanBody.Span[readBytes]);
			readBytes++;

			biometricProof = null;

			if (isBiometricProofProvided)
			{
				ReadCommitment(ref spanBody, ref readBytes, out byte[] assetCommitment);
				ReadSurjectionProof(ref spanBody, ref readBytes, out SurjectionProof surjectionProof);
				byte[] signature = spanBody.Slice(readBytes, Globals.SIGNATURE_SIZE).ToArray();
				readBytes += Globals.SIGNATURE_SIZE;
				byte[] signer = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
				readBytes += Globals.NODE_PUBLIC_KEY_SIZE;


				biometricProof = new BiometricProof
				{
					BiometricCommitment = assetCommitment,
					BiometricSurjectionProof = surjectionProof,
					VerifierSignature = signature,
					VerifierPublicKey = signer
				};
			}
		}
 }
}
