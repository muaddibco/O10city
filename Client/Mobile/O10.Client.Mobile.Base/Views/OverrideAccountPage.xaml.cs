using O10.Client.Mobile.Base.Aspects;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace O10.Client.Mobile.Base.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [Navigatable("OverrideAccount")]
    public partial class OverrideAccountPage : ContentPage
    {
        public OverrideAccountPage()
        {
            InitializeComponent();
        }
    }
}