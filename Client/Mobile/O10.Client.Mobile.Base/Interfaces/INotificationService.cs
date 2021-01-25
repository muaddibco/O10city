namespace O10.Client.Mobile.Base.Interfaces
{
    public interface INotificationService
    {
        void ShowMessage(string msg, bool longMessage = false, bool asyncCall = false);
    }
}
