using O10.Client.Mobile.Base.Aspects;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace O10.Client.Mobile.Base.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [Navigatable("AccountCompromised")]
    public partial class AccountCompromisedPage : ContentPage
    {
        public AccountCompromisedPage()
        {
            InitializeComponent();
        }
    }
}