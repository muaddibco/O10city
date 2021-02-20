using System;
using System.IO;
using O10.Transactions.Core.Ledgers.Stealth;
using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Serializers.Stealth
{
	public abstract class StealthTransactionSerializerBase<T> : StealthSerializerBase<T> where T : StealthTransactionBase
    {
        public StealthTransactionSerializerBase(IServiceProvider serviceProvider, LedgerType ledgerType, ushort blockType) 
            : base(serviceProvider, packetType, blockType)
        {
        }

		protected override void WriteBody(BinaryWriter bw)
		{
			WriteCommitment(bw, _block.AssetCommitment);
			if(_block.BiometricProof != null)
			{
				bw.Write(true);
				WriteCommitment(bw, _block.BiometricProof.BiometricCommitment);
				WriteSurjectionProof(bw, _block.BiometricProof.BiometricSurjectionProof);
				bw.Write(_block.BiometricProof.VerifierSignature);
				bw.Write(_block.BiometricProof.VerifierPublicKey);
			}
			else
			{
				bw.Write(false);
			}
		}
	}
}
