namespace O10.Client.Common.Dtos
{
    public partial class IdentityBaseDataDTO
    {
        public IssuanceProtectionDTO? Protection { get; set; }

        public string? Content { get; set; }
        public string? ImageContent { get; set; }
        public string? BlindingPoint { get; set; }

        public string? PublicSpendKey { get; set; }

        public string? PublicViewKey { get; set; }
    }
}
