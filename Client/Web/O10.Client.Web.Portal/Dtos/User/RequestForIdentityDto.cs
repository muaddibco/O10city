namespace O10.Client.Web.Portal.Dtos.User
{
    public class RequestForIdentityDto
    {
        public string Target { get; set; }

        public string IdCardContent { get; set; }

        public string Passphrase { get; set; }

        public string Password { get; set; }

        public string ImageContent { get; set; }
    }
}
