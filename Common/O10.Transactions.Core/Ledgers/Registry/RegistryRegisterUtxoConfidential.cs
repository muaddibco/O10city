//using O10.Transactions.Core.Ledgers.Stealth;
//using O10.Transactions.Core.Enums;

//namespace O10.Transactions.Core.Ledgers.Registry
//{
//    public class RegistryRegisterStealth : StealthBase
//    {
//        public override ushort LedgerType => (ushort)Enums.LedgerType.Registry;

//        public override ushort Version => 1;

//        public override ushort TransactionType => TransactionTypes.Registry_RegisterStealth;

//        public LedgerType ReferencedLedgerType { get; set; }

//        public ushort ReferencedBlockType { get; set; }

//		public byte[] ReferencedBodyHash { get; set; }
//    }
//}
