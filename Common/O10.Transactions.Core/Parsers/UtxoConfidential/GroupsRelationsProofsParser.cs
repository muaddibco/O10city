using System;
using System.Buffers.Binary;
using O10.Transactions.Core.Ledgers.Stealth;
using O10.Transactions.Core.Ledgers.Stealth.Internal;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Exceptions;
using O10.Core.Architecture;

using O10.Core.Cryptography;
using O10.Core.Identity;

namespace O10.Transactions.Core.Parsers.Stealth
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class GroupsRelationsProofsParser : StealthTransactionParserBase
	{
        public GroupsRelationsProofsParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => PacketTypes.Stealth_GroupsRelationsProofs;

        protected override Memory<byte> ParseStealthTransaction(ushort version, Memory<byte> spanBody, out StealthTransactionBase StealthBase)
        {
            if (version == 1)
            {
                int readBytes = 0;

				ReadEcdhTupleProofs(ref spanBody, ref readBytes, out EcdhTupleProofs ecdhTuple);
				ReadSurjectionProof(ref spanBody, ref readBytes, out SurjectionProof ownershipProofs);
                ReadSurjectionProof(ref spanBody, ref readBytes, out SurjectionProof eligibilityProofs);

				byte associatedProofsCount = spanBody.Slice(readBytes++).Span[0];

				AssociatedProofs[] associatedProofs = new AssociatedProofs[associatedProofsCount];

				for (int i = 0; i < associatedProofsCount; i++)
				{
					byte associatedProofType = spanBody.Slice(readBytes++).Span[0];

					AssociatedProofs associatedProof;

					if (associatedProofType == 1)
					{
						ReadCommitment(ref spanBody, ref readBytes, out byte[] associatedAssetCommitment);
						associatedProof = new AssociatedAssetProofs
						{
							AssociatedAssetCommitment = associatedAssetCommitment
						};
					}
					else
					{
						associatedProof = new AssociatedProofs();
					}

					ReadCommitment(ref spanBody, ref readBytes, out byte[] associatedGroupId);
					ReadSurjectionProof(ref spanBody, ref readBytes, out SurjectionProof associationProofs);
					ReadSurjectionProof(ref spanBody, ref readBytes, out SurjectionProof rootProofs);

					associatedProof.AssociatedAssetGroupId = associatedGroupId;
					associatedProof.AssociationProofs = associationProofs;
					associatedProof.RootProofs = rootProofs;

					associatedProofs[i] = associatedProof;
				}

				ushort relationsCount = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Slice(readBytes).Span);
                readBytes += sizeof(ushort);

                GroupRelationProof[] groupRelationProofs = new GroupRelationProof[relationsCount];

                for (int i = 0; i < relationsCount; i++)
                {
                    ReadCommitment(ref spanBody, ref readBytes, out byte[] groupOwner);
                    ReadSurjectionProof(ref spanBody, ref readBytes, out SurjectionProof relationProof);
                    ReadSurjectionProof(ref spanBody, ref readBytes, out SurjectionProof groupNameProof);

                    groupRelationProofs[i] = new GroupRelationProof
                    {
                        GroupOwner = groupOwner,
                        RelationProof = relationProof,
                        GroupNameProof = groupNameProof
                    };
                }

                GroupsRelationsProofs block = new GroupsRelationsProofs
                {
                    EcdhTuple = ecdhTuple,
                    OwnershipProof = ownershipProofs,
                    EligibilityProof = eligibilityProofs,
					AssociatedProofs = associatedProofs,
                    RelationProofs = groupRelationProofs,
                };
                StealthBase = block;

                return spanBody.Slice(readBytes);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
