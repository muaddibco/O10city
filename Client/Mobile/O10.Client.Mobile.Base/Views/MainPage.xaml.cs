using O10.Client.Mobile.Base.Aspects;
using Xamarin.Forms.Xaml;

namespace O10.Client.Mobile.Base.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [Navigatable("MainPage")]
    public partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
    }
}