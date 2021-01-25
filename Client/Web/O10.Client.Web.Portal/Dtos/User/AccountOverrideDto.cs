namespace O10.Client.Web.Portal.Dtos.User
{
    public class AccountOverrideDto
    {
        public string Password { get; set; }
        public string SecretSpendKey { get; set; }
        public string SecretViewKey { get; set; }
        public ulong LastCombinedBlockHeight { get; set; }
    }
}
