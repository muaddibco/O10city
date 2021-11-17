using O10.Client.Common.Dtos;

namespace O10.Client.Web.DataContracts
{
    public class AccountDto
    {
        public long AccountId { get; set; }
        public AccountTypeDTO AccountType { get; set; }
        public string AccountInfo { get; set; }
        public string Password { get; set; }

        public string? PublicViewKey { get; set; }
        public string? PublicSpendKey { get; set; }
    }
}
