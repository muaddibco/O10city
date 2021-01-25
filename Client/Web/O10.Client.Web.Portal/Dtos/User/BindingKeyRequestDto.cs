namespace O10.Client.Web.Portal.Dtos.User
{
    public class BindingKeyRequestDto
    {
        public long AccountId { get; set; }
        public string Password { get; set; }

        public bool Force { get; set; }
    }
}
