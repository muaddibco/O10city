namespace O10.Client.Web.DataContracts.ServiceProvider
{
    public class ServiceProviderRegistrationDto
    {
        public long RegistrationId { get; set; }

        public string Commitment { get; set; }

        public SpUserTransactionDto[]? Transactions { get; set; }
    }
}
