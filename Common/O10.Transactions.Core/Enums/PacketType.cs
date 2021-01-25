namespace O10.Transactions.Core.Enums
{
    /// <summary>
    /// Enum defines the storage where the packet will be stored
    /// </summary>
    public enum PacketType : ushort
    {
        Transactional = 1,
        Stealth = 2,

        Registry = ushort.MaxValue - 1,
        Synchronization = ushort.MaxValue
    }
}
