using System;
using System.Buffers.Binary;
using O10.Transactions.Core.DataModel.Transactional;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Exceptions;
using O10.Core.Architecture;

using O10.Core.Cryptography;
using O10.Core.Identity;

namespace O10.Transactions.Core.Parsers.Transactional
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class DocumentSignRecordParser : TransactionalBlockParserBase
    {
        public DocumentSignRecordParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => ActionTypes.Transaction_DocumentSignRecord;

        protected override Memory<byte> ParseTransactional(ushort version, Memory<byte> spanBody, out TransactionalPacketBase transactionalBlockBase)
        {
            DocumentSignRecord block = null;

            if (version == 1)
            {
                int readBytes = 0;

                ReadCommitment(ref spanBody, ref readBytes, out byte[] documentHash);

				ulong recordHeight = BinaryPrimitives.ReadUInt64LittleEndian(spanBody.Slice(readBytes).Span);
				readBytes += sizeof(ulong);

				ReadCommitment(ref spanBody, ref readBytes, out byte[] keyImage);
				ReadCommitment(ref spanBody, ref readBytes, out byte[] signerCommitment);
                ReadSurjectionProof(ref spanBody, ref readBytes, out SurjectionProof eligibilityProof);
				ReadCommitment(ref spanBody, ref readBytes, out byte[] issuer);
                ReadSurjectionProof(ref spanBody, ref readBytes, out SurjectionProof signerGroupRelationProof);
				ReadCommitment(ref spanBody, ref readBytes, out byte[] groupIssuer);
				ReadCommitment(ref spanBody, ref readBytes, out byte[] signerGroupCommitment);
				ReadSurjectionProof(ref spanBody, ref readBytes, out SurjectionProof signerGroupProof);
				ReadSurjectionProof(ref spanBody, ref readBytes, out SurjectionProof signerAllowedGroupsProof);

				block = new DocumentSignRecord
				{
					DocumentHash = documentHash,
					RecordHeight = recordHeight,
                    KeyImage = keyImage,
					SignerCommitment = signerCommitment,
                    EligibilityProof = eligibilityProof,
                    Issuer = issuer,
					SignerGroupRelationProof = signerGroupRelationProof,
					GroupIssuer = groupIssuer,
					SignerGroupCommitment = signerGroupCommitment,
					SignerGroupProof = signerGroupProof,
					SignerAllowedGroupsProof = signerAllowedGroupsProof
                };

                transactionalBlockBase = block;
                return spanBody.Slice(readBytes);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
