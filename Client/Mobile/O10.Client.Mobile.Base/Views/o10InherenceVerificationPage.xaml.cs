using O10.Client.Mobile.Base.Aspects;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace O10.Client.Mobile.Base.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [Navigatable("O10InherenceVerification")]
    public partial class O10InherenceVerificationPage : ContentPage
    {
        public O10InherenceVerificationPage()
        {
            InitializeComponent();
        }
    }
}