using System;
using O10.Transactions.Core.DataModel.Stealth;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Exceptions;
using O10.Core.Architecture;

using O10.Core.Cryptography;
using O10.Core.Identity;

namespace O10.Transactions.Core.Parsers.Stealth
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class DocumentSignRequestParser : StealthTransactionParserBase
	{
        public DocumentSignRequestParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => ActionTypes.Stealth_DocumentSignRequest;

        protected override Memory<byte> ParseStealthTransaction(ushort version, Memory<byte> spanBody, out StealthTransactionBase StealthBase)
        {
			DocumentSignRequest block = null;

            if (version == 1)
            {
                int readBytes = 0;

				ReadEcdhTupleProofs(ref spanBody, ref readBytes, out EcdhTupleProofs ecdhTuple);
				ReadSurjectionProof(ref spanBody, ref readBytes, out SurjectionProof ownershipProofs);
                ReadSurjectionProof(ref spanBody, ref readBytes, out SurjectionProof eligibilityProofs);

				ReadSurjectionProof(ref spanBody, ref readBytes, out SurjectionProof signerGroupRelationProof);
				//ReadSurjectionProof(ref spanBody, ref readBytes, out SurjectionProof signerGroupNameSurjectionProof);
                ReadCommitment(ref spanBody, ref readBytes, out byte[] allowedGroupCommitment);
                ReadSurjectionProof(ref spanBody, ref readBytes, out SurjectionProof allowedGroupNameSurjectionProof);

                block = new DocumentSignRequest
				{
                    EcdhTuple = ecdhTuple,
                    OwnershipProof = ownershipProofs,
                    EligibilityProof = eligibilityProofs,
                    SignerGroupRelationProof = signerGroupRelationProof,
                    //SignerGroupNameSurjectionProof = signerGroupNameSurjectionProof,
                    AllowedGroupCommitment = allowedGroupCommitment,
                    AllowedGroupNameSurjectionProof = allowedGroupNameSurjectionProof
				};

                StealthBase = block;

                return spanBody.Slice(readBytes);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
