using System.IO;
using O10.Client.Mobile.Base.Interfaces;
using Xamarin.Forms;

[assembly: Dependency(typeof(O10Wallet.Droid.Services.ConnectionStringProvider))]
namespace O10Wallet.Droid.Services
{
    public class ConnectionStringProvider : IConnectionStringProvider
    {
        public string GetConnectionString(string databaseName)
        {
            return Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), databaseName);
        }
    }
}