using O10.Client.Mobile.Base.Aspects;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace O10.Client.Mobile.Base.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [Navigatable("IdentityDetails")]
    public partial class IdentityDetailsPage : ContentPage
    {
        public IdentityDetailsPage()
        {
            InitializeComponent();
        }
    }
}