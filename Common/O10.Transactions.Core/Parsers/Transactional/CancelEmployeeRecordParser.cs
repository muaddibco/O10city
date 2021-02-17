using System;
using O10.Transactions.Core.Ledgers.O10State;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Exceptions;
using O10.Core;
using O10.Core.Architecture;

using O10.Core.Identity;

namespace O10.Transactions.Core.Parsers.Transactional
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class CancelEmployeeRecordParser : TransactionalBlockParserBase
    {
        public CancelEmployeeRecordParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => PacketTypes.Transaction_CancelEmployeeRecord;

        protected override Memory<byte> ParseTransactional(ushort version, Memory<byte> spanBody, out TransactionalPacketBase transactionalBlockBase)
        {
            if (version == 1)
            {
                int readBytes = 0;
                
                byte[] registrationCommitment = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                CancelEmployeeRecord block = new CancelEmployeeRecord
                {
                    RegistrationCommitment = registrationCommitment,
                };

                transactionalBlockBase = block;
                return spanBody.Slice(readBytes);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
