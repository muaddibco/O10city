namespace O10.Client.Web.DataContracts.ServiceProvider
{
    public class SpUserTransactionDto
    {
        public long SpUserTransactionId { get; set; }

        public long RegistrationId { get; set; }

        public string TransactionKey { get; set; }
        public string Description { get; set; }

        public bool IsProcessed { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsCompromised { get; set; }
    }
}
