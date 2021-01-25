namespace O10.Client.Web.Portal.Dtos.User
{
    public class UserAttributeTransferDto : UserAttributeDto
    {
        public string Target { get; set; }

        public string Target2 { get; set; }

        public string Payload { get; set; }

        public string ExtraInfo { get; set; }

        public string ImageContent { get; set; }

        public string Password { get; set; }
    }
}
