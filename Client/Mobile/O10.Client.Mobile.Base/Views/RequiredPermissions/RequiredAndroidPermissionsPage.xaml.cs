using O10.Client.Mobile.Base.Aspects;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace O10.Client.Mobile.Base.Views.RequiredPermissions
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [Navigatable("RequiredAndroidPermissions")]
    public partial class RequiredAndroidPermissionsPage : ContentPage
    {
        public RequiredAndroidPermissionsPage()
        {
            InitializeComponent();
        }
    }
}