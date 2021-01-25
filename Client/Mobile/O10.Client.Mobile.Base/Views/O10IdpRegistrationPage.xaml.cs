using O10.Client.Mobile.Base.Aspects;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace O10.Client.Mobile.Base.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [Navigatable("O10IdpRegistration")]
    public partial class O10IdpRegistrationPage : ContentPage
    {
        public O10IdpRegistrationPage()
        {
            InitializeComponent();
        }
    }
}