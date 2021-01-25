namespace O10.Client.Web.Portal.Dtos.ServiceProvider
{
    public class ServiceProviderRegistrationDto
    {
        public string ServiceProviderRegistrationId { get; set; }

        public string Commitment { get; set; }

        public SpUserTransactionDto[] Transactions { get; set; }
    }
}
