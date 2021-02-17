namespace O10.Transactions.Core.Enums
{
    /// <summary>
    /// Enum defines the storage where the packet will be stored
    /// </summary>
    public enum LedgerType : ushort
    {
        O10State = 1,
        Stealth = 2,

        Registry = ushort.MaxValue - 1,
        Synchronization = ushort.MaxValue
    }
}
