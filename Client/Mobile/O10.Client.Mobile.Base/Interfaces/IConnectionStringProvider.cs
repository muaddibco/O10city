namespace O10.Client.Mobile.Base.Interfaces
{
    public interface IConnectionStringProvider
    {
        string GetConnectionString(string dbFileName);
    }
}
