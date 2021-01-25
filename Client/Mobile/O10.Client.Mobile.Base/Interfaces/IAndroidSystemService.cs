using System.Threading.Tasks;

namespace O10.Client.Mobile.Base.Interfaces
{
    public interface IAndroidSystemService
    {
        bool IsAutoStartPermissionAvailable();
        void OpenAutoStartSettings();
        Task<bool> OpenOverflowSettings();
        bool IsOverflowSettingsAllowed();
    }
}
