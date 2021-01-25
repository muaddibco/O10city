using O10.Client.Mobile.Base.Aspects;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace O10.Client.Mobile.Base.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [Navigatable("EmbeddedIdps")]
    public partial class EmbeddedIdpsPage : ContentPage
    {
        public EmbeddedIdpsPage()
        {
            InitializeComponent();
        }
    }
}