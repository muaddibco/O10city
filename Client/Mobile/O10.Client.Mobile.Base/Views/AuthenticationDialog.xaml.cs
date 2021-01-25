using O10.Client.Mobile.Base.Aspects;
using Xamarin.Forms.Xaml;

namespace O10.Client.Mobile.Base.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [Navigatable("Authentication")]
    public partial class AuthenticationDialog
    {
        public AuthenticationDialog()
        {
            InitializeComponent();
        }
    }
}