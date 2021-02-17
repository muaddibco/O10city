using O10.Core.Models;

namespace O10.Transactions.Core.Ledgers.Registry
{
    public abstract class RegistryBlockBase : SignedPacketBase
    {
        public override ushort LedgerType => (ushort)Enums.LedgerType.Registry;
    }
}
