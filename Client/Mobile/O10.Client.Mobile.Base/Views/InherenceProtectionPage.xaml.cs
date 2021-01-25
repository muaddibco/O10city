using O10.Client.Mobile.Base.Aspects;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace O10.Client.Mobile.Base.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [Navigatable("InherenceProtection")]
    public partial class InherenceProtectionPage : ContentPage
    {
        public InherenceProtectionPage()
        {
            InitializeComponent();
        }
    }
}