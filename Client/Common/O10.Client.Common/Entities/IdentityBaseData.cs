namespace O10.Client.Common.Entities
{
    public partial class IdentityBaseData
	{
        public IssuanceProtection Protection { get; set; }

        public string Content { get; set; }
		public string ImageContent { get; set; }
        public string BlindingPoint { get; set; }

        public string PublicSpendKey { get; set; }

		public string PublicViewKey { get; set; }
	}
}
