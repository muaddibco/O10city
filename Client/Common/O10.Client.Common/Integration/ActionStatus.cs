namespace O10.Client.Common.Integration
{
    public class ActionStatus
    {
        public string IntegrationType { get; set; }
        public string IntegrationAction { get; set; }
        public string IntegrationAddress { get; set; }
        public bool ActionSucceeded { get; set; }
        public string ErrorMsg { get; set; }
    }
}