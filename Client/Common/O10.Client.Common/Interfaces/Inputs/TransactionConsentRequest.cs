namespace O10.Client.Common.Interfaces.Inputs
{
    public class TransactionConsentRequest : ProofsRequest
    {
        public string RegistrationCommitment { get; set; }
        public string TransactionId { get; set; }
        public string Description { get; set; }
        public string PublicSpendKey { get; set; }
        public string PublicViewKey { get; set; }
    }
}
