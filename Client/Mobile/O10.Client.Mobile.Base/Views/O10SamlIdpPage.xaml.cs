using O10.Client.Mobile.Base.Aspects;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace O10.Client.Mobile.Base.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [Navigatable("O10SamlIdp")]
    public partial class O10SamlIdpPage : ContentPage
    {
        public O10SamlIdpPage()
        {
            InitializeComponent();
        }
    }
}