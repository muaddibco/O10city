using O10.Client.DataLayer.Enums;

namespace O10.Client.Web.Portal.Dtos
{
    public class AccountDto
    {
        public long AccountId { get; set; }
        public AccountType AccountType { get; set; }
        public string AccountInfo { get; set; }
        public string Password { get; set; }

        public string? PublicViewKey { get; set; }
        public string? PublicSpendKey { get; set; }
    }
}
