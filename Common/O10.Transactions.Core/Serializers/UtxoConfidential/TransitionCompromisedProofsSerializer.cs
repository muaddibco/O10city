using System;
using System.IO;
using O10.Transactions.Core.Ledgers.Stealth;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;


namespace O10.Transactions.Core.Serializers.Stealth
{
	[RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.Transient)]
	public class TransitionCompromisedProofsSerializer : StealthTransactionSerializerBase<TransitionCompromisedProofs>
	{
		public TransitionCompromisedProofsSerializer(IServiceProvider serviceProvider) 
			: base(serviceProvider, LedgerType.Stealth, PacketTypes.Stealth_TransitionCompromisedProofs)
		{
		}

		protected override void WriteBody(BinaryWriter bw)
		{
			base.WriteBody(bw);

			WriteEcdhTupleCA(bw, _block.EcdhTuple);
			WriteSurjectionProof(bw, _block.OwnershipProof);
			WriteSurjectionProof(bw, _block.EligibilityProof);
			bw.Write(_block.CompromisedKeyImage);
			// TODO: need to add serializing compromised Public Keys and Signature
		}
	}
}
