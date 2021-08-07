namespace O10.Client.Web.DataContracts.User
{
    public class UserActionInfoDto
    {
        public UserActionTypeDto ActionType { get; set; }

        public string ActionInfoEncoded { get; set; }
    }
}
