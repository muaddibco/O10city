using O10.Transactions.Core.Ledgers.Registry;
using O10.Transactions.Core.Ledgers.Synchronization;

namespace O10.Transactions.Core.DTOs
{
    public class RtPackage
    {
        public SynchronizationPacket? AggregatedRegistrations { get; set; }
        public RegistryPacket? FullRegistrations { get; set; }
    }
}
