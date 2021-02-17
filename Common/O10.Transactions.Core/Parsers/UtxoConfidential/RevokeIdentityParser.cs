using System;
using O10.Transactions.Core.Ledgers.Stealth;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Exceptions;
using O10.Core.Architecture;

using O10.Core.Cryptography;
using O10.Core.Identity;

namespace O10.Transactions.Core.Parsers.Stealth
{
	[RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class RevokeIdentityParser : StealthTransactionParserBase
	{
        public RevokeIdentityParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => PacketTypes.Stealth_RevokeIdentity;

        protected override Memory<byte> ParseStealthTransaction(ushort version, Memory<byte> spanBody, out StealthTransactionBase StealthBase)
        {
            if (version == 1)
            {
                int readBytes = 0;

				ReadSurjectionProof(ref spanBody, ref readBytes, out SurjectionProof ownershipProofs);
				ReadSurjectionProof(ref spanBody, ref readBytes, out SurjectionProof eligibilityProofs);

                RevokeIdentity block = new RevokeIdentity
				{
					OwnershipProof = ownershipProofs,
                    EligibilityProof = eligibilityProofs,
                };

                StealthBase = block;

                return spanBody.Slice(readBytes);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
