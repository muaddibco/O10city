using System;
using System.IO;
using O10.Transactions.Core.Ledgers.O10State;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;


namespace O10.Transactions.Core.Serializers.Signed.Transactional
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.Transient)]
    public class EmployeeRecordSerializer : TransactionalSerializerBase<EmployeeRecord>
    {
        public EmployeeRecordSerializer(IServiceProvider serviceProvider) : base(serviceProvider, LedgerType.O10State, PacketTypes.Transaction_EmployeeRecord)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            base.WriteBody(bw);

            bw.Write(_block.RegistrationCommitment);
            bw.Write(_block.GroupCommitment);
        }
    }
}
