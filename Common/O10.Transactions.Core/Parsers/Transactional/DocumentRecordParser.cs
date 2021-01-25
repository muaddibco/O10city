using System;
using System.Buffers.Binary;
using O10.Transactions.Core.DataModel.Transactional;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Exceptions;
using O10.Core.Architecture;

using O10.Core.Identity;

namespace O10.Transactions.Core.Parsers.Transactional
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class DocumentRecordParser : TransactionalBlockParserBase
    {
        public DocumentRecordParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => ActionTypes.Transaction_DocumentRecord;

        protected override Memory<byte> ParseTransactional(ushort version, Memory<byte> spanBody, out TransactionalPacketBase transactionalBlockBase)
        {
            DocumentRecord block = null;

            if (version == 1)
            {
                int readBytes = 0;

				ReadCommitment(ref spanBody, ref readBytes, out byte[] documentHash);

				ushort allowedSignerGroupCommitmentsCount = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Slice(readBytes).Span);
				readBytes += sizeof(ushort);

				byte[][] allowedSignerGroupCommitments = new byte[allowedSignerGroupCommitmentsCount][];

				for (int i = 0; i < allowedSignerGroupCommitmentsCount; i++)
				{
					ReadCommitment(ref spanBody, ref readBytes, out byte[] allowedSignerGroupCommitment);
					allowedSignerGroupCommitments[i] = allowedSignerGroupCommitment;
				}

				block = new DocumentRecord
				{
					DocumentHash = documentHash,
					AllowedSignerGroupCommitments = allowedSignerGroupCommitments
                };

                transactionalBlockBase = block;
                return spanBody.Slice(readBytes);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
