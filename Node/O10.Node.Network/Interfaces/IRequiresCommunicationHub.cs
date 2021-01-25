namespace O10.Network.Interfaces
{
    public interface IRequiresCommunicationHub
    {
        void RegisterCommunicationHub(IServerCommunicationService communicationHub);
    }
}
