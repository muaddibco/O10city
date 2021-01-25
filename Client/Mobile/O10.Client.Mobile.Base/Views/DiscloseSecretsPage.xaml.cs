using O10.Client.Mobile.Base.Aspects;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace O10.Client.Mobile.Base.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [Navigatable("DiscloseSecrets")]
    public partial class DiscloseSecretsPage : ContentPage
    {
        public DiscloseSecretsPage()
        {
            InitializeComponent();
#pragma warning disable CS0168 // The variable 'v' is declared but never used
            ZXing.Net.Mobile.Forms.ZXingBarcodeImageView v;
#pragma warning restore CS0168 // The variable 'v' is declared but never used

        }
    }
}