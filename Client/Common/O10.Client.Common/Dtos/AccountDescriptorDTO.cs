namespace O10.Client.Common.Dtos
{
    public class AccountDescriptorDTO
    {
        public byte[] SecretSpendKey { get; set; }
        public byte[] PublicSpendKey { get; set; }
        public byte[] SecretViewKey { get; set; }
        public byte[] PublicViewKey { get; set; }
        public AccountTypeDTO AccountType { get; set; }
        public string AccountInfo { get; set; }
        public byte[] PwdHash { get; set; }
        public long AccountId { get; set; }
        public bool IsCompromised { get; set; }
        public long LastAggregatedRegistrations { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsActive { get; set; }
    }
}
