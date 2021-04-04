//using O10.Transactions.Core.Enums;

//namespace O10.Transactions.Core.Ledgers.Registry.Transactions
//{
//    public class ShortRegistryTransaction : RegistryTransactionBase
//    {
//        public override ushort TransactionType => TransactionTypes.Registry_ShortBlock;

//        public WitnessStateKey[] WitnessStateKeys { get; set; }
//        public WitnessUtxoKey[] WitnessUtxoKeys { get; set; }

//        public bool Equals(ShortRegistryTransaction x, ShortRegistryTransaction y)
//        {
//            if (x != null && y != null)
//            {
//                return x.SyncHeight == y.SyncHeight && x.Height == y.Height && x.Source.Equals(y.Source);
//            }

//            return x == null && y == null;
//        }

//        public int GetHashCode(ShortRegistryTransaction obj)
//        {
//            int hash = obj.SyncHeight.GetHashCode() ^ obj.Height.GetHashCode() ^ obj.Source.GetHashCode();

//            hash += hash << 13;
//            hash ^= hash >> 7;
//            hash += hash << 3;
//            hash ^= hash >> 17;
//            hash += hash << 5;

//            return hash;
//        }
//    }
//}
