using O10.Client.Mobile.Base.Aspects;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace O10.Client.Mobile.Base.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [Navigatable("AccountCreation")]
    public partial class AccountCreationPage : ContentPage
    {
        public AccountCreationPage()
        {
            InitializeComponent();
        }
    }
}