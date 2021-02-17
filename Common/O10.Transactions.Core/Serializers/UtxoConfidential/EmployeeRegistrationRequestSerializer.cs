using System;
using System.IO;
using O10.Transactions.Core.Ledgers.Stealth;
using O10.Transactions.Core.Ledgers.Stealth.Internal;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;


namespace O10.Transactions.Core.Serializers.Stealth
{
	[RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.Transient)]
    public class EmployeeRegistrationRequestSerializer : StealthTransactionSerializerBase<EmployeeRegistrationRequest>
    {
        public EmployeeRegistrationRequestSerializer(IServiceProvider serviceProvider) 
			: base(serviceProvider, LedgerType.Stealth, PacketTypes.Stealth_EmployeeReqistrationRequest)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
			base.WriteBody(bw);

			WriteEcdhTupleProofs(bw, _block.EcdhTuple);
            WriteSurjectionProof(bw, _block.OwnershipProof);
            WriteSurjectionProof(bw, _block.EligibilityProof);

			if(_block.AssociatedProofs != null && _block.AssociatedProofs.Length > 0)
			{
				bw.Write((byte)_block.AssociatedProofs.Length);

				foreach (var item in _block.AssociatedProofs)
				{
					if(item is AssociatedAssetProofs associatedAssetProofs)
					{
						bw.Write((byte)1);
						bw.Write(associatedAssetProofs.AssociatedAssetCommitment);
					}
					else
					{
						bw.Write((byte)0);
					}

					bw.Write(item.AssociatedAssetGroupId);
					WriteSurjectionProof(bw, item.AssociationProofs);
					WriteSurjectionProof(bw, item.RootProofs);
				}
			}
			else
			{
				bw.Write((byte)0);
			}

            bw.Write(_block.GroupCommitment);
            WriteSurjectionProof(bw, _block.GroupSurjectionProof);
        }
    }
}
