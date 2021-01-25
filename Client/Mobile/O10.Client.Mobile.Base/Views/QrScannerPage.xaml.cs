using O10.Client.Mobile.Base.Aspects;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace O10.Client.Mobile.Base.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [Navigatable("QrScanner")]
    public partial class QrScannerPage
    {
        public QrScannerPage()
        {
            InitializeComponent();
            Overlay.ShowFlashButton = Scanner.HasTorch;
            Scanner.Options.TryHarder = true;
            Scanner.Options.UseNativeScanning = true;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Scanner.IsScanning = true;
        }

        protected override void OnDisappearing()
        {
            Scanner.IsScanning = false;
            base.OnDisappearing();
        }

        private void Overlay_FlashButtonClicked(Button sender, System.EventArgs e)
        {
            Scanner.ToggleTorch();
        }
    }
}