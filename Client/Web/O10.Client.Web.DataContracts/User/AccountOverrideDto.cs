namespace O10.Client.Web.DataContracts.User
{
    public class AccountOverrideDto
    {
        public string Password { get; set; }
        public string SecretSpendKey { get; set; }
        public string SecretViewKey { get; set; }
        public long LastCombinedBlockHeight { get; set; }
    }
}
