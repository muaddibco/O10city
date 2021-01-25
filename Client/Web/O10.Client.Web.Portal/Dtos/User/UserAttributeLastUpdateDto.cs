namespace O10.Client.Web.Portal.Dtos.User
{
    public class UserAttributeLastUpdateDto
    {
        public string Issuer { get; set; }

        public string AssetId { get; set; }

        public string LastBlindingFactor { get; set; }

        public string LastCommitment { get; set; }

        public string LastTransactionKey { get; set; }

        public string LastDestinationKey { get; set; }
    }
}
